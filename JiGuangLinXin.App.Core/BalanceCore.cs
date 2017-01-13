using System;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class BalanceCore : BaseRepository<Core_Balance>
    {
        /// <summary>
        /// ֧���� ����ֵ
        /// </summary>
        /// <param name="orderId">֧�������̻������ţ��̼��Զ��壩</param>
        /// <param name="money">�������</param>
        /// <param name="aliOrderId">֧�����Ķ����ţ�֧�������ɣ�</param>
        /// <returns></returns>
        public ResultMessageViewModel RechargeAlipay(string orderId, decimal money, string aliOrderId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var aliOrder = db.Core_AlipayOrder.FirstOrDefault(o => o.A_OrderNo == orderId);
                if (aliOrder != null)
                {
                    if (aliOrder.A_Status != (int)PayOffEnum.δ����)
                    {
                        rs.State = 1;
                        rs.Msg = "����������";
                    }
                    else   //֧�����������ڣ���ʼ����
                    {
                        //���Ķ���״̬
                        aliOrder.A_Remark = aliOrderId;
                        aliOrder.A_Status = (int)PayOffEnum.������;

                        //�����û������ 
                        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                        balance.B_Balance += aliOrder.A_Money;

                        //����û����˵�
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.��ֵ,
                            B_Money = aliOrder.A_Money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone, 
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = "�û�ʹ��֧�������߳�ֵ",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int)MemberRoleEnum.��Ա
                        };
                        db.Core_BillMember.Add(bill);

                        //���ƽ̨�˵�
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.��ֵ,
                            B_Money = aliOrder.A_Money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone,
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = "�û�ʹ��֧�������߳�ֵ",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int)MemberRoleEnum.��Ա
                        };
                        db.Sys_BillMaster.Add(billMaster);

                        if (db.SaveChanges() > 0)
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                        }
                    }
                }
                else
                {
                    rs.Msg = "����������";
                }
            }
            return rs;
        }
    }
}
