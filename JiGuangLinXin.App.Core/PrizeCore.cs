using System;
using System.Collections.Generic;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class PrizeCore : BaseRepository<Core_Prize>
    {
    }

    public class PrizeDetailCore : BaseRepository<Core_PrizeDetail>
    {
        /// <summary>
        /// 获取 星际大冲关的 总统计信息
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <returns></returns>
        public int JoinCountByBuildingId(Guid buildingId)
        {

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                return
                    db.Core_PrizeDetail.Join(db.Core_User, a => a.PD_UId, b => b.U_Id,
                        (a, b) => new { bId = b.U_BuildingId }).Where(o => o.bId == buildingId).GroupBy(o => o.bId).Count();
            }
            return 0;
        }

        /// <summary>
        /// 获取 星际大冲关 每一项中奖详情
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public List<dynamic> JoinCountDetailByBuildingId(Guid buildingId)
        {

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var info = db.Core_PrizeDetail.Join(db.Core_User, a => a.PD_UId, b => b.U_Id,
                        (a, b) => new { bId = b.U_BuildingId, item = a.PD_Award }).Where(o => o.bId == buildingId).ToList().Select(o => new { item = o.item.Substring(0, 1) }).GroupBy(o => o.item).Select(o => new
                        {
                            item = o.Key,
                            count = o.Count()
                        });


                int i0 = 0;
                int i1 = 0;
                int i2 = 0;
                int i3 = 0;
                int i4 = 0;

                foreach (var temp in info)
                {
                    if (temp.item == "4" || temp.item == "7")
                    {
                        i0 += temp.count;
                    }

                    if (temp.item == "5")
                    {
                        i1 += temp.count;

                    }

                    if (temp.item == "0" || temp.item == "1" || temp.item == "6")
                    {
                        i2 += temp.count;
                    }

                    if (temp.item == "3")
                    {
                        i4 += temp.count;
                    }
                }
                List<dynamic> list = new List<dynamic>()
                {
                    new {item = 0,count =i0,title = "红包中奖人数"},
                    new {item = 1,count = i1,title = "VR眼镜中奖人数"},
                    new {item = 2,count = i2,title = "业主卡中奖人数"},
                    new {item = 3,count = i3,title = "iPhone中奖人数"},
                    new {item = 4,count = i4,title = "未中奖人数"}
                };
                return list;
            }
            return null;
        }




        //活动已经过期了

        ///// <summary>
        ///// 业主登录成功，返还已中奖励
        ///// </summary>
        ///// <param name="phone"></param>
        //public void BackAward(string phone)
        //{
        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        //添加账单
        //        var user = db.Core_User.FirstOrDefault(o => o.U_LoginPhone == phone);
        //        Guid uid = user.U_Id;
        //        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == uid);

        //        if (balance == null)
        //        {
        //            return;
        //        }
        //        var prList = db.Core_PrizeDetail.Where(o => o.PD_UPhone == phone && o.PD_Flag == 0 && (o.PD_LuckGift > 0 || o.PD_OwnerCard > 0));

        //        foreach (var info in prList)
        //        {
        //            //todo: 考虑兑奖 过期

        //            if (info.PD_OwnerCard > 0)
        //            {
        //                Core_BillMember bill = new Core_BillMember()
        //                {
        //                    B_Flag = 0,
        //                    B_Module = (int)BillEnum.平台业主卡,
        //                    B_Money = info.PD_OwnerCard,
        //                    B_OrderId = info.PD_Id,
        //                    B_Phone = info.PD_UPhone,
        //                    B_Remark = "“九宫格”大抽奖，获得业主卡",
        //                    B_Status = 0,
        //                    B_Time = info.PD_Time,
        //                    B_Title = string.Format("抽奖获得业主卡{0}元", info.PD_OwnerCard.ToString("N")),
        //                    B_UId = uid,
        //                    B_Type = (int)MemberRoleEnum.会员
        //                };
        //                db.Core_BillMember.Add(bill);
        //                balance.B_CouponMoney += info.PD_OwnerCard;  //累计余额


        //                info.PD_Flag = 1;  //变更中奖状态
        //                info.PD_TimeAward = DateTime.Now;
        //                info.PD_UId = uid;
        //            }
        //            else if (info.PD_LuckGift > 0)
        //            {

        //                Core_BillMember bill = new Core_BillMember()
        //                {
        //                    B_Flag = 0,
        //                    B_Module = (int)BillEnum.红包,
        //                    B_Money = info.PD_LuckGift,
        //                    B_OrderId = info.PD_Id,
        //                    B_Phone = info.PD_UPhone,
        //                    B_Remark = "“九宫格”大抽奖,获得红包",
        //                    B_Status = 0,
        //                    B_Time = info.PD_Time,
        //                    B_Title = string.Format("抽奖获得红包{0}元", info.PD_LuckGift.ToString("N")),
        //                    B_UId = uid,
        //                    B_Type = (int)MemberRoleEnum.会员
        //                };
        //                db.Core_BillMember.Add(bill);

        //                balance.B_Balance += info.PD_LuckGift;//累计余额


        //                info.PD_Flag = 1;  //变更中奖状态
        //                info.PD_TimeAward = DateTime.Now;
        //                info.PD_UId = uid;
        //            }

        //            //info.PD_Id = Guid.NewGuid();
        //            //info.PD_UId = user.U_Id;
        //            //info.PD_UPhone = user.U_LoginPhone;
        //            //info.PD_Flag = 1;
        //            //info.PD_Time = DateTime.Now; 

        //            //db.Core_PrizeDetail.Add(info);  

        //            Sys_OperateLog log = new Sys_OperateLog() { L_Desc = "参加官方“九宫格”抽奖活动分享；奖励返还", L_DriverType = 3, L_Flag = (int)ModuleEnum.九宫格抽奖, L_Phone = info.PD_UPhone, L_UId = info.PD_UId, L_Url = "/Prize/Lottery", L_Status = 0, L_Time = DateTime.Now };

        //            db.Sys_OperateLog.Add(log);  //添加日志

        //        }
        //        db.SaveChanges();
        //    }
        //}

        ///// <summary>
        ///// App内部抽奖
        ///// </summary>
        ///// <param name="uid">用户ID</param>
        ///// <param name="info">获奖详情</param>
        ///// <returns></returns>
        //public ResultMessageViewModel TurnInside(Guid uid, Core_PrizeDetail info)
        //{
        //    ResultMessageViewModel rs = new ResultMessageViewModel();
        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid);
        //        if (user == null)
        //        {
        //            rs.Msg = "用户不存在";
        //            return rs;
        //        }


        //        //info.PD_Id = Guid.NewGuid();

        //        //var prize = db.Core_Prize.FirstOrDefault(o => o.P_UId == uid);
        //        //if (prize == null)
        //        //{
        //        //    var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid);
        //        //    if (user == null)
        //        //    {
        //        //        rs.Msg = "用户不存在";
        //        //        return rs;
        //        //    }


        //        //    prize = new Core_Prize()
        //        //    {
        //        //        P_Count = 1,
        //        //        P_Id = Guid.NewGuid(),
        //        //        P_RemainCount = 0,
        //        //        P_Time = DateTime.Now,
        //        //        P_TimeUseful = DateTime.Now.AddDays(60),
        //        //        P_TypeId = 1,
        //        //        P_UId = user.U_Id,
        //        //        P_UPhone = user.U_LoginPhone
        //        //    };

        //        //    db.Core_Prize.Add(prize);  //添加中奖记录i，但是已经领取奖品了
        //        //}
        //        //else
        //        //{
        //        //    prize.P_RemainCount -= 1; //减少一次抽奖机会了
        //        //}

        //        var pdInfo = db.Core_PrizeDetail.FirstOrDefault(o => o.PD_Id == info.PD_Id);
        //        if (pdInfo == null || pdInfo.PD_TimeUseful < DateTime.Now)
        //        {
        //            rs.Msg = "兑奖失败";
        //            return rs;

        //        }

        //        //添加账单
        //        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
        //        if (balance == null)
        //        {
        //            rs.Msg = "用户账户不存在";
        //            return rs;
        //        }
        //        if (info.PD_OwnerCard > 0)
        //        {
        //            Core_BillMember bill = new Core_BillMember()
        //            {
        //                B_Flag = 0,
        //                B_Module = (int)BillEnum.平台业主卡,
        //                B_Money = info.PD_OwnerCard,
        //                B_OrderId = info.PD_Id,
        //                B_Phone = info.PD_UPhone,
        //                B_Remark = "“九宫格”大抽奖，获得业主卡",
        //                B_Status = 0,
        //                B_Time = info.PD_Time,
        //                B_Title = string.Format("抽奖获得业主卡{0}元", info.PD_OwnerCard.ToString("N")),
        //                B_UId = uid,
        //                B_Type = (int)MemberRoleEnum.会员
        //            };
        //            db.Core_BillMember.Add(bill);
        //            balance.B_CouponMoney += info.PD_OwnerCard;  //累计余额


        //            pdInfo.PD_Flag = 1;  //变更中奖状态
        //            pdInfo.PD_TimeAward = DateTime.Now;
        //        }
        //        else if (info.PD_LuckGift > 0)
        //        {

        //            Core_BillMember bill = new Core_BillMember()
        //            {
        //                B_Flag = 0,
        //                B_Module = (int)BillEnum.红包,
        //                B_Money = info.PD_LuckGift,
        //                B_OrderId = info.PD_Id,
        //                B_Phone = info.PD_UPhone,
        //                B_Remark = "“九宫格”大抽奖,获得红包",
        //                B_Status = 0,
        //                B_Time = info.PD_Time,
        //                B_Title = string.Format("抽奖获得红包{0}元", info.PD_LuckGift.ToString("N")),
        //                B_UId = uid,
        //                B_Type = (int)MemberRoleEnum.会员
        //            };
        //            db.Core_BillMember.Add(bill);

        //            balance.B_Balance += info.PD_LuckGift;//累计余额


        //            pdInfo.PD_Flag = 1;  //变更中奖状态
        //            pdInfo.PD_TimeAward = DateTime.Now;
        //        }

        //        //info.PD_Id = Guid.NewGuid();
        //        //info.PD_UId = user.U_Id;
        //        //info.PD_UPhone = user.U_LoginPhone;
        //        //info.PD_Flag = 1;
        //        //info.PD_Time = DateTime.Now; 

        //        //db.Core_PrizeDetail.Add(info);  

        //        Sys_OperateLog log = new Sys_OperateLog() { L_Desc = "参加官方“九宫格”抽奖活动", L_DriverType = 3, L_Flag = (int)ModuleEnum.九宫格抽奖, L_Phone = info.PD_UPhone, L_UId = info.PD_UId, L_Url = "/Prize/Lottery", L_Status = 0, L_Time = DateTime.Now };

        //        db.Sys_OperateLog.Add(log);  //添加日志



        //        if (db.SaveChanges() > 0)
        //        {
        //            rs.State = 0;
        //            rs.Data = new
        //            {
        //                awards = info.PD_Award,
        //                token = ""
        //            };
        //            rs.Msg = "ok";
        //        }
        //    }
        //    return rs;
        //}




        ///// <summary>
        ///// 活动分享出去，用户填写手机号码领奖
        ///// </summary>
        ///// <param name="phone">电话号码</param>
        ///// <param name="info">中奖详情</param>
        ///// <returns></returns>
        //public ResultMessageViewModel TurnOutside(string phone, Core_PrizeDetail info)
        //{
        //    ResultMessageViewModel rs = new ResultMessageViewModel();

        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        //用户已经参加过一次“xx”活动第一关抽奖
        //        var prize = db.Core_PrizeDetail.FirstOrDefault(o => o.PD_UPhone == phone && o.PD_TypeId == 0 && o.PD_Round == 1);

        //        if (prize != null)
        //        {
        //            rs.Msg = "此电话号码已经参加过本次抽奖，不能重复参加";
        //            return rs;
        //        }
        //        var user = db.Core_User.FirstOrDefault(o => o.U_LoginPhone == phone);

        //        if (user != null)
        //        {
        //            rs.State = 1;
        //            rs.Msg = "此帐号是邻信业主，请登录邻信APP参加活动！";
        //            return rs;
        //        }

        //        //prize = new Core_Prize()
        //        //{
        //        //    P_Count = 1,
        //        //    P_Id = Guid.NewGuid(),
        //        //    P_RemainCount = 1,  //没有注册的用户
        //        //    P_Time = DateTime.Now,
        //        //    P_TimeUseful = DateTime.Now.AddDays(60),
        //        //    P_TypeId = 1,
        //        //    P_UId = Guid.Empty,
        //        //    P_UPhone = phone
        //        //};

        //        //db.Core_Prize.Add(prize);  //添加抽奖机会


        //        info.PD_Id = Guid.NewGuid();
        //        info.PD_UId = Guid.Empty;
        //        info.PD_UPhone = phone;
        //        info.PD_Flag = 0;  //未领取
        //        info.PD_Time = DateTime.Now;
        //        info.PD_Round = 1;//只能是第一关抽奖

        //        info.PD_TimeUseful = DateTime.Now.AddDays(60);

        //        db.Core_PrizeDetail.Add(info);  //添加领奖详情
        //        if (db.SaveChanges() > 0)
        //        {
        //            rs.State = 0;
        //            rs.Msg = "ok";
        //        }
        //    }
        //    return rs;
        //}

        ///// <summary>
        ///// 添加一个抽奖项目
        ///// </summary>
        ///// <param name="uid">中奖人ID</param>
        ///// <param name="phone">电话</param>
        ///// <param name="round">第几关</param>
        ///// <returns></returns>
        //public bool AddOne(Guid uid, string phone, int round)
        //{
        //    if (round > 2) //第三轮开始
        //    {
        //        var info = base.LoadEntity(o => o.PD_UId == uid && o.PD_Round == round);
        //        var infoPre = base.LoadEntity(o => o.PD_UId == uid && o.PD_Round == round - 1);
        //        if (info == null && infoPre != null)  //并未获得此轮的机会，并且已经取得了上一轮的资格
        //        {

        //            var pd = new Core_PrizeDetail();
        //            pd.PD_Id = Guid.NewGuid();
        //            pd.PD_Flag = 0;
        //            pd.PD_Round = round;
        //            pd.PD_Time = DateTime.Now;
        //            pd.PD_TimeUseful = DateTime.Now.AddDays(60);
        //            pd.PD_UId = uid;
        //            pd.PD_UPhone = phone;

        //            if (base.AddEntity(pd) != null)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

    }
}
