using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class LuckyGiftCore : BaseRepository<Core_LuckyGift>
    {

        #region ����
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="money">�ܽ��</param>
        /// <param name="count">���ٸ�</param>
        /// <returns></returns>
        public double CalcGift(double money, int count)
        {
            //���ٱ�֤�����쵽 0.01
            if (money - 0.01 * count > 0)
            {
                double cur = GetRandom(money - 0.01 * count, 0.01);
                while (cur >= (money / count) * 2)  //���ܴ��� ƽ����������
                {
                    cur = GetRandom(money - 0.01 * count, 0.01);
                }
                return cur;
            }
            return 0.01;
        }
        /// <summary>
        /// ��ȡ�����
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public double GetRandom(double max, double min)
        {
            Random random = new Random();
            return Math.Round(random.NextDouble() * (max - min) + min, 2);
        }


        #endregion



        #region ���


        /// <summary>
        /// ��Ⱥ���
        /// </summary>
        /// <param name="money">������</param>
        /// <param name="count">��ָ���</param>
        /// <param name="uid">�û�id</param>
        /// <param name="tips">���ף����</param>
        /// <param name="remark">��ע���磬�������</param>
        /// <returns></returns>
        public ResultMessageViewModel SendGiftGroup(double money, int count, Guid uid, string remark, string tips, string enPaypwd)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();

            if (money <= 0)
            {
                result.State = 1;
                result.Msg = "������������0";
                return result;
            }

            if (money > 200)
            {
                result.State = 1;
                result.Msg = "������ܳ���200Ԫ";
                return result;
            }

            using (DbContext db = new LinXinApp20Entities())
            {
                //���������
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "�û�������";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.����)
                {
                    result.State = 1;
                    result.Msg = "�û��ѱ�����";
                    return result;

                }

                //�ж��û��Ľ���Ƿ����
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);

                if (string.IsNullOrEmpty(userBalance.B_PayPwd) || string.IsNullOrEmpty(userBalance.B_EncryptCode))
                {
                    result.State = 2;
                    result.Msg = "����δ����֧������,������ǰȥ���ã�";
                    return result;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //֧������
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + userBalance.B_EncryptCode);// ����֧������
                    if (!userBalance.B_PayPwd.Equals(payPwd))
                    {
                        result.Msg = "֧���������";
                        return result;
                    }
                }

                if (userBalance.B_Balance < Convert.ToDecimal(money))  //����
                {
                    result.State = 1;
                    result.Msg = "����";
                    return result;
                }

                /*****���Ϸ����������֤����*****/

                Guid gId = Guid.NewGuid();
                DateTime curTime = DateTime.Now;

                //��1���۳����,�޸Ļ�Ա��
                userBalance.B_Balance = userBalance.B_Balance - Convert.ToDecimal(money);
                db.Set<Core_User>().Attach(user);
                db.Entry<Core_User>(user).State = EntityState.Modified;

                //��2����ӻ�Ա�˵���
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = remark,
                    B_Money = Convert.ToDecimal(-money),
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.���,
                    B_OrderId = gId,
                    B_Type = (int)MemberRoleEnum.��Ա,
                    B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                    B_Status = 0
                };
                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//����˵���ϸ ���ύ



                //��3����¼���͵ĺ����Ϣ
                Core_LuckyGift gift = new Core_LuckyGift()
                {
                    LG_Id = gId,
                    LG_Title = tips,
                    LG_Tags = "",
                    LG_Type = (int)LuckGiftTypeEnum.Ⱥ���,
                    LG_UserId = uid,
                    LG_ProjectId = user.U_BuildingId,
                    L_ProjectTitle = user.U_BuildingName,
                    LG_Money = Convert.ToDecimal(money),
                    LG_RemainMoney = Convert.ToDecimal(money),
                    LG_Count = count,
                    LG_RemainCount = count,
                    LG_CreateTime = curTime,
                    LG_Flag = null,//user.U_BuildingId,
                    LG_Remark = remark,
                    LG_Status = 0,
                    LG_AreaCode = user.U_AreaCode,
                    LG_VillageId = user.U_BuildingId
                };
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Added;//��Ӻ�� ���ύ

                #region �����㷨
                /*
                //int temp = 0;
                //for (int i = 1; i <= count - 1; i++)
                //{
                //    Random ran = new Random();
                //    var giftmoney = ran.Next(1, (money * 100 - temp) / (count + 1 - i));
                //    temp += giftmoney;

                //    LuckyGiftDetail detail = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(giftmoney * 0.01), LD_State = 0, LD_Uid = 0 };
                //    db.Entry<LuckyGiftDetail>(detail).State = EntityState.Added;//��Ӻ����ϸ ���ύ
                //}
                ////������һ�����
                //LuckyGiftDetail detailLast = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal((money * 100 - temp) * 0.01), LD_State = 0, LD_Uid = 0 };



                try
                {

                    double min = 0.01;//ÿ�����������յ�0.01Ԫ  
                    double t_max = 0, t_min = 0;
                    double safe_total, moneyTemp;

                    Random rand = new Random(DateTime.Now.Millisecond);
                    for (int i = 1; i < count; i++)
                    {
                        safe_total = (money - (count - i) * min) / (count - i);//�����ȫ����  

                        double ztfb_u = money / count;//��̬�ֲ�����

                        moneyTemp = rand.Next((int)(min * 100), (int)(safe_total * 100)) / 100.0d;
                        if (money > t_max) t_max = money;
                        if (i == 1) t_min = t_max;
                        if (money < t_min) t_min = money;

                        money = money - moneyTemp;
                        LuckyGiftDetail detail = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(moneyTemp.ToString("0.00")), LD_State = 0, LD_Uid = 0 };
                        db.Entry<LuckyGiftDetail>(detail).State = EntityState.Added;//��Ӻ����ϸ ���ύ
                    }

                    //������һ�����
                    LuckyGiftDetail detailLast = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(money.ToString("0.00")), LD_State = 0, LD_Uid = 0 };
                    db.Entry<LuckyGiftDetail>(detailLast).State = EntityState.Added;//��Ӻ����ϸ ���ύ

                    db.SaveChanges();//��Ԫ�����������ύ


                    result.State = 0;
                    result.Msg = "С������ѷ��ɹ�";
                    result.Data = new { giftId = gId };
                    return result;
                }
                catch (Exception ex)
                {
                    result.State = 1;
                    result.Msg = "ÿ������Ľ������Ŷ";
                    return result;
                }
                 */
                #endregion


                bool bl = db.SaveChanges() > 0;//��Ԫ�����������ύ
                if (bl)
                {
                    result.State = 0;
                    result.Msg = "ok";
                    result.Data = new { giftId = gId };
                }
            }

            return result;
        }


        /// <summary>
        /// �������
        /// </summary>
        /// <param name="money">������</param>
        /// <param name="uid">�������Id</param>
        /// <param name="tUid">�Ӻ���Ļ�ԱId( ����id)</param>
        /// <param name="tips">�����ע</param>
        /// <returns></returns>
        public ResultMessageViewModel SendGiftSingle(decimal money, Guid uid, Guid tUid, string tips, string remark, string enPaypwd)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            if (money <= 0)
            {
                result.State = 1;
                result.Msg = "������������0";
                return result;
            }
            if (money > 200)
            {
                result.State = 1;
                result.Msg = "������ܳ���200Ԫ";
                return result;
            }

            Guid gId = Guid.NewGuid();
            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "�û��ʺŲ�����";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.����)
                {
                    result.State = 1;
                    result.Msg = "�û��ʺ��ѱ�����";
                    return result;

                }

                //�Ӻ���û�
                var targetUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == tUid && o.U_Status != (int)UserStatusEnum.����); //�պ���Ļ�Ա
                if (targetUser == null)
                {
                    result.State = 1;
                    result.Msg = "�պ�����û��ʺŲ�����";
                    return result;

                }
                //�ж��û��Ľ���Ƿ����
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);


                if (string.IsNullOrEmpty(userBalance.B_PayPwd) || string.IsNullOrEmpty(userBalance.B_EncryptCode))
                {
                    result.State = 2;
                    result.Msg = "����δ����֧������,������ǰȥ���ã�";
                    return result;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //֧������
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + userBalance.B_EncryptCode);// ����֧������
                    if (!userBalance.B_PayPwd.Equals(payPwd))
                    {
                        result.Msg = "֧���������";
                        return result;
                    }
                }


                if (userBalance.B_Balance < Convert.ToDecimal(money))  //����
                {
                    result.State = 1;
                    result.Msg = "����";
                    return result;
                }
                Guid targetUid = targetUser.U_Id;  //Ŀ���û�id
                /*****���Ϸ����������֤����*****/

                DateTime curTime = DateTime.Now;

                //��1���۳����,�޸Ļ�Ա��
                userBalance.B_Balance -= money;
                db.Set<Core_User>().Attach(user);
                db.Entry<Core_User>(user).State = EntityState.Modified;

                //��2����ӻ�Ա�˵���
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = remark,
                    B_Money = Convert.ToDecimal(-money),
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.���,
                    B_OrderId = gId,
                    B_Type = (int)MemberRoleEnum.��Ա,
                    B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                    B_Status = 0
                };
                db.Entry<Core_BillMember>(bill).State = EntityState.Added; //����˵���ϸ ���ύ


                //��� ��� �� ����
                Core_LuckyGift gift = new Core_LuckyGift()
                {
                    LG_Id = gId,
                    LG_Title = tips,
                    LG_Tags = "",
                    LG_Type = (int)LuckGiftTypeEnum.�������,
                    LG_UserId = uid,
                    LG_ProjectId = user.U_BuildingId,
                    L_ProjectTitle = user.U_BuildingName,
                    LG_Money = Convert.ToDecimal(money),
                    LG_RemainMoney = Convert.ToDecimal(money),
                    LG_Count = 1,
                    LG_RemainCount = 1,
                    LG_CreateTime = curTime,
                    LG_Flag = targetUid,   //�պ�����û�ID
                    LG_Remark = remark,
                    LG_Status = 0,
                    LG_AreaCode = user.U_AreaCode,
                    LG_VillageId = user.U_BuildingId,

                };
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Added;//��Ӻ�� ���ύ

                var bl = db.SaveChanges() > 0;//��Ԫ�����������ύ
                if (bl)
                {
                    result.State = 0;
                    result.Msg = "ok";
                    result.Data = new { giftId = gId, uid = uid };
                }

            }

            return result;

        }





        /// <summary>
        ///  ��Ա�����(Ⱥ��)
        /// </summary>
        /// <param name="uid">��ԱId</param>
        /// <param name="groupId">����Id</param>
        /// <param name="giftId">���Id</param>
        /// <param name="giftTips">���Tips</param>
        /// <returns></returns>
        public ResultMessageViewModel ReceiveGiftGroup(Guid uid, Guid groupId, Guid giftId, string giftTips)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid); //������Ļ�Ա
                //��˻�Ա״̬
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "�û�������";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.����)
                {
                    result.State = 1;
                    result.Msg = "�û��ʺ��ѱ�����";
                    return result;

                }

                if (user.U_BuildingId != groupId)
                {
                    result.State = 1;
                    result.Msg = "�û��ʺŲ��ڴ�����";
                    return result;
                }

                //�����ĺ��
                var gift = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_Id == giftId);
                var sendUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == gift.LG_UserId);

                if (gift.LG_Status == (int)LuckGiftStateEnum.�ѹ���)
                {
                    result.State = 1;
                    result.Msg = "�ѹ���";
                    result.Data = new { money = 0, tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };
                    return result;
                }
                if (user.U_BuildingId != gift.LG_VillageId)
                {
                    result.State = 1;
                    result.Msg = "���û����ڴ�����";
                    return result;
                }

                if (gift.LG_RemainCount < 1)
                {
                    result.State = 1;
                    result.Msg = "����ѱ����";
                    result.Data = new { money = 0, tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };

                    return result;
                }

                //�������¼���鿴�Ƿ�����ȡ��
                var giftHistory = db.Set<Core_LuckyGiftHistory>().FirstOrDefault(o => o.LH_GiftId == giftId && o.LH_UserId == uid);
                if (giftHistory != null)
                {
                    result.State = 2;
                    result.Msg = "����ȡ����";
                    result.Data =
                        new
                        {
                            money = giftHistory.LH_Money.ToString("N"),
                            tips = gift.LG_Title,
                            headImg = sendUser.U_Logo,
                            nickName = sendUser.U_NickName
                        };
                    return result;
                }

                DateTime curTime = DateTime.Now;

                /******����������ʸ�******/
                //�����һ�����

                decimal receiveMoney = 0;
                if (gift.LG_RemainCount == 1)  //ֻ��һ���������ֱ�ӷ���
                {
                    receiveMoney = gift.LG_RemainMoney;
                }
                else
                {
                    receiveMoney = Convert.ToDecimal(CalcGift(Convert.ToDouble(gift.LG_RemainMoney), gift.LG_RemainCount));
                }

                Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                {
                    LH_Id = Guid.NewGuid(),
                    LH_UserId = uid,
                    LH_UserNickName = user.U_NickName,
                    LH_UserPhone = user.U_LoginPhone,
                    LH_CreateTime = curTime,
                    LH_Flag = (int)LuckGiftTypeEnum.Ⱥ���,
                    LH_GiftDetailId = null,
                    LH_GiftId = gift.LG_Id,
                    LH_Money = receiveMoney,
                    LH_Remark = giftTips,
                    LH_Status = 0,
                    LH_UserLogo = user.U_Logo
                };
                db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;// ��1�������ȡ��¼ ���ύ

                //��2��׷���û����
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                userBalance.B_Balance += receiveMoney;
                db.Set<Core_Balance>().Attach(userBalance);
                db.Entry<Core_Balance>(userBalance).State = EntityState.Modified;

                //��3����ӻ�Ա�˵���
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = "����һ������Ⱥ���",
                    B_Money = receiveMoney,
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.���,
                    B_OrderId = gift.LG_Id,
                    B_Type = (int)MemberRoleEnum.��Ա,
                    B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                    B_Status = 0
                };

                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//����˵���ϸ ���ύ


                //��4����ȡһ������󣬼��ٺ����ʣ������,��ʣ�����
                gift.LG_RemainCount -= 1;
                gift.LG_RemainMoney -= receiveMoney;
                db.Set<Core_LuckyGift>().Attach(gift);
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Modified;
                //var cc = db.GetValidationErrors();
                bool bl = db.SaveChanges() > 0;//��Ԫ�����������ύ

                if (bl)//���ݲ����ɹ�
                {
                    //���ط������Ա����Ϣ
                    var dataStr =
                        new
                        {
                            money = receiveMoney.ToString("N"),
                            tips = gift.LG_Title,
                            headImg = sendUser.U_Logo,
                            nickname = sendUser.U_NickName
                        };
                    result.State = 0;
                    result.Msg = "�����ȡ�ɹ�";
                    result.Data = dataStr;
                }
            }

            return result;
        }



        /// <summary>
        ///  ��Ա�պ��(���Ե�)
        /// </summary>
        /// <param name="uid">�պ����Ա��Id</param>
        /// <param name="giftId">���Id</param>
        /// <param name="giftTips">���Tips</param>
        /// <returns></returns>
        public ResultMessageViewModel ReceiveGiftSingle(Guid uid, Guid giftId, string giftTips)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid); //������Ļ�Ա
                //��˻�Ա״̬
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "��������û�������";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.����)
                {
                    result.State = 1;
                    result.Msg = "�û��ѱ�����";
                    return result;

                }

                //���������
                //var receiveGift = db.Set<LuckyGiftDetail>().FirstOrDefault(o => o.LD_GiftId == giftId);
                //if (receiveGift == null)
                //{
                //    result.Status = 1;
                //    result.Msg = "���������";
                //    return result;
                //}
                //else if (receiveGift.LD_Status != (int)GiftStatusEnum.����ȡ)
                //{
                //    result.Status = 1;
                //    result.Msg = "�������ȡ";
                //    result.Data = new { money = receiveGift.LD_Money, tips = gift.LG_Title };
                //    return result;
                //}

                //�պ��
                var gift = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_Id == giftId); 
                //���ط������Ա����Ϣ
                var sendUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == gift.LG_UserId);

                if (gift != null && gift.LG_RemainCount > 0)
                {

                    if (gift.LG_UserId == uid)  //�Լ����Լ��ĺ������ʾ������ѷ�����
                    {
                        result.State = 1;
                        result.Msg = "����ѷ���";
                        result.Data =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        return result;

                    }

                    if ( gift.LG_Type != (int)LuckGiftTypeEnum.������� || gift.LG_Flag != uid)
                    {
                        result.State = 1;
                        result.Msg = "����ѹ���";//��������ڣ���ʾ�ĳɡ��ѹ��ڡ�
                        return result;
                    }

                    if (gift.LG_Status == (int)LuckGiftStateEnum.�ѹ���)
                    {
                        result.State = 1;
                        result.Msg = "�ѹ���";
                        result.Data =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        return result;
                    }

                    DateTime curTime = DateTime.Now;

                    /******���պ�����ʸ�******/
                    Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                    {
                        LH_Id = Guid.NewGuid(),
                        LH_UserId = uid,
                        LH_UserNickName = user.U_NickName,
                        LH_UserPhone = user.U_LoginPhone,
                        LH_CreateTime = curTime,
                        LH_Flag = (int)LuckGiftTypeEnum.�������,
                        LH_GiftDetailId = null,
                        LH_GiftId = gift.LG_Id,
                        LH_Money = gift.LG_Money,
                        LH_Remark = giftTips,
                        LH_Status = 0,
                        LH_UserLogo = user.U_Logo
                    };


                    db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;// ��1�������ȡ��¼ ���ύ

                    //��2��׷���û����
                    var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                    userBalance.B_Balance += gift.LG_Money;
                    db.Set<Core_Balance>().Attach(userBalance);
                    db.Entry<Core_Balance>(userBalance).State = EntityState.Modified;

                    //��3����ӻ�Ա�˵���
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = curTime,
                        B_Title = string.Format("�յ����ѣ�{0} ��������", sendUser.U_NickName),
                        B_Money = gift.LG_Money,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.���,
                        B_OrderId = gift.LG_Id,
                        B_Type = (int)MemberRoleEnum.��Ա,
                        B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                        B_Status = 0
                    };

                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;//����˵���ϸ ���ύ


                    //��4����ȡһ������󣬼��ٺ����ʣ��������ʣ�����
                    gift.LG_RemainCount = 0;
                    gift.LG_RemainMoney = 0;
                    db.Set<Core_LuckyGift>().Attach(gift);
                    db.Entry<Core_LuckyGift>(gift).State = EntityState.Modified;

                    bool bl = db.SaveChanges() > 0;//��Ԫ�����������ύ

                    if (bl)//���ݲ����ɹ�
                    {
                        //���ط������Ա����Ϣ
                        var dataStr =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        result.State = 0;
                        result.Msg = "�����ȡ�ɹ�";
                        result.Data = dataStr;
                    }
                }
                else
                {
                    result.State = 1;
                    result.Msg = "�������ȡ";
                    result.Data = new { money = gift.LG_Money.ToString("N"), tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };
                    return result;
                }
            }

            return result;
        }

        #endregion

    }
}
