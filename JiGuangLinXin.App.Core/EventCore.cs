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
        /// ����Ƿ���Ч
        /// </summary>
        /// <param name="eid">�ID</param>
        /// <returns>true ��Ч false ��Ч</returns>
        public bool IsActivity(string eid)
        {
            Guid id;
            if (Guid.TryParse(eid, out id))
            {
                return base.LoadEntity(o => o.E_Id == id && o.E_Status == 0 && o.E_AuditingState == (int)AuditingEnum.��֤�ɹ� && o.E_STime < DateTime.Now && o.E_ETime > DateTime.Now) != null;
            }

            return false;
        }

        ///// <summary>
        ///// ��ȡ����Ȧ ��б�
        ///// </summary>
        ///// <param name="buildingId">����id</param>
        ///// <param name="pn">ҳ��</param>
        ///// <param name="rows">������</param>
        ///// <returns></returns>
        //        public IEnumerable<EventIndexViewModel> GetLinyouEvents(Guid? buildingId, int pn, int rows = 5)
        //        {

        //            //e_status=0  δɾ���� b_status==0 �̼�δ������
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

        //                if (rs.Any())//�����������
        //                {//����������е����е����б��Լ���ǰ4������

        //                    string commentsSql = "select top 4 C_Id,C_UserId,C_UserName,C_Content from Core_Comments where C_State = 0 and C_ProjectId=@projectId  order by  C_Time desc";
        //                    string likesSql = @"select l.L_Id,l.L_UserId,u.U_Logo,u.U_NickName,l.L_Time from Core_Likes as l
        //                                            inner join Core_User as u
        //                                            on l.L_UserId = u.U_Id
        //                                            where l.L_State = 0 and l.L_ProjectId=@projectId order by L_Time desc";
        //                    foreach (var item in rs)
        //                    {
        //                        //string a =item.s;
        //                        //1��ȡ ���ۼ���
        //                        Guid tid = item.E_Id;
        //                        item.CommentsList = db.Database.SqlQuery(typeof(object), commentsSql, new SqlParameter("@projectId", item.E_Id));

        //                        //2��ȡ ���޼���
        //                        item.LikesList = db.Database.SqlQuery(typeof(object), likesSql, new SqlParameter("@projectId", item.E_Id));

        //                    }
        //                    return rs;
        //                }
        //            }
        //            return null;
        //        }

        /// <summary>
        ///  ��ȡ�̼� �������С�����������ƽֱ̨�ӿ��Բ�ѯ�õ����������ӱ�
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
        /// �鿴����������ػ�����ģ����ޡ���������ۼ�¼
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public EventIndexViewModel GetOneEvent(Guid proId)
        {

            //t_status=0  δɾ���� u_status==0 �û�δ������
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
                    //1��ȡ ���ۼ���
                    item.CommentsList = db.Database.SqlQuery(typeof(object), commentsSql,
                        new SqlParameter("@projectId", proId));

                    //2��ȡ ���޼���
                    item.LikesList = db.Database.SqlQuery(typeof(object), likesSql,
                        new SqlParameter("@projectId", proId));

                    return item;
                }
            }
            return null;
        }





        /// <summary>
        /// ���� �̼һ
        /// </summary>
        /// <param name="proId">�ID</param>
        /// <param name="uid">�û�ID</param>
        /// <param name="proName">�����</param>
        /// <param name="nikeName">�û��ǳ�</param>
        /// <returns></returns>
        public bool Like(Guid proId, Guid uid, string proName, string nikeName)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var topic = db.Set<Core_Event>().FirstOrDefault(o => o.E_Id == proId);
                topic.E_Likes += 1;
                db.Set<Core_Event>().Attach(topic);
                db.Entry<Core_Event>(topic).State = EntityState.Modified;  //1�ۼ���

                Core_Likes like = new Core_Likes()
                {
                    L_Id = Guid.NewGuid(),
                    L_ProjectId = proId,
                    L_ProjectTitle = proName,
                    L_State = 0,
                    L_Time = DateTime.Now,
                    L_UserId = uid,
                    L_UserName = nikeName,
                    L_Type = (int)CommentTypeEnum.�̼һ
                };
                db.Entry<Core_Likes>(like).State = EntityState.Added;  //2����޵ļ�¼

                return db.SaveChanges() > 0;
            }
            return false;
        }



        /// <summary>
        /// ���� �̼һ
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="uid"></param>
        /// <param name="refUid">����˭������</param>
        /// <param name="refUname">�����˵�name</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool Comment(Guid proId, Guid uid, string nikeName, string refUid, string refUname, string content)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var tp = db.Set<Core_Event>().FirstOrDefault(o => o.E_Id == proId);
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);



                DateTime dt = DateTime.Now;
                if (tp != null)  //�����к�����Һ��δ����ȡ��
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == proId);  //������

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
                            LH_Money = hb.LG_Money,
                            LH_Remark = string.Format("��ȡ�̼ҡ�{0}������Ȧ�����{1}�� ���", tp.E_BusName, tp.E_Title),
                            LH_Status = 0,
                            LH_UserId = uid,
                            LH_UserNickName = user.U_NickName,
                            LH_UserPhone = user.U_LoginPhone,
                            LH_Flag = (int)LuckGiftTypeEnum.����Ȧ�û����
                        };
                        // 2. �����ȡ�ĺ����¼
                        db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;

                        //�����ȡ�� ���
                        var recUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);

                        var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                        balance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;   //3.�ۼƵ����


                        //��ӻ�Ա�˵�
                        Core_BillMember bill = new Core_BillMember() { B_Time = dt, B_Remark = "�̼�����Ȧ���" + tp.E_Title, B_Money = hb.LG_Money, B_UId = user.U_Id, B_Flag = (int)BillFlagEnum.��ͨ��ˮ, B_Module = (int)BillEnum.���, B_OrderId = hisID, B_Phone = user.U_LoginPhone, B_Status = 0, B_Title = "��������Ȧ�̼һ������̼Һ��", B_Type = (int)MemberRoleEnum.��Ա };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 ��������ߵĺ���˵�

                        if (hb.LG_RemainCount == 0) //5����������,���Ļ���ĺ����ʶ
                        {
                            //  tp.E_Hongbao = (int)LuckGiftFlagEnum.��������;
                        }

                    }
                }

                tp.E_Comments += 1;  //6�ۼƻ����������

                Guid rId = string.IsNullOrEmpty(refUid) ? Guid.Empty : Guid.Parse(refUid);
                Core_Comments com = new Core_Comments() { C_Content = content, C_Id = Guid.NewGuid(), C_ProjectId = proId, C_ProjectTitle = tp.E_Title, C_RefId = rId, C_RefName = refUname, C_State = 0, C_Time = dt, C_Type = (int)CommentTypeEnum.����Ȧ����, C_UserId = uid, C_UserName = nikeName };

                db.Entry(com).State = EntityState.Added;  //7��������

                return db.SaveChanges() > 0;

            }
            return false;
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="et">�����</param>
        /// <param name="items">ͶƱ��</param>
        /// <param name="images">ͼƬ�б�</param>
        /// <returns></returns>
        public bool AddOneEvent(Core_Event et, IEnumerable<dynamic> items, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Core_Event.Add(et);

                if (et.E_Flag == (int)EventFlagEnum.ͶƱ && items != null && items.Any())  //ͶƱ����
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

                        db.Core_EventVoteItem.Add(vi);  //���ͶƱ��Ŀ
                    }
                }

                if (images != null && images.Any())
                {
                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.E_Id;
                        am.A_Type = (int)AttachmentTypeEnum.ͼƬ;
                        am.A_Time = et.E_Date;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //�����Ŀ����
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }



        /// <summary>
        /// �༭��������
        /// </summary>
        /// <param name="et">�����</param>
        /// <param name="items">ͶƱ��</param>
        /// <param name="images">ͼƬ�б�</param>
        /// <returns></returns>
        public bool EditOneEvent(Core_Event et, IEnumerable<dynamic> items, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Set<Core_Event>().Attach(et);
                db.Entry(et).State = EntityState.Modified;  //1�޸Ļ

                //2ͶƱ���������Ҫ�޸Ļ������ɾ��֮ǰ���õ�ͶƱ��Ŀ
                if (et.E_Flag == (int)EventFlagEnum.ͶƱ && items != null && items.Any())
                {
                    //var tempList = db.Core_EventVoteItem.Where(o => o.I_EventId == et.E_Id); 

                    db.Core_EventVoteItem.Update(o => o.I_EventId == et.E_Id, o => new Core_EventVoteItem() { I_State = 1 });  //����ɾ��,�߼�

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

                        db.Core_EventVoteItem.Add(vi);  //���ͶƱ��Ŀ
                    }
                }
                //3�ͼƬ����������޸ģ�����ɾ��֮ǰ���õĻͼƬ
                if (images != null && images.Any())
                {
                    db.Sys_Attachment.Delete(o => o.A_PId == et.E_Id);  //����ɾ��������

                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.E_Id;
                        am.A_Type = (int)AttachmentTypeEnum.ͼƬ;
                        am.A_Time = et.E_Date;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //�����Ŀ����
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }


    }
}
