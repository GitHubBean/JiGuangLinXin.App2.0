using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection.Emit;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.Core
{
    /// <summary>
    /// 邻友圈  广告橱窗位（核心广告位）
    /// </summary>
    public class ShowcaseCore : BaseRepository<Core_Showcase>
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        /// <summary>
        /// 推广的 广告位
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <param name="pn">当前页码</param>
        /// <returns></returns>
        public dynamic QueryShowcase(Guid buildingId, int pn)
        {
            dynamic rs = null;
            Guid caseId = Guid.Empty;  //推广活动ID
            Guid giftProId = Guid.Empty;  //推广活动 对应的红包ID
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                pn = pn - 1;

                var guanfang =
                    db.Core_Showcase.Where(
                        o => o.S_Status == 0 && o.S_Target == 0 && DateTime.Now > o.S_STime && DateTime.Now < o.S_ETime).OrderBy(o => o.S_Rank);
                if (guanfang.Count() > pn)  //官方有活动
                {
                    var gf = guanfang.Skip(pn).Take(1).Select(a => new
                    {

                        a.S_Id,
                        a.S_BusId,
                        a.S_BusName,
                        a.S_BusLogo,
                        a.S_BusRole,
                        a.S_Phone,
                        a.S_Title,
                        a.S_ImgTop,
                        a.S_Flag,
                        a.S_Video,
                        a.S_Img,
                        a.S_Content,
                        a.S_Tags,
                        a.S_Remark,
                        a.S_Likes,
                        a.S_Comments,
                        a.S_Clicks,
                        a.S_Desc,
                        a.S_Address,
                        a.S_LinkPhone,
                        a.S_Date,
                        a.S_TargetUrl,
                        a.S_Rank,
                        a.S_GoodsName,
                        a.S_GoodsId,
                        a.S_BuildingId,
                        a.S_BuildingName,
                        a.S_Hongbao,
                        targetId = "",
                        tempId = "",
                        S_Ticket = 0

                    }).FirstOrDefault();
                    caseId = gf.S_Id;
                    rs = gf;

                    if (gf.S_Hongbao == (int)LuckGiftFlagEnum.有红包) //带有红包
                    {

                        giftProId = caseId;
                    }
                }
                else  //没有官方活动
                {
                    //发布到社区的活动
                    var rs1 =
                        db.Core_Showcase.Where(o => o.S_Status == 0 && o.S_Target == 1 && o.S_STime < DateTime.Now && o.S_ETime > DateTime.Now).Join(db.Core_ShowcaseScope, a => a.S_Id, b => b.S_EId, (a, b) => new
                        {
                            a.S_Id,
                            a.S_BusId,
                            a.S_BusName,
                            a.S_BusLogo,
                            a.S_BusRole,
                            a.S_Hongbao,
                            a.S_Phone,
                            a.S_Title,
                            a.S_ImgTop,
                            a.S_Flag,
                            a.S_Video,
                            a.S_Img,
                            a.S_Content,
                            a.S_Tags,
                            a.S_Remark,
                            a.S_Likes,
                            a.S_Comments,
                            a.S_Clicks,
                            a.S_Desc,
                            a.S_Address,
                            a.S_LinkPhone,
                            a.S_Date,
                            a.S_TargetUrl,
                            a.S_GoodsName,
                            a.S_GoodsId,
                            a.S_BuildingId,
                            a.S_BuildingName,
                            a.S_Rank,
                            targetId = b.S_BuildingId,
                            tempId = b.S_Id,
                            S_Ticket = 0

                        }).Where(b => b.targetId == buildingId).OrderBy(o => o.S_Rank).Skip(pn).Take(1).FirstOrDefault();


                    if (rs1 != null)
                    {
                        rs = rs1;
                        caseId = rs1.S_Id;
                        if (rs1.S_Hongbao == (int)LuckGiftFlagEnum.有红包)  //带有红包
                        {
                            giftProId = rs1.tempId;

                        }
                    }
                }
                if (rs != null)
                {
                    string commentsSql = "select top 4 C_Id,C_UserId,C_UserName,C_RefId,C_RefName,C_Content from Core_Comments where C_State = 0 and C_ProjectId=@projectId order by  C_Time desc";
                    string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                        inner join Core_User as u
                                        on l.L_UserId = u.U_Id
                                        where l.L_State = 0 and l.L_ProjectId=@projectId  order by L_Time desc";
                    string hongbaoSql = @"select c.U_Id, c.U_Logo,a.LH_Money from Core_LuckyGiftHistory a
                                            inner join Core_LuckyGift b
                                            on a.LH_GiftId  = b.LG_Id
                                            inner join Core_User c
                                            on c.U_Id = a.LH_UserId  where b.LG_ProjectId =@projectId order by LH_CreateTime desc";



                    var CommentsList = db.Database.SqlQuery<CommentsIndexViewModel>(commentsSql, new SqlParameter("@projectId", caseId)).ToList();

                    var lList = db.Database.SqlQuery<LikeIndexViewModel>(likesSql, new SqlParameter("@projectId", caseId)).ToList();
                    dynamic HongbaoList = null;

                    if (giftProId != Guid.Empty)  //有红包ID
                    {
                        var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(hongbaoSql, new SqlParameter("@projectId", caseId)).ToList();
                        if (hList.Any())
                        {
                            HongbaoList = hList;  //红包领取列表

                        }
                    }


                    return new
                    {
                        Info = rs,
                        CommentsList,
                        LikesList = lList,
                        HongbaoList,
                    };
                }

            }
            return null;
        }



        /// <summary>
        /// 点赞 商家推广的活动
        /// </summary>
        /// <param name="proId">活动ID</param>
        /// <param name="uid">用户ID</param>
        /// <param name="proName">活动标题</param>
        /// <param name="nikeName">用户昵称</param>
        /// <returns></returns>
        public bool Like(Guid proId, Guid uid, string proName, string nikeName)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var topic = db.Set<Core_Showcase>().FirstOrDefault(o => o.S_Id == proId);
                topic.S_Likes += 1;
                db.Set<Core_Showcase>().Attach(topic);
                db.Entry<Core_Showcase>(topic).State = EntityState.Modified;  //1累计赞

                Core_Likes like = new Core_Likes()
                {
                    L_Id = Guid.NewGuid(),
                    L_ProjectId = proId,
                    L_ProjectTitle = proName,
                    L_State = 0,
                    L_Time = DateTime.Now,
                    L_UserId = uid,
                    L_UserName = nikeName,
                    L_Type = (int)CommentTypeEnum.商家活动
                };
                db.Entry<Core_Likes>(like).State = EntityState.Added;  //2添加赞的记录

                return db.SaveChanges() > 0;
            }
            return false;
        }


        /// <summary>
        /// 评论 商家推广活动
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="uid"></param>
        /// <param name="refUid">引用谁的评论</param>
        /// <param name="refUname">引用人的name</param>
        /// <param name="content"></param>
        /// <param name="buildingId">小区ID</param>
        /// <returns></returns>
        public ResultMessageViewModel Comment(Guid proId, Guid uid, string nikeName, string refUid, string refUname, string content, Guid buildingId, ref decimal hongbaoMoney)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var tp = db.Set<Core_Showcase>().FirstOrDefault(o => o.S_Id == proId);
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);

                int hongbaoResult = 1;  //没领红包为1 领取了红包就是2 


                DateTime dt = DateTime.Now;

                //发送话题的用户
                var recUser = db.Set<Core_Business>().FirstOrDefault(o => o.B_Id == tp.S_BusId);
                Guid giftId = Guid.Empty;
                if (tp != null && tp.S_Hongbao == (int)LuckGiftFlagEnum.有红包)  //话题有红包，且红包未被领取完
                {
                    Guid giftProId;
                    if (tp.S_Target == 0)  //平台的推广
                    {
                        giftProId = proId;
                    }
                    else  //商家发的推广
                    {
                        var sc =
                            db.Core_ShowcaseScope.FirstOrDefault(o => o.S_BuildingId == buildingId && o.S_EId == proId);
                        if (sc == null)
                        {
                            return rs;
                        }

                        giftProId = sc.S_Id;
                    }

                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == giftProId);  //话题红包

                    var recHis =
                        db.Set<Core_LuckyGiftHistory>()
                            .FirstOrDefault(o => o.LH_UserId == uid && o.LH_GiftId == hb.LG_Id);  //领取的历史记录

                    if (hb != null && hb.LG_RemainCount > 0 && hb.LG_RemainMoney > 0 && recHis == null)  //有红包且该用户还没有领取过
                    {

                        decimal money = 0;
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
                            LH_CreateTime = dt,
                            LH_GiftDetailId = null,
                            LH_GiftId = hb.LG_Id,
                            LH_Id = hisID,
                            LH_Money = money,// hb.LG_Money,
                            LH_Remark = string.Format("领取邻里圈商家推广活动：“{0}” 红包", tp.S_Title),
                            LH_Status = 0,
                            LH_UserId = uid,
                            LH_UserNickName = user.U_NickName,
                            LH_UserPhone = user.U_LoginPhone,
                            LH_Flag = (int)LuckGiftTypeEnum.邻友圈用户红包,
                            LH_UserLogo = user.U_Logo
                        };
                        // 2. 添加领取的红包记录
                        db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;

                        var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                        balance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;   //3.累计到余额


                        //添加会员账单
                        Core_BillMember bill = new Core_BillMember() { B_Time = dt, B_Remark = "邻里圈商家推广活动：" + tp.S_Title, B_Money = hb.LG_Money, B_UId = user.U_Id, B_Flag = (int)BillFlagEnum.普通流水, B_Module = (int)BillEnum.红包, B_OrderId = hisID, B_Phone = user.U_LoginPhone, B_Status = 0, B_Title = "评论邻友圈商家活动，获得红包", B_Type = (int)MemberRoleEnum.会员 };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 添加评论者的红包账单

                        if (hb.LG_RemainCount == 0) //5红包被领光了,更改话题的红包标识
                        {
                            tp.S_Hongbao = (int)LuckGiftFlagEnum.红包被领光;
                        }

                        hongbaoMoney = money;
                        hongbaoResult = 2;
                    }
                }

                tp.S_Comments += 1;  //6累计话题的评论数

                Guid rId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid);
                Core_Comments com = new Core_Comments()
                {
                    C_Content = content,
                    C_Id = Guid.NewGuid(),
                    C_ProjectId = proId,
                    C_ProjectTitle = tp.S_Title,
                    C_RefId = rId,
                    C_RefName = refUname,
                    C_State = 0,
                    C_Time = dt,
                    C_Type = (int)CommentTypeEnum.邻友圈话题,
                    C_UserId = uid,
                    C_UserName = nikeName
                };

                db.Entry(com).State = EntityState.Added;  //7新增评论

                //return db.SaveChanges() > 0;

                if (db.SaveChanges() > 0)
                {
                    rs.State = hongbaoResult;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        C_Content = content,
                        C_Id = com.C_Id,
                        C_RefId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid),
                        C_RefName = refUname,
                        C_UserId = uid,
                        C_UserName = nikeName,

                        hb = hongbaoResult != 2 ? null : new
                        {
                            money = hongbaoMoney,
                            tips = "恭喜发财",
                            headImg = recUser.B_Logo,
                            nickName = recUser.B_NickName,
                            giftId
                        }
                    };
                }

            }
            return rs;
        }


        /// <summary>
        /// 查看单个广告详情
        /// </summary>
        /// <returns></returns>
        public dynamic GetOneShowCase(Guid id)
        {
            string commentsSql = "select  C_Id,C_UserId,C_UserName,C_Content from Core_Comments where C_State = 0 and C_ProjectId='{0}' order by  C_Time desc";
            string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                        inner join Core_User as u
                                        on l.L_UserId = u.U_Id
                                        where l.L_State = 0 and l.L_ProjectId='{0}' order by L_Time desc";
            string hongbaoSql = @"select c.U_Id, c.U_Logo,a.LH_Money from Core_LuckyGiftHistory a
                                            inner join Core_LuckyGift b
                                            on a.LH_GiftId  = b.LG_Id
                                            inner join Core_User c
                                            on c.U_Id = a.LH_UserId  where b.LG_ProjectId = '{0}' order by LH_CreateTime desc";


            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                var info = db.Core_Showcase.Where(o => o.S_Id == id && o.S_Status == 0).ToList();


                if (info.Any())
                {
                    var showcase = info.Select(a => new
                    {

                        a.S_Id,
                        a.S_BusId,
                        a.S_BusName,
                        a.S_BusLogo,
                        a.S_BusRole,
                        a.S_Phone,
                        a.S_Title,
                        a.S_ImgTop,
                        a.S_Flag,
                        a.S_Video,
                        a.S_Img,
                        a.S_Content,
                        a.S_Tags,
                        a.S_Remark,
                        a.S_Likes,
                        a.S_Comments,
                        a.S_Clicks,
                        a.S_Desc,
                        a.S_Address,
                        a.S_LinkPhone,
                        a.S_Date,
                        a.S_TargetUrl,
                        a.S_Rank,
                        a.S_GoodsName,
                        a.S_GoodsId,
                        a.S_BuildingId,
                        a.S_BuildingName,
                        a.S_Hongbao,
                        targetId = "",
                        tempId = "",
                        S_Ticket = 0

                    }).First();

                    //1获取话题 评论集合 
                    var cList =
                        db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, id)).ToList();
                    dynamic CommentsList = null;
                    if (cList.Any())
                    {
                        CommentsList = cList;
                    }

                    //2获取话题 点赞集合
                    var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, id)).ToList();
                    dynamic LikesList = null;
                    if (lList.Any())
                    {
                        LikesList = lList;
                    }

                    //3获取话题 得到红包的用户列表
                    dynamic HongbaoList = null;
                    if (showcase.S_Hongbao == (int)LuckGiftFlagEnum.有红包) //标识为红包的话题
                    {
                        HongbaoList =
                            db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, id)).ToList();
                    }


                    //1 主题浏览次数累计
                    var sc = info.First();
                    sc.S_Clicks += 1;
                    base.UpdateEntity(sc); //更新点击次数

                    return new
                    {
                        Info = showcase,
                        CommentsList,
                        LikesList,
                        HongbaoList
                    };
                }
            }
            return null;
        }



        #region 商家管理系统后台
        /// <summary>
        /// 发布邻里圈 商家广告位 内容
        /// </summary>
        /// <param name="sc">推广内容</param>
        /// <param name="hbCount">红包个数</param>
        /// <param name="hbMoney">红包金额</param>
        /// <param name="buildings">定向小区集合</param>
        /// <returns></returns>
        public ResultMessageViewModel AddOne(Core_Showcase sc, int hbCount, decimal hbMoney, IEnumerable<dynamic> buildings)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var busBalance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == sc.S_BusId);
                if (busBalance == null)
                {
                    rs.Msg = "商家不存在";
                    return rs;
                }
                decimal totalMoney = 0;
                if (sc.S_Target == (int)AdTargetEnum.全平台广告)
                {
                    totalMoney = hbCount * hbMoney;
                }
                else if (sc.S_Target == (int)AdTargetEnum.小区定向广告 && buildings.Any())  //每个小区都特么要发红包
                {
                    totalMoney = hbCount * hbMoney * buildings.Count();
                }
                else
                {
                    return rs;
                }

                if (busBalance.B_Balance < totalMoney)
                {
                    rs.Msg = "商家余额不足";
                }
                else  //符合发布条件
                {
                    //1.添加 推广信息
                    db.Core_Showcase.Add(sc);
                    //2.如果是发布到小区，添加发布到小区的记录
                    if (sc.S_Target == (int)AdTargetEnum.小区定向广告)
                    {
                        foreach (var building in buildings)
                        {
                            Core_ShowcaseScope scope = new Core_ShowcaseScope()
                            {
                                S_BuildingId = building.S_BuildingId,
                                S_BuildingName = building.S_BuildingName,
                                S_CityName = building.S_CityName,
                                S_DistrictName = building.S_DistrictName,

                                S_EId = sc.S_Id,
                                S_Id = Guid.NewGuid(),
                                S_Time = sc.S_Date
                            };
                            db.Core_ShowcaseScope.Add(scope);

                            //添加红包信息
                            if (sc.S_Hongbao == 1)
                            {

                                Core_LuckyGift hb = new Core_LuckyGift();

                                hb.LG_Id = Guid.NewGuid();
                                hb.LG_Title = "商家发布邻里圈推广位";
                                hb.LG_Type = (int)LuckGiftTypeEnum.商家推广红包;
                                hb.LG_UserId = sc.S_BusId;
                                hb.LG_UserNickname = sc.S_BusName;
                                hb.LG_Money = hbMoney;
                                hb.LG_RemainMoney = hb.LG_Money;
                                hb.LG_Count = hbCount;
                                hb.LG_RemainCount = hb.LG_Count;
                                hb.LG_CreateTime = sc.S_Date;
                                hb.LG_Flag = null;// scope.S_Id;  //红包与话题关联
                                hb.LG_Status = 0;
                                hb.LG_VillageId = scope.S_BuildingId;
                                hb.LG_AreaCode = scope.S_CityName;
                                hb.LG_ProjectId = scope.S_Id;
                                hb.L_ProjectTitle = sc.S_Title;
                                db.Core_LuckyGift.Add(hb);
                            }
                        }

                    }
                    else  //全平台的广告，添加红包记录
                    {
                        if (sc.S_Hongbao == 1)
                        {
                            Core_LuckyGift hb = new Core_LuckyGift();

                            hb.LG_Id = Guid.NewGuid();
                            hb.LG_Title = "商家发布邻里圈推广位";
                            hb.LG_Type = (int)LuckGiftTypeEnum.商家推广红包;
                            hb.LG_UserId = sc.S_BusId;
                            hb.LG_UserNickname = sc.S_BusName;
                            hb.LG_Money = hbMoney;
                            hb.LG_RemainMoney = hb.LG_Money;
                            hb.LG_Count = hbCount;
                            hb.LG_RemainCount = hb.LG_Count;
                            hb.LG_CreateTime = sc.S_Date;
                            hb.LG_Flag = null;//sc.S_Id;  //红包与话题关联
                            hb.LG_Status = 0;
                            hb.LG_VillageId = Guid.Empty;
                            hb.LG_AreaCode = "全平台";
                            hb.LG_ProjectId = sc.S_Id;
                            hb.L_ProjectTitle = sc.S_Title;
                            db.Core_LuckyGift.Add(hb);
                        }
                    }
                    //发了红包
                    if (totalMoney > 0)
                    {

                        //3扣除余额
                        busBalance.B_Balance -= totalMoney;

                        //4添加账单流水
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.红包,
                            B_Money = -totalMoney,
                            B_OrderId = sc.S_Id,
                            B_Phone = sc.S_Phone,
                            B_Remark = Enum.GetName(typeof(AdTargetEnum), sc.S_Target),
                            B_Status = 0,
                            B_Time = sc.S_Date,
                            B_Title = "商家发布邻里圈推广位",
                            B_UId = sc.S_BusId,
                            B_Type = (int)MemberRoleEnum.商家
                        };
                        db.Core_BillMember.Add(bill);

                    }

                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }
            }


            return rs;
        }

        #endregion
    }
}
