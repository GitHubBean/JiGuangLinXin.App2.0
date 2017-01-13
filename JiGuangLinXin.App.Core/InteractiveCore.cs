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
        /// ����Ȧ ��ҳ - ���������б�
        /// </summary>
        /// <param name="buildingId">����ID��Ϊnull����ʶȫ����</param>
        /// <param name="pn">ҳ��</param>
        /// <param name="rows">����</param>
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
        /// �û������������
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
                if (hb != null)  //�к�����к����Ҫ��Ǯ��������ȯ�Ľ��,�������������
                {
                    Core_Balance balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == obj.I_UserId);//  �õ���Ա


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
                        Core_BillMember bill = new Core_BillMember() { B_Flag = 0, B_Module = (int)BillEnum.���, B_Money = hb.LG_Money, B_OrderId = hb.LG_Id, B_Phone = phone, B_Remark = "", B_Status = 0, B_Time = hb.LG_CreateTime, B_Title = "�û������������������", B_UId = hb.LG_UserId, B_Type = (int)MemberRoleEnum.��Ա };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                    }
                }

                if (voteList != null)  //��ͶƱ��Ŀ
                {
                    foreach (var vote in voteList)
                    {
                        //4���ͶƱ��
                        db.Entry<Core_EventVoteItem>(vote).State = EntityState.Added;
                    }
                }
                //��ӻ���
                db.Entry<Core_Interactive>(obj).State = EntityState.Added;  //5��ӻ���


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
        /// �鿴����������
        /// </summary>
        /// <param name="activeId">��������ID</param>
        /// <param name="uid">�û���ID</param>
        /// <returns></returns>
        public ResultMessageViewModel View(Guid activeId, Guid uid)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var info = db.Set<Core_Interactive>().FirstOrDefault(o => o.I_Id == activeId);

                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);

                dynamic gift = null;  //���
                if (info.I_Hongbao != (int)LuckGiftFlagEnum.û�к��) //�к�����ͷֺ����
                {
                    var hb = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_ProjectId == activeId);
                    var recHis =
                        db.Set<Core_LuckyGiftHistory>()
                            .FirstOrDefault(o => o.LH_UserId == uid && o.LH_GiftId == hb.LG_Id);  //��ȡ����ʷ��¼

                    decimal money = -1;  //���õĺ�����
                    if (hb != null)//�������
                    {

                        if (hb.LG_RemainCount > 0 && recHis == null) //��δ��ȡ��������Һ������ʣ��
                        {

                            //todo:�����������Ҫ��Ҫ���͸���Ϣ�أ�
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
                                LH_CreateTime = DateTime.Now,
                                LH_GiftDetailId = null,
                                LH_GiftId = hb.LG_Id,
                                LH_Id = hisID,
                                LH_Money = money,
                                LH_Remark = string.Format("��ȡ������������{0}�� ���", info.I_Title),
                                LH_Status = 0,
                                LH_UserId = uid,
                                LH_UserNickName = user.U_NickName,
                                LH_UserPhone = user.U_LoginPhone,
                                LH_Flag = (int)LuckGiftTypeEnum.����Ȧ�û����,
                                LH_UserLogo = user.U_Logo
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
                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Remark = "��ȡ����������" + info.I_Title,
                                B_Money = money,
                                B_UId = user.U_Id,
                                B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                                B_Module = (int)BillEnum.���,
                                B_OrderId = hisID,
                                B_Phone = user.U_LoginPhone,
                                B_Status = 0,
                                B_Title = "������������������û����",
                                B_Type = (int)MemberRoleEnum.��Ա
                            };
                            db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4 ��������ߵĺ���˵�

                            if (hb.LG_RemainCount == 0) //5����������,���Ļ���ĺ����ʶ
                            {
                                info.I_Hongbao = (int)LuckGiftFlagEnum.��������;
                            }
                        }
                        else if (recHis != null)  //�Ѿ���ȡ�������
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



                }//�������
                dynamic voteItem = null;  //ͶƱ�����б�
                if (info.I_Flag == (int)InteractiveFlagEnum.ͶƱ����)  //ͶƱ����
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
                info.I_Clicks += 1;  //�ۼ������
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
