using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class InteractiveCore : BaseRepository<Core_Interactive>
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        /// <summary>
        /// 邻友圈 首页 - 社区互动列表
        /// </summary>
        /// <param name="buildingId">社区ID（为null，标识全国）</param>
        /// <param name="pn">页码</param>
        /// <param name="rows">条数</param>
        /// <returns></returns>
        public List<ActiveIndexViewModel> GetLinyouActives(Guid? buildingId, int pn, int rows = 5)
        {
            string sql = @"select top {1} * from (
                            SELECT row_number() over(order by a.I_Top desc,a.I_Recom desc ,a.I_Date desc) as rownumber, a.I_Id, a.I_UserId, a.I_Title, a.I_Flag, a.I_Content as I_Remark, a.I_Img, a.I_Hongbao, a.I_Date, 
                            a.I_Likes, a.I_Comments, a.I_Clicks, a.I_Tags, a.I_Type,b.U_Logo,b.U_NickName,b.U_Sex
                            FROM Core_Interactive as a
                             inner  JOIN Core_User  as b
                             on b.U_Id = a.I_UserId
                             where {0} and a.I_Status = 0
                            ) temp
                             where rownumber > {2}";

            string where = buildingId == null ? "1=1" : "a.I_VillageId='" + buildingId + "'";

            using (DbContext db = new LinXinApp20Entities())
            {
                var rs =
                    db.Database.SqlQuery<ActiveIndexViewModel>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();
                return rs;
            }
            return null;
        }


        /// <summary>
        /// 用户添加社区互动
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="hb"></param>
        /// <param name="phone"></param>
        /// <param name="voteList"></param>
        /// <returns></returns>
        public ResultMessageViewModel AddOne(Core_Interactive obj, Core_LuckyGift hb, string phone, List<Core_EventVoteItem> voteList = null, string enPaypwd = "")
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                if (hb != null)  //有红包，有红包就要扣钱咯（代金券的金额,不能用做红包）
                {
                    Core_Balance balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == obj.I_UserId);//  得到会员


                    if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                    {
                        rs.State = 2;
                        rs.Msg = "您还未设置支付密码,请立即前去设置！";
                        return rs;
                    }
                    else
                    {
                        string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                        payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// 加密支付密码
                        if (!balance.B_PayPwd.Equals(payPwd))
                        {
                            rs.Msg = "支付密码错误！";
                            return rs;
                        }
                    }

                    if (balance.B_Balance < hb.LG_Money)
                    {
                        rs.Msg = "余额不足，请充值";
                        return rs;
                    }
                    else  // 可以发红包
                    {
                        //todo:用户发红包要扣除余额，目前是没有要求输入支付密码，后期可能会扩展
                        //1添加红包
                        db.Entry<Core_LuckyGift>(hb).State = EntityState.Added;
                        //2扣余额
                        balance.B_Balance = balance.B_Balance - hb.LG_Money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry<Core_Balance>(balance).State = EntityState.Modified;
                        //3添加账单流水
                        Core_BillMember bill = new Core_BillMember() { B_Flag = 0, B_Module = (int)BillEnum.红包, B_Money = hb.LG_Money, B_OrderId = hb.LG_Id, B_Phone = phone, B_Remark = "", B_Status = 0, B_Time = hb.LG_CreateTime, B_Title = "用户发社区互动，附红包", B_UId = hb.LG_UserId, B_Type = (int)MemberRoleEnum.会员 };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                    }
                }

                if (voteList != null)  //有投票项目
                {
                    foreach (var vote in voteList)
                    {
                        //4添加投票项
                        db.Entry<Core_EventVoteItem>(vote).State = EntityState.Added;
                    }
                }
                //添加互动
                db.Entry<Core_Interactive>(obj).State = EntityState.Added;  //5添加互动


                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = null;
                }
            }

            return rs;
        }




        /// <summary>
        /// 查看的社区互动
        /// </summary>
        /// <param name="activeId">社区互动ID</param>
        /// <param name="uid">用户的ID</param>
        /// <returns></returns>
        public ResultMessageViewModel View(Guid activeId, Guid uid)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var info = db.Set<Core_Interactive>().FirstOrDefault(o => o.I_Id == activeId);

                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);

                dynamic gift = null;  //红包
                if (info.I_Hongbao != (int)LuckGiftFlagEnum.没有红包) //有红包，就分红包呗
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == activeId);
                    var recHis =
                        db.Set<Core_LuckyGiftHistory>()
                            .FirstOrDefault(o => o.LH_UserId == uid && o.LH_GiftId == hb.LG_Id);  //领取的历史记录

                    decimal money = -1;  //抢得的红包金额
                    if (hb != null)//红包互动
                    {

                        if (hb.LG_RemainCount > 0 && recHis == null) //还未领取过红包，且红包还有剩余
                        {

                            //todo:领了社区红包要不要推送个消息呢？
                            if (hb.LG_RemainCount == 1)//只有一个，就全部给这个人
                            {
                                money = hb.LG_RemainMoney;
                            }
                            else
                            {
                                money = decimal.Round(
                                      Convert.ToDecimal(giftCore.CalcGift(Convert.ToDouble(hb.LG_RemainMoney),
                                          hb.LG_RemainCount)), 2);
                            }

                            hb.LG_RemainCount -= 1;
                            hb.LG_RemainMoney -= money;

                            //1.变更红包余额、个数
                            db.Set<Core_LuckyGift>().Attach(hb);
                            db.Entry(hb).State = EntityState.Modified;

                            //添加抢得红包的用户 红包记录
                            Guid hisID = Guid.NewGuid();
                            Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                            {
                                LH_CreateTime = DateTime.Now,
                                LH_GiftDetailId = null,
                                LH_GiftId = hb.LG_Id,
                                LH_Id = hisID,
                                LH_Money = money,
                                LH_Remark = string.Format("领取社区互动：“{0}” 红包", info.I_Title),
                                LH_Status = 0,
                                LH_UserId = uid,
                                LH_UserNickName = user.U_NickName,
                                LH_UserPhone = user.U_LoginPhone,
                                LH_Flag = (int)LuckGiftTypeEnum.邻友圈用户红包,
                                LH_UserLogo = user.U_Logo
                            };
                            // 2. 添加领取的红包记录
                            db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;

                            //变更领取者 余额
                            var recUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);

                            var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                            balance.B_Balance += money;
                            db.Set<Core_Balance>().Attach(balance);
                            db.Entry(balance).State = EntityState.Modified;   //3.累计到余额


                            //添加会员账单
                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Remark = "领取社区互动：" + info.I_Title,
                                B_Money = money,
                                B_UId = user.U_Id,
                                B_Flag = (int)BillFlagEnum.普通流水,
                                B_Module = (int)BillEnum.红包,
                                B_OrderId = hisID,
                                B_Phone = user.U_LoginPhone,
                                B_Status = 0,
                                B_Title = "评论社区互动，获得用户红包",
                                B_Type = (int)MemberRoleEnum.会员
                            };
                            db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 添加评论者的红包账单

                            if (hb.LG_RemainCount == 0) //5红包被领光了,更改话题的红包标识
                            {
                                info.I_Hongbao = (int)LuckGiftFlagEnum.红包被领光;
                            }
                        }
                        else if (recHis != null)  //已经领取过红包了
                        {
                            money = recHis.LH_Money;
                        }

                        gift = new
                        {
                            gid = hb.LG_Id,
                            gTitle = hb.LG_Title,
                            money = money.ToString("N")
                        };
                    }



                }//红包结束
                dynamic voteItem = null;  //投票互动列表
                if (info.I_Flag == (int)InteractiveFlagEnum.投票互动)  //投票互动
                {
                    var voteHis = db.Core_EventJoinHistory.Where(o => o.H_EventId == activeId).ToList();
                    voteItem =
                        db.Set<Core_EventVoteItem>()
                            .Where(o => o.I_EventId == info.I_Id && o.I_State == 0)
                            .OrderBy(o => o.I_Rank).ToList()
                            .Select(o => new
                            {
                                voteId = o.I_Id,
                                title = o.I_Title,
                                count = o.I_Count,
                                flag = voteHis.Any(c => c.H_UserId == uid && c.H_VoteId == o.I_Id) ? 1 : 0
                            }).ToList();
                }
                info.I_Clicks += 1;  //累计浏览量
                db.Set<Core_Interactive>().Attach(info);
                db.Entry(info).State = EntityState.Modified;

                if (db.SaveChanges() > 0)
                {
                    var intUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == info.I_UserId);
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        eventId = info.I_Id,
                        title = info.I_Title,
                        nickname = intUser.U_NickName,
                        logo = intUser.U_Logo,
                        coverImg = info.I_Img,
                        content = info.I_Content,
                        time = info.I_Date,
                        phone = intUser.U_LoginPhone,
                        voteFlag = info.I_Flag,
                        voteList = voteItem,
                        chatId = intUser.U_ChatID,
                        gift
                    };
                }

            }
            return rs;
        }

    }
}
