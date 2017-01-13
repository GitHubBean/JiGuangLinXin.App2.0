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
        /// ��ȡ ���� �Ļ���
        /// </summary>
        /// <param name="buildingId">С����ID</param>
        /// <param name="pn">ҳ��</param>
        /// <param name="rows">����</param>
        /// <returns></returns>
        public List<TopicIndexViewModel> GetLinyouTopics(Guid? buildingId, int pn, int rows = 10)
        {
            //t_status=0  δɾ���� u_status!=1 �û�δ������
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



            //sql = string.Format(sql, rows, (page-1)*rows);  //��ҳ��sql

            string where = "b.U_BuildingId='" + buildingId + "'";
            if (buildingId == null || Guid.Empty == buildingId)  //ȫ��������
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

                if (rs.Any())  //�����������
                {
                    //����������е����е����б��Լ���ǰ4������

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
                        //1��ȡ���� ���ۼ���
                        Guid tid = item.T_Id;
                        var cList =
                            db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();
                        item.CommentsList = null;
                        if (cList.Any())
                        {
                            item.CommentsList = cList;
                        }

                        //2��ȡ���� ���޼���
                        var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                        item.LikesList = null;
                        if (lList.Any())
                        {
                            item.LikesList = lList;
                        }

                        //3��ȡ���� �õ�������û��б�
                        item.HongbaoList = null;

                        if (item.T_Hongbao > 0)  //��ʶΪ����Ļ���
                        {
                            var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                            if (hList.Any())
                            {
                                item.HongbaoList = hList;

                            }
                        }

                        //4��ȡ�������б�
                        item.attachmentList = null;
                        if (item.T_ImgAttaCount > 1)  //�и���
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
        /// ��ȡ ���� �Ļ���
        /// </summary>
        /// <param name="uid">��ǰ�û�ID</param>
        /// <param name="buildingId">С����ID</param>
        /// <param name="pn">ҳ��</param>
        /// <param name="rows">����</param>
        /// <returns></returns>
        public List<TopicIndexViewModel> GetFriendsTopics(Guid uid, Guid? buildingId, int pn, int rows = 10)
        {
            //t_status=0  δɾ���� u_status!=1 �û�δ������
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

            //sql = string.Format(sql, rows, (page-1)*rows);  //��ҳ��sql

            string where = (buildingId == null || Guid.Empty == buildingId) ? "1=1" : "a.T_VillageId='" + buildingId + "'";
            using (DbContext db = new LinXinApp20Entities())
            {
                var rs =
                    db.Database.SqlQuery(typeof(TopicIndexViewModel),
                        (string.Format(sql, where, rows, (pn - 1) * rows, uid))).Cast<TopicIndexViewModel>().ToList();

                if (rs.Any())  //�����������
                {
                    //����������е����е����б��Լ���ǰ4������

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
                        //1��ȡ���� ���ۼ���
                        var cList =
                            db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();

                        item.CommentsList = null;
                        if (cList.Any())
                        {
                            item.CommentsList = cList;
                        }

                        //2��ȡ���� ���޼���
                        var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                        item.LikesList = null;
                        if (lList.Any())
                        {
                            item.LikesList = lList;
                        }

                        //3��ȡ���� �õ�������û��б�
                        item.HongbaoList = null;

                        if (item.T_Hongbao > 0)  //��ʶΪ����Ļ���
                        {
                            var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                            if (hList.Any())
                            {
                                item.HongbaoList = hList;

                            }
                        }

                        //4��ȡ�������б�
                        item.attachmentList = null;
                        if (item.T_ImgAttaCount > 1)  //�и���
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
        /// �鿴�����û����⣬���ػ�������ģ����ޡ���������ۼ�¼
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public TopicIndexViewModel GetOneTopic(Guid proId)
        {

            //t_status=0  δɾ���� u_status!=1 �û�δ������
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
                    //1��ȡ���� ���ۼ���
                    Guid tid = item.T_Id;
                    var cList =
                        db.Database.SqlQuery<CommentsIndexViewModel>(string.Format(commentsSql, item.T_Id)).ToList();
                    item.CommentsList = null;
                    if (cList.Any())
                    {
                        item.CommentsList = cList;
                    }

                    //2��ȡ���� ���޼���
                    var lList = db.Database.SqlQuery<LikeIndexViewModel>(string.Format(likesSql, item.T_Id)).ToList();
                    item.LikesList = null;
                    if (lList.Any())
                    {
                        item.LikesList = lList;
                    }

                    //3��ȡ���� �õ�������û��б�
                    var hList = db.Database.SqlQuery<HongbaoIndexViewModel>(string.Format(hongbaoSql, item.T_Id)).ToList();
                    item.HongbaoList = null;
                    if (item.T_Hongbao > 0) //��ʶΪ����Ļ���
                    {
                        item.HongbaoList = hList;
                    }

                    //4��ȡ�������б�
                    item.attachmentList = null;
                    if (item.T_ImgAttaCount > 1)  //�и���
                    {
                        item.attachmentList =
                            attCore.LoadEntities(p => p.A_PId == item.T_Id).OrderBy(o => o.A_Rank).ToList().Select(o => new
                            {
                                imgUrl = string.Format("{0}/{1}", o.A_Folder, o.A_FileName)
                            });
                    }

                    //1 ������������ۼ�
                    var pro = base.LoadEntity(o => o.T_Id == proId);
                    pro.T_Clicks += 1;
                    base.UpdateEntity(pro);  //���µ������

                    return item;
                }
            }
            return null;
        }


        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="topic">�������</param>
        /// <param name="hb">�������</param>
        /// <param name="ticket">Ʊ�ݶ���</param>
        /// <param name="phone">�ֻ���</param>
        /// <returns></returns>
        public ResultMessageViewModel PublishTopic(Core_Topic topic, Core_LuckyGift hb, Core_TopicMovieTicket ticket, string phone, string enPaypwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                if (hb != null)  //�к�����к����Ҫ��Ǯ��������ȯ�Ľ��,�������������
                {
                    Core_Balance balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == topic.T_UserId);//  �õ���Ա�˻�

                    if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                    {
                        rs.State = 2;
                        rs.Msg = "����δ����֧������,������ǰȥ���ã�";
                        return rs;
                    }
                    else
                    {
                        string payPwd = DESProvider.DecryptString(enPaypwd);  //֧������
                        payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// ����֧������
                        if (!balance.B_PayPwd.Equals(payPwd))
                        {
                            rs.Msg = "֧���������";
                            return rs;
                        }
                    }


                    if (balance.B_Balance < hb.LG_Money)
                    {
                        rs.Msg = "���㣬���ֵ";
                        return rs;
                    }
                    else  // ���Է����
                    {
                        //todo:�û������Ҫ�۳���Ŀǰ��û��Ҫ������֧�����룬���ڿ��ܻ���չ
                        //1��Ӻ��
                        db.Entry<Core_LuckyGift>(hb).State = EntityState.Added;
                        //2�����
                        balance.B_Balance = balance.B_Balance - hb.LG_Money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry<Core_Balance>(balance).State = EntityState.Modified;
                        //3����˵���ˮ
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.���,
                            B_Money = -hb.LG_Money,
                            B_OrderId = hb.LG_Id,
                            B_Phone = phone,
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = hb.LG_CreateTime,
                            B_Title = "�û�������Ȧ�������",
                            B_UId = hb.LG_UserId,
                            B_Type = (int)MemberRoleEnum.��Ա
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                    }
                }

                if (ticket != null)
                {
                    //���Ʊ������
                    db.Entry<Core_TopicMovieTicket>(ticket).State = EntityState.Added;
                }
                //��ӻ���
                db.Entry<Core_Topic>(topic).State = EntityState.Added;  //1��ӻ���


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
        /// ���� �û�����
        /// </summary>
        /// <param name="proId">����ID</param>
        /// <param name="uid">�û�ID</param>
        /// <param name="proName">�������</param>
        /// <param name="nikeName">�û��ǳ�</param>
        /// <returns></returns>
        public bool Like(Guid proId, Guid uid, string proName, string nikeName)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var topic = db.Set<Core_Topic>().FirstOrDefault(o => o.T_Id == proId);
                topic.T_Likes += 1;
                db.Set<Core_Topic>().Attach(topic);
                db.Entry<Core_Topic>(topic).State = EntityState.Modified;  //1�ۼ���

                Core_Likes like = new Core_Likes()
                {
                    L_Id = Guid.NewGuid(),
                    L_ProjectId = proId,
                    L_ProjectTitle = proName,
                    L_State = 0,
                    L_Time = DateTime.Now,
                    L_UserId = uid,
                    L_UserName = nikeName,
                    L_Type = (int)CommentTypeEnum.����Ȧ����
                };
                db.Entry<Core_Likes>(like).State = EntityState.Added;  //2����޵ļ�¼

                return db.SaveChanges() > 0;
            }
            return false;
        }


        /// <summary>
        /// ���� �û�����
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="uid"></param>
        /// <param name="refUid">����˭������</param>
        /// <param name="refUname">�����˵�name</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ResultMessageViewModel Comment(Guid proId, Guid uid, string uname, string refUid, string refUname, string content, ref decimal hongbaoMoney)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (DbContext db = new LinXinApp20Entities())
            {
                int hongbaoResult = 1;  //û����Ϊ1 ��ȡ�˺������2 
                var tp = db.Set<Core_Topic>().FirstOrDefault(o => o.T_Id == proId);
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);



                //���ͻ�����û�
                var recUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == tp.T_UserId);
                Guid giftId = Guid.Empty;
                DateTime dt = DateTime.Now;
                if (tp != null && tp.T_Hongbao == (int)LuckGiftFlagEnum.�к��)  //�����к�����Һ��δ����ȡ��
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == proId);  //������
                    giftId = hb.LG_Id;
                    var recHis =
                        db.Set<Core_LuckyGiftHistory>()
                            .FirstOrDefault(o => o.LH_UserId == uid && o.LH_GiftId == hb.LG_Id);  //��ȡ����ʷ��¼

                    if (hb != null && hb.LG_RemainCount > 0 && hb.LG_RemainMoney > 0 && recHis == null)  //�к���Ҹ��û���û����ȡ��
                    {

                        decimal money = 0;
                        if (hb.LG_RemainCount == 1)//ֻ��һ������ȫ���������
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

                        //1.������������
                        db.Set<Core_LuckyGift>().Attach(hb);
                        db.Entry(hb).State = EntityState.Modified;

                        //������ú�����û� �����¼
                        Guid hisID = Guid.NewGuid();
                        Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                        {
                            LH_CreateTime = dt,
                            LH_GiftDetailId = null,
                            LH_GiftId = hb.LG_Id,
                            LH_Id = hisID,
                            LH_Money = money,// hb.LG_Money,
                            LH_Remark = string.Format("��ȡ�û�����Ȧ ���⣺��{0}�� ���", tp.T_Title),
                            LH_Status = 0,
                            LH_UserId = uid,
                            LH_UserNickName = user.U_NickName,
                            LH_UserPhone = user.U_LoginPhone,
                            LH_Flag = (int)LuckGiftTypeEnum.����Ȧ�û����,
                            LH_UserLogo = user.U_Logo
                        };
                        // 2. �����ȡ�ĺ����¼
                        db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;

                        var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                        balance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;   //3.�ۼƵ����


                        //��ӻ�Ա�˵�
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = dt,
                            B_Remark = "�û�����Ȧ���⣺" + tp.T_Title,
                            B_Money = money,
                            B_UId = user.U_Id,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Module = (int)BillEnum.���,
                            B_OrderId = hisID,
                            B_Phone = user.U_LoginPhone,
                            B_Status = 0,
                            B_Title = "��������Ȧ������û����",
                            B_Type = (int)MemberRoleEnum.��Ա
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 ��������ߵĺ���˵�

                        if (hb.LG_RemainCount == 0) //5����������,���Ļ���ĺ����ʶ
                        {
                            tp.T_Hongbao = (int)LuckGiftFlagEnum.��������;
                        }
                        hongbaoMoney = money;
                        hongbaoResult = 2;
                    }
                }  //����жϽ���

                tp.T_Comments += 1;  //6�ۼƻ����������
                db.Set<Core_Topic>().Attach(tp);
                db.Entry(tp).State = EntityState.Modified;

                Guid rId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid);
                Core_Comments com = new Core_Comments() { C_Content = content, C_Id = Guid.NewGuid(), C_ProjectId = proId, C_ProjectTitle = tp.T_Title, C_RefId = rId, C_RefName = refUname, C_State = 0, C_Time = dt, C_Type = (int)CommentTypeEnum.����Ȧ����, C_UserId = uid, C_UserName = uname, };
                db.Entry(com).State = EntityState.Added;  //7��������

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
                            tips = "��ϲ����",
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


