using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    /// <summary>
    /// ��������
    /// </summary>
    public class ChinapayOrderCore : BaseRepository<Core_ChinapayOrder>
    {
        /// <summary>
        /// �������ʳɹ���1����ɷѵ�֧���������ļ�¼״̬Ϊ���� 2����������ʼ�¼ 3���֧����֧���Ľ��������ʵĽ������Ǯ�������
        /// </summary>
        /// <param name="alipay">֧�����Ķ�����</param>
        /// <param name="order">������������</param>
        /// <returns></returns>
        public int OrderWriteoff(string alipay, Core_ChinapayOrder order)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var aliOrder = db.Set<Core_AlipayOrder>().FirstOrDefault(o => o.A_OrderNo == alipay);
                aliOrder.A_Status = 0;// 1��ʶ����
                db.Set<Core_AlipayOrder>().Attach(aliOrder);
                db.Entry<Core_AlipayOrder>(aliOrder).State = EntityState.Modified;


                order.D_Id = Guid.NewGuid();
                order.D_PayBillId = aliOrder.A_Id.ToString(); //�������ʵ�֧�����ʺ�id
                db.Entry<Core_ChinapayOrder>(order).State = EntityState.Added;  //2��������Ķ���


                var money = aliOrder.A_Money - Convert.ToDecimal(order.B_billAmt);
                if (money > 0)  //3֧������Ǯ������ ����Ӧ�ɷ��ã������Ǯ�������
                {
                    //��õ�ǰ�û������
                    var memberBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                    if (memberBalance != null)
                    {
                        memberBalance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(memberBalance);
                        db.Entry<Core_Balance>(memberBalance).State = EntityState.Modified;


                        //4����һ���˵���¼
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int) BillEnum.����ɷѷ���,
                            B_Money = money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone,
                            B_Remark = "����ɷѣ����ʺ����������˻����",
                            B_Status = 0,
                            B_Time = aliOrder.A_Time,
                            B_Title = aliOrder.A_Remark + "[��������]",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int) MemberRoleEnum.��Ա
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;

                    }
                }

                Core_BillMember bill2 = new Core_BillMember() { B_Flag = 0, B_Module = (int)BillEnum.����ɷ�, B_Money = money, B_OrderId = aliOrder.A_Id, B_Phone = aliOrder.A_Phone, B_Remark = "��������ɷ�", B_Status = 0, B_Time = aliOrder.A_Time, B_Title = aliOrder.A_Remark + "[��������]", B_UId = aliOrder.A_UId, B_Type = (int)MemberRoleEnum.��Ա };
                db.Entry<Core_BillMember>(bill2).State = EntityState.Added;

                db.SaveChanges();//��Ԫ�����������ύ
                return 0;
            }
            return 1;
        }



        /// <summary>
        /// ȷ���µ�
        /// </summary>
        /// <param name="order">����</param>
        /// <returns></returns>
        public ResultMessageViewModel ChinapayOrder(Core_ChinapayOrder order )
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (DbContext db = new LinXinApp20Entities())
            {
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == order.D_UserId);

                if (balance != null)
                {
                    decimal money = Convert.ToDecimal(order.B_billAmt);
                    if (balance.B_Balance > money)
                    {
                        balance.B_Balance -= money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified; //1.����������

                        //2.������ˮ�˵�
                        Core_BillMember bill2 = new Core_BillMember()
                        {
                            B_Flag = (int)BillFlagModuleEnum.������ƽ̨,
                            B_Module = (int) BillEnum.����ɷ�,
                            B_Money = -money,
                            B_OrderId = order.D_Id,
                            B_Phone = order.D_UserPhone,
                            B_Remark = order.D_DeptName,
                            B_Status = 0,
                            B_Time = order.D_Time,
                            B_Title = string.Format("{0}:{1}", order.D_ProjectName, order.C_billNo),
                            B_UId =order.D_UserId,
                            B_Type = (int) MemberRoleEnum.��Ա
                        };
                        db.Entry<Core_BillMember>(bill2).State = EntityState.Added;


                        db.Entry<Core_ChinapayOrder>(order).State = EntityState.Added;  //3.��������Ķ���

                        if (db.SaveChanges()>0)
                        {
                            rs.Msg = "ok";
                            rs.State = 0;
                        }
                    }
                    else
                    {

                        rs.Msg = "�û��ʺ����㣬�����г�ֵ��";
                    }
                }
                else
                {
                    rs.Msg = "�û��ʺŴ���";
                }
            }
            return rs;
        }
    }
}
