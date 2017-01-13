using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.Core
{
    public class FeedbackCore : BaseRepository<Core_Feedback>
    {
        /// <summary>
        /// ϵͳ�����������˿�
        /// </summary>
        /// <param name="id">����ID</param>
        /// <param name="content">��ע����</param>
        /// <param name="source">������Դ</param>
        /// <param name="orderNo">������</param>
        /// <returns></returns>
        public bool BackOrderMoney(int id, string content, int source, string orderNo)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var fb = db.Core_Feedback.FirstOrDefault(o => o.F_Id == id);
                if (fb != null)
                {

                    Guid on = Guid.Empty;  // ��ˮ�˵���������Ŀ����
                    decimal money = 0;
                    if (source == 1) //�ۺ�
                    {
                        on = Guid.Parse(orderNo);

                        var order = db.Core_JuheOrder.FirstOrDefault(o => o.O_Id == on && o.O_Status == (int)PayOffEnum.����ʧ��);  //ֻ������ʧ�ܵĲ��ֶ��˿�
                        order.O_Status = (int)PayOffEnum.���˿�;  //���¶���״̬

                        db.Set<Core_JuheOrder>().Attach(order);
                        db.Entry(order).State = EntityState.Modified;
                        money = order.O_Money;

                        //�û��Ķ���״̬����Ϊ��Ч��ɾ����
                        //var bill = db.Core_BillMember.FirstOrDefault(o=>o.B_OrderId == on && o.B_Status == 0);

                    }

                    if (source == 0) //����
                    {
                        var order = db.Core_ChinapayOrder.FirstOrDefault(o => o.C_ordId == orderNo.ToString() && o.D_ordStat == "221" && o.D_scBillStat == "3");  //ֻ������ʧ�ܵĲ��ֶ�����
                        order.D_ordStat = "000";  //���¶���״̬
                        order.D_scBillStat = "1";
                        order.D_billDate = DateTime.Now.ToString();

                        db.Set<Core_ChinapayOrder>().Attach(order);
                        db.Entry(order).State = EntityState.Modified;

                        money = Convert.ToDecimal(order.B_billAmt);
                        on = order.D_Id;
                    }



                    if (money > 0)
                    {
                        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == fb.F_UId);

                        //1�ۼ��û����ʺ����
                        balance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;




                        //2.����ʺŵ���ˮ�˵�
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = (int)BillFlagModuleEnum.�ٷ�ƽ̨,
                            B_Module = (int)BillEnum.�����˿�,
                            B_Money = money,
                            B_OrderId = on,
                            B_Phone = fb.F_Phone,
                            B_Remark = source == 1 ? "���ٷ������˿�,��ע��" + content : "�����������˿�,��ע��" + content,
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = source == 1 ? "���ٷ������˿�" : "�����������˿�",
                            B_UId = fb.F_UId,
                            B_Type = (int)MemberRoleEnum.��Ա
                        };

                        //3.����״̬
                        fb.F_Status = 1;  //�Ѵ���
                        fb.F_ReplyTime = DateTime.Now;
                        fb.F_Reply = content;

                        db.Set<Core_Feedback>().Attach(fb);
                        db.Entry(fb).State = EntityState.Modified;
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;

                        if (db.SaveChanges() > 0)
                        {
                            return true;
                        }
                    }





                }
            }

            return false;
        }
    }
}
