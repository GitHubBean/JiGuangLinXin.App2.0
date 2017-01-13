using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class JuheOrderCore : BaseRepository<Core_JuheOrder>
    {

        /// <summary>
        /// ����ɷ�
        /// </summary>
        /// <param name="order">�ۺϵĶ���</param>
        /// <returns></returns>
        public ResultMessageViewModel Payment(Core_JuheOrder order, string enPaypwd, string moneyList = "")
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //��ѯ���
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == order.O_UId);

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

                if (balance.B_Balance >= order.O_Money)  //����㹻�ɷ�
                {
                    //1�����û����ʺ����
                    balance.B_Balance -= order.O_Money;
                    db.Set<Core_Balance>().Attach(balance);
                    db.Entry(balance).State = EntityState.Modified;

                    //2.����ʺŵ���ˮ�˵�
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagModuleEnum.�ٷ�ƽ̨,
                        B_Money = -order.O_Money,
                        B_OrderId = order.O_Id,
                        B_Phone = order.O_Phone,
                        B_Remark = order.O_Remark,
                        B_Status = 0,
                        B_Time = order.O_Time,
                        B_Module = (int)BillEnum.����ɷ�,
                        B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type) + ":" + order.O_OrderNo,
                        B_UId = order.O_UId,
                        B_Type = (int)MemberRoleEnum.��Ա
                    };

                    var moneyArr = moneyList.Split('|');
                    bool flag = false;
                    foreach (var item in moneyArr)  //ͨ���ۿ۽��õ�ʵ�ʵ��˽��
                    {
                        var tem = item.Split(',');
                        if (Convert.ToDecimal(tem[0]) == order.O_Money) //���۽��Ϸ�
                        {
                            order.O_Money = Convert.ToDecimal(tem[1]);  //ʵ�ʵĶ�������ԭ��
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        rs.Msg = "���ݷ����쳣���������APP��лл��";
                        return rs;
                    }


                    #region 85�۳�ֵ �
                    /**
                       var bl = db.Core_BillMember.FirstOrDefault(o => o.B_UId == order.O_UId && o.B_Module == (int)BillEnum.���ѳ�ֵ85�� && o.B_Status == 0);

                    if (bl != null)   //�Ѿ����ܹ�85�۳�ֵ�Ż�
                    {
                        bill.B_Module = (int)BillEnum.����ɷ�;
                    }
                    else
                    {

                        bill.B_Module = (int)BillEnum.���ѳ�ֵ85��;
                        bill.B_Remark += "�����ѳ�ֵ85���Żݡ�";

                        //order.O_Money = Convert.ToDecimal((order.O_Money / Convert.ToDecimal(0.85)).ToString("F0"));  //����85��
                        order.O_Remark += "�����ѳ�ֵ85���Żݡ�";

                        var moneyArr = moneyList.Split('|');
                        bool flag = false;
                        foreach (var item in moneyArr)  //ͨ���ۿ۽��õ�ʵ�ʵ��˽��
                        {
                            var tem = item.Split(',');
                            if (Convert.ToDecimal(tem[0]) == order.O_Money) //���۽��Ϸ�
                            {
                                order.O_Money = Convert.ToDecimal(tem[1]);  //����85�۽ɷ�,����ʵ�ʵĶ�������ԭ��
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            rs.Msg = "���ݷ����쳣���������APP��лл��";
                            return rs;
                        }
                    }
                     * **/
                    #endregion


                    int juInt = 0;
                    if (int.TryParse(order.O_Money.ToString(), out juInt)) //��ֵ������int����
                    {
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //���ҵ���˵�
                        //3.��Ӿۺ��˵�
                        db.Entry<Core_JuheOrder>(order).State = EntityState.Added;
                        if (db.SaveChanges() > 0)
                        {
                            rs.State = 0;
                            rs.Msg = juInt.ToString();
                            return rs;
                        }
                    }
                    else
                    {
                        rs.Msg = "���ݷ����쳣���������APP��лл��";
                        return rs;
                    }
                }
                else
                {
                    rs.Msg = "�û����˻����㣡";
                }
            }
            return rs;
        }


        /// <summary>
        /// ����ʧ�ܣ��˿�
        /// </summary>
        /// <param name="order">���ʵĶ���</param>
        /// <returns></returns>
        public bool WriteOffFail(Core_JuheOrder order,string source = "��android��")
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var od = db.Core_JuheOrder.FirstOrDefault(o => o.O_Id == order.O_Id);
                if (od != null)
                {
                    od.O_Status = (int)PayOffEnum.����ʧ��;  //���¶���״̬

                    db.Set<Core_JuheOrder>().Attach(od);
                    db.Entry(od).State = EntityState.Modified;



                    //�������ʧ�ܣ�֪ͨ����Ա����
                    Core_Feedback fb = new Core_Feedback()
                    {
                        F_Content =
                            string.Format("�ٷ�(�ۺ�)�ӿڱ���֧���ɹ���������������ʧ�ܡ�#��ˮ�Ŷ����ţ�{0},���ڣ�{1},֧�����:{2}��Ӧ�ɽ��{3}#,{4}",
                                order.O_Id, order.O_Time, order.O_Money, order.O_Money,source),
                        F_Flag = (int)FeedbackEnum.ϵͳ,
                        F_Phone = order.O_Phone,
                        F_Status = 0,
                        F_Time = DateTime.Now,
                        F_Title = "�ۺϱ���ɷѣ�֧���ɹ�������ʧ��",
                        F_UId = order.O_UId
                    };


                    db.Entry(fb).State = EntityState.Added;
                    return db.SaveChanges() > 0;
                }
                /*  �����߼�  ���������ʧ�ܣ�֪ͨ����Ա����
                var ban = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId== order.O_UId);
                if (ban != null)
                {
                    ban.B_Balance += order.O_Money;
                    db.Set<Core_Balance>().Attach(ban);
                    //1�������
                    db.Entry(ban).State = EntityState.Modified;


                    //2.����ʺŵ���ˮ�˵�
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                        B_Module = (int)BillEnum.����ɷѷ���,
                        B_Money = order.O_Money,
                        B_OrderId = order.O_Id,
                        B_Phone = order.O_Phone,
                        B_Remark = order.O_Remark,
                        B_Status = 0,
                        B_Time = order.O_Time,
                        B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type),
                        B_UId = order.O_UId,
                        B_Type = (int)MemberRoleEnum.��Ա
                    };

                     
                    return db.SaveChanges() > 0;

                }
                */
            }

            return false;
        }

    }
}
