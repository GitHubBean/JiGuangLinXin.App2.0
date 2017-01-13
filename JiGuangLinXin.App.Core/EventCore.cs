using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class EventCore : BaseRepository<Core_Event>
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();


        /// <summary>
        /// 检查活动是否有效
        /// </summary>
        /// <param name="eid">活动ID</param>
        /// <returns>true 有效 false 无效</returns>
        public bool IsActivity(string eid)
        {
            Guid id;
            if (Guid.TryParse(eid, out id))
            {
                return base.LoadEntity(o => o.E_Id == id && o.E_Status == 0 && o.E_AuditingState == (int)AuditingEnum.认证成功 && o.E_STime < DateTime.Now && o.E_ETime > DateTime.Now) != null;
            }

            return false;
        }

        ///// <summary>
        ///// 获取邻友圈 活动列表
        ///// </summary>
        ///// <param name="buildingId">社区id</param>
        ///// <param name="pn">页码</param>
        ///// <param name="rows">多少条</param>
        ///// <returns></returns>
        //        public IEnumerable<EventIndexViewModel> GetLinyouEvents(Guid? buildingId, int pn, int rows = 5)
        //        {

        //            //e_status=0  未删除， b_status==0 商家未被冻结
        //            string sql = @"select top @topCount *
        //                            from 
        //	                            (
        //	                            select row_number() over(order by a.E_Recom desc, a.E_Rank asc,a.E_Date desc) as rownumber,a.E_Id,a.E_BusId,a.E_Title,a.E_Tags,a.E_Img,a.E_Video,a.E_Clicks,a.E_Comments,a.E_Likes,a.E_Date,b.B_NickName,b.B_Logo,b.B_City,b.B_Address
        //                                     from Core_Event a
        //                                     inner join Core_Business b
        //                                     on a.E_BusId = b.B_Id
        //                                    where {0} and  a.E_Status = 0 and b.B_Status = 0 and a.E_ETime >GETDATE()
        //	                            ) C
        //                            where rownumber > @skipCount";

        //            //a.E_VillageId=@buildingId
        //            string where = buildingId == null ? "1=1" : "a.E_VillageId='" + buildingId + "'";
        //            using (DbContext db = new LinXinApp20Entities())
        //            {
        //                var rs =
        //                    db.Database.SqlQuery(typeof(EventIndexViewModel), string.Format(sql, where), new SqlParameter("@topCount", rows),
        //                        new SqlParameter("@skipCount", (pn - 1) * rows)).Cast<EventIndexViewModel>();

        //                if (rs.Any())//结果集有数据
        //                {//遍历结果集中的所有点赞列表以及、前4条评论

        //                    string commentsSql = "select top 4 C_Id,C_UserId,C_UserName,C_Content from Core_Comments where C_State = 0 and C_ProjectId=@projectId  order by  C_Time desc";
        //                    string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
        //                                            inner join Core_User as u
        //                                            on l.L_UserId = u.U_Id
        //                                            where l.L_State = 0 and l.L_ProjectId=@projectId order by L_Time desc";
        //                    foreach (var item in rs)
        //                    {
        //                        //string a =item.s;
        //                        //1获取 评论集合
        //                        Guid tid = item.E_Id;
        //                        item.CommentsList = db.Database.SqlQuery(typeof(object), commentsSql, new SqlParameter("@projectId", item.E_Id));

        //                        //2获取 点赞集合
        //                        item.LikesList = db.Database.SqlQuery(typeof(object), likesSql, new SqlParameter("@projectId", item.E_Id));

        //                    }
        //                    return rs;
        //                }
        //            }
        //            return null;
        //        }

        /// <summary>
        ///  获取商家 活动（定向小区），如果是平台直接可以查询得到，不用链接表
        /// </summary>
        /// <param name="buildingId">sfs</param>
        /// <returns></returns>
        public IEnumerable<Core_Event> GetBuildingEvents(Guid buildingId)
        {

            string sql = @"select  a.* from Core_Event a
                                inner join Core_BusinessVillage b
                                on a.E_BusId= b.BV_BusinessId
                                where a.E_Status=0 and a.E_AuditingState=1 and a.E_Recom=0 and a.E_Target =1 and b.BV_VillageId = '{0}'
                                order by a.E_Rank ";
            if (buildingId != Guid.Empty)
            {
                return base.ExecuteStoreQuery(string.Format(sql, buildingId));
            }
            return null;
        }

        /// <summary>
        /// 查看单个活动，返回活动关联的，点赞、红包、评论记录
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public EventIndexViewModel GetOneEvent(Guid proId)
        {

            //t_status=0  未删除， u_status==0 用户未被冻结
            string sql = @"select a.E_Id,a.E_BusId,a.E_Title,a.E_Tags,a.E_Img,a.E_Video,a.E_Clicks,a.E_Comments,a.E_Likes,a.E_Date,b.B_NickName,b.B_Logo,b.B_City,b.B_Address
                                     from Core_Event a
                                     inner join Core_Business b
                                     on a.E_BusId = b.B_Id
                                    where a.E_Id=@proId and  a.E_Status = 0 and a.E_AuditingState=1 and b.B_Status = 0 ";




            string commentsSql = "select C_Id,C_UserId,C_UserName,C_RefId,C_RefName,C_Content from Core_Comments where C_State = 0 and C_ProjectId=@projectId  C_Time desc";
            string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                            inner join Core_User as u
                                            on l.L_UserId = u.U_Id
                                            where l.L_State = 0 and l.L_ProjectId=@projectId order by L_Time desc";


            using (DbContext db = new LinXinApp20Entities())
            {

                EventIndexViewModel item = db.Database.SqlQuery<EventIndexViewModel>(sql, new SqlParameter("@proId", proId)).ToList<EventIndexViewModel>().FirstOrDefault();
                if (item != null)
                {
                    //1获取 评论集合
                    item.CommentsList = db.Database.SqlQuery(typeof(object), commentsSql,
                        new SqlParameter("@projectId", proId));

                    //2获取 点赞集合
                    item.LikesList = db.Database.SqlQuery(typeof(object), likesSql,
                        new SqlParameter("@projectId", proId));

                    return item;
                }
            }
            return null;
        }





        /// <summary>
        /// 点赞 商家活动
        /// </summary>
        /// <param name="proId">活动ID</param>
        /// <param name="uid">用户ID</param>
        /// <param name="proName">活动标题</param>
        /// <param name="nikeName">用户昵称</param>
        /// <returns></returns>
        public bool Like(Guid proId, Guid uid, string proName, string nikeName)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var topic = db.Set<Core_Event>().FirstOrDefault(o => o.E_Id == proId);
                topic.E_Likes += 1;
                db.Set<Core_Event>().Attach(topic);
                db.Entry<Core_Event>(topic).State = EntityState.Modified;  //1累计赞

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
        /// 评论 商家活动
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="uid"></param>
        /// <param name="refUid">引用谁的评论</param>
        /// <param name="refUname">引用人的name</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool Comment(Guid proId, Guid uid, string nikeName, string refUid, string refUname, string content)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var tp = db.Set<Core_Event>().FirstOrDefault(o => o.E_Id == proId);
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);



                DateTime dt = DateTime.Now;
                if (tp != null)  //话题有红包，且红包未被领取完
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == proId);  //话题红包

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
                            LH_Money = hb.LG_Money,
                            LH_Remark = string.Format("领取商家【{0}】邻里圈活动：“{1}” 红包", tp.E_BusName, tp.E_Title),
                            LH_Status = 0,
                            LH_UserId = uid,
                            LH_UserNickName = user.U_NickName,
                            LH_UserPhone = user.U_LoginPhone,
                            LH_Flag = (int)LuckGiftTypeEnum.邻友圈用户红包
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
                        Core_BillMember bill = new Core_BillMember() { B_Time = dt, B_Remark = "商家邻友圈活动：" + tp.E_Title, B_Money = hb.LG_Money, B_UId = user.U_Id, B_Flag = (int)BillFlagEnum.普通流水, B_Module = (int)BillEnum.红包, B_OrderId = hisID, B_Phone = user.U_LoginPhone, B_Status = 0, B_Title = "评论邻友圈商家活动，获得商家红包", B_Type = (int)MemberRoleEnum.会员 };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 添加评论者的红包账单

                        if (hb.LG_RemainCount == 0) //5红包被领光了,更改话题的红包标识
                        {
                            //  tp.E_Hongbao = (int)LuckGiftFlagEnum.红包被领光;
                        }

                    }
                }

                tp.E_Comments += 1;  //6累计话题的评论数

                Guid rId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid);
                Core_Comments com = new Core_Comments() { C_Content = content, C_Id = Guid.NewGuid(), C_ProjectId = proId, C_ProjectTitle = tp.E_Title, C_RefId = rId, C_RefName = refUname, C_State = 0, C_Time = dt, C_Type = (int)CommentTypeEnum.邻友圈话题, C_UserId = uid, C_UserName = nikeName };

                db.Entry(com).State = EntityState.Added;  //7新增评论

                return db.SaveChanges() > 0;

            }
            return false;
        }

        /// <summary>
        /// 添加社区互动
        /// </summary>
        /// <param name="et">活动对象</param>
        /// <param name="items">投票项</param>
        /// <param name="images">图片列表</param>
        /// <returns></returns>
        public bool AddOneEvent(Core_Event et, IEnumerable<dynamic> items, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Core_Event.Add(et);

                if (et.E_Flag == (int)EventFlagEnum.投票 && items != null && items.Any())  //投票互动
                {
                    foreach (var item in items)
                    {
                        Core_EventVoteItem vi = new Core_EventVoteItem();
                        vi.I_EventId = et.E_Id;
                        vi.I_EventTtitle = et.E_Title;

                        vi.I_Title = item.I_Title;
                        vi.I_Rank = item.I_Rank;
                        vi.I_Img = item.I_Img;
                        vi.I_State = 0;
                        vi.I_Count = 0;
                        vi.I_Id = Guid.NewGuid();

                        db.Core_EventVoteItem.Add(vi);  //添加投票项目
                    }
                }

                if (images != null && images.Any())
                {
                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.E_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = et.E_Date;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }



        /// <summary>
        /// 编辑社区互动
        /// </summary>
        /// <param name="et">活动对象</param>
        /// <param name="items">投票项</param>
        /// <param name="images">图片列表</param>
        /// <returns></returns>
        public bool EditOneEvent(Core_Event et, IEnumerable<dynamic> items, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Set<Core_Event>().Attach(et);
                db.Entry(et).State = EntityState.Modified;  //1修改活动

                //2投票互动，如果要修改活动，必须删除之前配置的投票项目
                if (et.E_Flag == (int)EventFlagEnum.投票 && items != null && items.Any())
                {
                    //var tempList = db.Core_EventVoteItem.Where(o => o.I_EventId == et.E_Id); 

                    db.Core_EventVoteItem.Update(o => o.I_EventId == et.E_Id, o => new Core_EventVoteItem() { I_State = 1 });  //批量删除,逻辑

                    foreach (var item in items)
                    {
                        Core_EventVoteItem vi = new Core_EventVoteItem();
                        vi.I_EventId = et.E_Id;
                        vi.I_EventTtitle = et.E_Title;

                        vi.I_Title = item.I_Title;
                        vi.I_Rank = item.I_Rank;
                        vi.I_Img = item.I_Img;
                        vi.I_State = 0;
                        vi.I_Count = 0;
                        vi.I_Id = Guid.NewGuid();

                        db.Core_EventVoteItem.Add(vi);  //添加投票项目
                    }
                }
                //3活动图片附件，如果修改，必须删除之前配置的活动图片
                if (images != null && images.Any())
                {
                    db.Sys_Attachment.Delete(o => o.A_PId == et.E_Id);  //批量删除，物理

                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.E_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = et.E_Date;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }


    }
}
