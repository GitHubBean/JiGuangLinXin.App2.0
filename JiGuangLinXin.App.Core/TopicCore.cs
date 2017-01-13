using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection.Emit;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.Core
{
    public class TopicCore : BaseRepository<Core_Topic>
    {
        private LikesCore likeCore = new LikesCore();
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        private AttachmentCore attCore = new AttachmentCore();

        /// <summary>
        /// 获取 邻友 的话题
        /// </summary>
        /// <param name="buildingId">小区的ID</param>
        /// <param name="pn">页码</param>
        /// <param name="rows">条数</param>
        /// <returns></returns>
        public List<TopicIndexViewModel> GetLinyouTopics(Guid? buildingId, int pn, int rows = 10)
        {
            //t_status=0  未删除， u_status!=1 用户未被冻结
            string sql = @"select top {1} *
                            from 
	                            (
	                            select row_number() over(order by a.T_Top desc,T_Recom desc ,a.T_Date desc) as rownumber, a.T_Id,a.T_UserId,a.T_Title,a.T_Typle,a.T_Hongbao,a.T_Ticket,a.T_Img,a.T_ImgAttaCount,a.T_Tags,a.T_Clicks,a.T_Likes,a.T_Comments,a.T_Date,
			                            b.U_Logo,b.U_NickName,b.U_Sex,b.U_Age,b.U_City,'. '+b.U_BuildingName as U_BuildingName
	                            from  Core_Topic a
	                            inner join Core_User b
	                            on a.T_UserId = b.U_Id
	                            where {0} and a.T_Status = 0 and b.U_Status!=1
	                            ) C
                            where rownumber > {2}";



            //sql = string.Format(sql, rows, (page-1)*rows);  //分页的sql

            string where = "b.U_BuildingId='" + buildingId + "'";
            if (buildingId == null || Guid.Empty == buildingId)  //全国的社区
            {
                where = "1=1";
                sql = @"select top {1} *
                            from 
	                            (
	                            select row_number() over(order by a.T_Clicks desc) as rownumber, a.T_Id,a.T_UserId,a.T_Title,a.T_Typle,a.T_Hongbao,a.T_Ticket,a.T_Img,a.T_ImgAttaCount,a.T_Tags,a.T_Clicks,a.T_Likes,a.T_Comments,a.T_Date,
			                            b.U_Logo,b.U_NickName,b.U_Sex,b.U_Age,b.U_City,'. '+b.U_BuildingName as U_BuildingName
	                            from  Core_Topic a
	                            inner join Core_User b
	                            on a.T_UserId = b.U_Id
	                            where {0} and a.T_Status = 0 and b.U_Status!=1
	                            ) C
                            where rownumber > {2}";
            }

            using (DbContext db = new LinXinApp20Entities())
            {

                //var cc = db.Database.SqlQuery<object>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();
                var rs = db.Database.SqlQuery<TopicIndexViewModel>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();

                if (rs.Any())  //结果集有数据
                {
                    //遍历结果集中的所有点赞列表以及、前4条评论

                    string commentsSql = "select top 4 C_Id,C_UserId,C_UserName,C_RefId,C_RefName,C_Content from Core_Comments where C_State = 0 and C_ProjectId='{0}' order by  C_Time desc";
                    string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                        inner join Core_User as u
                                        on l.L_UserId = u.U_Id
                                        where l.L_State = 0 and l.L_ProjectId='{0}' order by L_Time desc";

                    string hongbaoSql = @"select a.LH_UserId as U_Id, a.LH_UserLogo as U_Logo,a.LH_Money from Core_LuckyGiftHistory a
                                            inner join Core_LuckyGift b
                                            on a.LH_GiftId  = b.LG_Id
                                            inner join Core_User c
                                            on c.U_Id = a.LH_UserId where b.LG_ProjectId ='{0}' order by LH_CreateTime desc";


                    foreach (var item in rs)
                    {
                        //1获取话题 评论集合
                        Guid tid = item.T_Id;
                        var cList =
                            db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();
                        item.CommentsList = null;
                        if (cList.Any())
                        {
                            item.CommentsList = cList;
                        }

                        //2获取话题 点赞集合
                        var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                        item.LikesList = null;
                        if (lList.Any())
                        {
                            item.LikesList = lList;
                        }

                        //3获取话题 得到红包的用户列表
                        item.HongbaoList = null;

                        if (item.T_Hongbao > 0)  //标识为红包的话题
                        {
                            var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                            if (hList.Any())
                            {
                                item.HongbaoList = hList;

                            }
                        }

                        //4获取附件的列表
                        item.attachmentList = null;
                        if (item.T_ImgAttaCount > 1)  //有附件
                        {
                            item.attachmentList =
                                attCore.LoadEntities(p => p.A_PId == item.T_Id).OrderBy(o => o.A_Rank).ToList().Select(o => new
                                {
                                    imgUrl = string.Format("{0}/{1}", o.A_Folder, o.A_FileName)
                                });
                        }
                    }

                    return rs;
                }
            }

            //base.ExecuteStoreQuery(sql, new SqlParameter("@topCount", rows), new SqlParameter("@skipCount", (page - 1) * rows));

            return null;
        }


        /// <summary>
        /// 获取 好友 的话题
        /// </summary>
        /// <param name="uid">当前用户ID</param>
        /// <param name="buildingId">小区的ID</param>
        /// <param name="pn">页码</param>
        /// <param name="rows">条数</param>
        /// <returns></returns>
        public List<TopicIndexViewModel> GetFriendsTopics(Guid uid, Guid? buildingId, int pn, int rows = 10)
        {
            //t_status=0  未删除， u_status!=1 用户未被冻结
            string sql = @"select top {1} *
                            from 
	                            (
	                            select row_number() over(order by a.T_Top desc,T_Recom desc ,a.T_Date desc) as rownumber, a.T_Id,a.T_UserId,a.T_Title,a.T_Typle,a.T_Hongbao,a.T_Ticket,a.T_Img,a.T_ImgAttaCount,a.T_Tags,a.T_Clicks,a.T_Likes,a.T_Comments,a.T_Date,
			                            b.U_Logo,b.U_NickName,b.U_Sex,b.U_Age,b.U_City,'. '+b.U_BuildingName as U_BuildingName
	                            from  Core_Topic a
	                            inner join Core_User b
	                            on a.T_UserId = b.U_Id
                                inner join (select F_UId,F_FriendId from Core_UserFriend where F_UId='{3}' or F_FriendId = '{3}') c
	                            on a.T_UserId = c.F_UId or a.T_UserId = c.F_FriendId
	                            where {0} and a.T_Status = 0 and b.U_Status!=1  and  a.T_UserId!='{3}'
	                            ) C
                            where rownumber > {2}";

            //sql = string.Format(sql, rows, (page-1)*rows);  //分页的sql

            string where = (buildingId == null || Guid.Empty == buildingId) ? "1=1" : "a.T_VillageId='" + buildingId + "'";
            using (DbContext db = new LinXinApp20Entities())
            {
                var rs =
                    db.Database.SqlQuery(typeof(TopicIndexViewModel),
                        (string.Format(sql, where, rows, (pn - 1) * rows, uid))).Cast<TopicIndexViewModel>().ToList();

                if (rs.Any())  //结果集有数据
                {
                    //遍历结果集中的所有点赞列表以及、前4条评论

                    string commentsSql = "select top 4 C_Id,C_UserId,C_UserName,C_RefId,C_RefName,C_Content from Core_Comments where C_State = 0 and C_ProjectId='{0}' order by  C_Time desc";
                    string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                        inner join Core_User as u
                                        on l.L_UserId = u.U_Id
                                        where l.L_State = 0 and l.L_ProjectId='{0}' order by L_Time desc";
                    string hongbaoSql = @"select a.LH_UserId as  U_Id, a.LH_UserLogo as  U_Logo,a.LH_Money from Core_LuckyGiftHistory a
                                            inner join Core_LuckyGift b
                                            on a.LH_GiftId  = b.LG_Id
                                            where b.LG_ProjectId = '{0}' order by a.LH_CreateTime desc";

                    foreach (var item in rs)
                    {
                        //1获取话题 评论集合
                        var cList =
                            db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();

                        item.CommentsList = null;
                        if (cList.Any())
                        {
                            item.CommentsList = cList;
                        }

                        //2获取话题 点赞集合
                        var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                        item.LikesList = null;
                        if (lList.Any())
                        {
                            item.LikesList = lList;
                        }

                        //3获取话题 得到红包的用户列表
                        item.HongbaoList = null;

                        if (item.T_Hongbao > 0)  //标识为红包的话题
                        {
                            var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                            if (hList.Any())
                            {
                                item.HongbaoList = hList;

                            }
                        }

                        //4获取附件的列表
                        item.attachmentList = null;
                        if (item.T_ImgAttaCount > 1)  //有附件
                        {
                            item.attachmentList =
                                attCore.LoadEntities(p => p.A_PId == item.T_Id).OrderBy(o => o.A_Rank).ToList().Select(o => new
                                {
                                    imgUrl = string.Format("{0}/{1}", o.A_Folder, o.A_FileName)
                                });
                        }
                    }

                    return rs;
                }
            }

            //base.ExecuteStoreQuery(sql, new SqlParameter("@topCount", rows), new SqlParameter("@skipCount", (page - 1) * rows));

            return null;
        }



        /// <summary>
        /// 查看单个用户话题，返回话题关联的，点赞、红包、评论记录
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public TopicIndexViewModel GetOneTopic(Guid proId)
        {

            //t_status=0  未删除， u_status!=1 用户未被冻结
            string sql = @"select a.T_Id,a.T_UserId,a.T_Title,a.T_Typle,a.T_Hongbao,a.T_Ticket,a.T_Img,a.T_ImgAttaCount,a.T_Tags,a.T_Clicks,a.T_Likes,a.T_Comments,a.T_Date,
			                            b.U_Logo,b.U_NickName,b.U_Sex,b.U_Age,b.U_City,'. '+b.U_BuildingName as U_BuildingName
	                            from  Core_Topic a
	                            inner join Core_User b
	                            on a.T_UserId = b.U_Id
	                            where  a.T_Id='{0}' and a.T_Status = 0 and b.U_Status!=1";




            string commentsSql = "select  C_Id,C_UserId,C_UserName,C_RefId,C_RefName,C_Content from Core_Comments where C_State = 0 and C_ProjectId='{0}' order by  C_Time desc";
            string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
                                        inner join Core_User as u
                                        on l.L_UserId = u.U_Id
                                        where l.L_State = 0 and l.L_ProjectId='{0}' order by L_Time desc";
            string hongbaoSql = @"select c.U_Id, c.U_Logo,a.LH_Money from Core_LuckyGiftHistory a
                                            inner join Core_LuckyGift b
                                            on a.LH_GiftId  = b.LG_Id
                                            inner join Core_User c
                                            on c.U_Id = a.LH_UserId  where b.LG_ProjectId = '{0}' order by LH_CreateTime desc";


            using (DbContext db = new LinXinApp20Entities())
            {

                TopicIndexViewModel item = db.Database.SqlQuery<TopicIndexViewModel>(string.Format(sql, proId)).ToList<TopicIndexViewModel>().FirstOrDefault();
                if (item != null)
                {
                    //1获取话题 评论集合
                    Guid tid = item.T_Id;
                    var cList =
                        db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();
                    item.CommentsList = null;
                    if (cList.Any())
                    {
                        item.CommentsList = cList;
                    }

                    //2获取话题 点赞集合
                    var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                    item.LikesList = null;
                    if (lList.Any())
                    {
                        item.LikesList = lList;
                    }

                    //3获取话题 得到红包的用户列表
                    var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                    item.HongbaoList = null;
                    if (item.T_Hongbao > 0) //标识为红包的话题
                    {
                        item.HongbaoList = hList;
                    }

                    //4获取附件的列表
                    item.attachmentList = null;
                    if (item.T_ImgAttaCount > 1)  //有附件
                    {
                        item.attachmentList =
                            attCore.LoadEntities(p => p.A_PId == item.T_Id).OrderBy(o => o.A_Rank).ToList().Select(o => new
                            {
                                imgUrl = string.Format("{0}/{1}", o.A_Folder, o.A_FileName)
                            });
                    }

                    //1 主题浏览次数累计
                    var pro = base.LoadEntity(o => o.T_Id == proId);
                    pro.T_Clicks += 1;
                    base.UpdateEntity(pro);  //更新点击次数

                    return item;
                }
            }
            return null;
        }


        /// <summary>
        /// 发布话题
        /// </summary>
        /// <param name="topic">话题对象</param>
        /// <param name="hb">红包对象</param>
        /// <param name="ticket">票据对象</param>
        /// <param name="phone">手机号</param>
        /// <returns></returns>
        public ResultMessageViewModel PublishTopic(Core_Topic topic, Core_LuckyGift hb, Core_TopicMovieTicket ticket, string phone, string enPaypwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                if (hb != null)  //有红包，有红包就要扣钱咯（代金券的金额,不能用做红包）
                {
                    Core_Balance balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == topic.T_UserId);//  得到会员账户

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
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.红包,
                            B_Money = -hb.LG_Money,
                            B_OrderId = hb.LG_Id,
                            B_Phone = phone,
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = hb.LG_CreateTime,
                            B_Title = "用户发邻友圈，附红包",
                            B_UId = hb.LG_UserId,
                            B_Type = (int)MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                    }
                }

                if (ticket != null)
                {
                    //添加票务数据
                    db.Entry<Core_TopicMovieTicket>(ticket).State = EntityState.Added;
                }
                //添加话题
                db.Entry<Core_Topic>(topic).State = EntityState.Added;  //1添加话题


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
        /// 点赞 用户话题
        /// </summary>
        /// <param name="proId">话题ID</param>
        /// <param name="uid">用户ID</param>
        /// <param name="proName">话题标题</param>
        /// <param name="nikeName">用户昵称</param>
        /// <returns></returns>
        public bool Like(Guid proId, Guid uid, string proName, string nikeName)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var topic = db.Set<Core_Topic>().FirstOrDefault(o => o.T_Id == proId);
                topic.T_Likes += 1;
                db.Set<Core_Topic>().Attach(topic);
                db.Entry<Core_Topic>(topic).State = EntityState.Modified;  //1累计赞

                Core_Likes like = new Core_Likes()
                {
                    L_Id = Guid.NewGuid(),
                    L_ProjectId = proId,
                    L_ProjectTitle = proName,
                    L_State = 0,
                    L_Time = DateTime.Now,
                    L_UserId = uid,
                    L_UserName = nikeName,
                    L_Type = (int)CommentTypeEnum.邻友圈话题
                };
                db.Entry<Core_Likes>(like).State = EntityState.Added;  //2添加赞的记录

                return db.SaveChanges() > 0;
            }
            return false;
        }


        /// <summary>
        /// 评论 用户话题
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="uid"></param>
        /// <param name="refUid">引用谁的评论</param>
        /// <param name="refUname">引用人的name</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ResultMessageViewModel Comment(Guid proId, Guid uid, string uname, string refUid, string refUname, string content, ref decimal hongbaoMoney)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (DbContext db = new LinXinApp20Entities())
            {
                int hongbaoResult = 1;  //没领红包为1 领取了红包就是2 
                var tp = db.Set<Core_Topic>().FirstOrDefault(o => o.T_Id == proId);
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);



                //发送话题的用户
                var recUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == tp.T_UserId);
                Guid giftId = Guid.Empty;
                DateTime dt = DateTime.Now;
                if (tp != null && tp.T_Hongbao == (int)LuckGiftFlagEnum.有红包)  //话题有红包，且红包未被领取完
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == proId);  //话题红包
                    giftId = hb.LG_Id;
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
                            LH_Remark = string.Format("领取用户邻里圈 话题：“{0}” 红包", tp.T_Title),
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
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = dt,
                            B_Remark = "用户邻友圈话题：" + tp.T_Title,
                            B_Money = money,
                            B_UId = user.U_Id,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Module = (int)BillEnum.红包,
                            B_OrderId = hisID,
                            B_Phone = user.U_LoginPhone,
                            B_Status = 0,
                            B_Title = "评论邻友圈，获得用户红包",
                            B_Type = (int)MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 添加评论者的红包账单

                        if (hb.LG_RemainCount == 0) //5红包被领光了,更改话题的红包标识
                        {
                            tp.T_Hongbao = (int)LuckGiftFlagEnum.红包被领光;
                        }
                        hongbaoMoney = money;
                        hongbaoResult = 2;
                    }
                }  //红包判断结束

                tp.T_Comments += 1;  //6累计话题的评论数
                db.Set<Core_Topic>().Attach(tp);
                db.Entry(tp).State = EntityState.Modified;

                Guid rId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid);
                Core_Comments com = new Core_Comments() { C_Content = content, C_Id = Guid.NewGuid(), C_ProjectId = proId, C_ProjectTitle = tp.T_Title, C_RefId = rId, C_RefName = refUname, C_State = 0, C_Time = dt, C_Type = (int)CommentTypeEnum.邻友圈话题, C_UserId = uid, C_UserName = uname, };
                db.Entry(com).State = EntityState.Added;  //7新增评论

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
                        C_UserName = uname,

                        hb = hongbaoResult != 2 ? null : new
                        {
                            money = hongbaoMoney,
                            tips = "恭喜发财",
                            headImg = recUser.U_Logo,
                            nickName = recUser.U_NickName,
                            giftId
                        }
                    };
                }
            }
            return rs;
        }
    }
}


