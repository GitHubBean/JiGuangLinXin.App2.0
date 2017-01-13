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
        /// 支付宝 余额充值
        /// </summary>
        /// <param name="orderId">支付宝的商户订单号（商家自定义）</param>
        /// <param name="money">订单金额</param>
        /// <param name="aliOrderId">支付宝的订单号（支付宝生成）</param>
        /// <returns></returns>
        public ResultMessageViewModel RechargeAlipay(string orderId, decimal money, string aliOrderId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var aliOrder = db.Core_AlipayOrder.FirstOrDefault(o => o.A_OrderNo == orderId);
                if (aliOrder != null)
                {
                    if (aliOrder.A_Status != (int)PayOffEnum.未销帐)
                    {
                        rs.State = 1;
                        rs.Msg = "订单已销帐";
                    }
                    else   //支付宝订单存在，开始销帐
                    {
                        //更改订单状态
                        aliOrder.A_Remark = aliOrderId;
                        aliOrder.A_Status = (int)PayOffEnum.已销帐;

                        //新增用户的余额 
                        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                        balance.B_Balance += aliOrder.A_Money;

                        //添加用户的账单
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.充值,
                            B_Money = aliOrder.A_Money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone, 
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = "用户使用支付宝在线充值",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int)MemberRoleEnum.会员
                        };
                        db.Core_BillMember.Add(bill);

                        //添加平台账单
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.充值,
                            B_Money = aliOrder.A_Money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone,
                            B_Remark = "",
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = "用户使用支付宝在线充值",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int)MemberRoleEnum.会员
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
                    rs.Msg = "订单不存在";
                }
            }
            return rs;
        }
    }
}
