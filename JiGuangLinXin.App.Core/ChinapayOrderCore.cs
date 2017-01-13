using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    /// <summary>
    /// 银联订单
    /// </summary>
    public class ChinapayOrderCore : BaseRepository<Core_ChinapayOrder>
    {
        /// <summary>
        /// 银联销帐成功：1变更缴费的支付宝订单的记录状态为销帐 2添加银联销帐记录 3如果支付宝支付的金额大于销帐的金额，多余的钱存入余额
        /// </summary>
        /// <param name="alipay">支付宝的订单号</param>
        /// <param name="order">银联订单对象</param>
        /// <returns></returns>
        public int OrderWriteoff(string alipay, Core_ChinapayOrder order)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var aliOrder = db.Set<Core_AlipayOrder>().FirstOrDefault(o => o.A_OrderNo == alipay);
                aliOrder.A_Status = 0;// 1标识销帐
                db.Set<Core_AlipayOrder>().Attach(aliOrder);
                db.Entry<Core_AlipayOrder>(aliOrder).State = EntityState.Modified;


                order.D_Id = Guid.NewGuid();
                order.D_PayBillId = aliOrder.A_Id.ToString(); //设置销帐的支付宝帐号id
                db.Entry<Core_ChinapayOrder>(order).State = EntityState.Added;  //2添加银联的订单


                var money = aliOrder.A_Money - Convert.ToDecimal(order.B_billAmt);
                if (money > 0)  //3支付宝的钱，大于 便民应缴费用，多余的钱存入余额
                {
                    //获得当前用户的余额
                    var memberBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                    if (memberBalance != null)
                    {
                        memberBalance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(memberBalance);
                        db.Entry<Core_Balance>(memberBalance).State = EntityState.Modified;


                        //4新增一条账单记录
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int) BillEnum.便民缴费返还,
                            B_Money = money,
                            B_OrderId = aliOrder.A_Id,
                            B_Phone = aliOrder.A_Phone,
                            B_Remark = "便民缴费，销帐后多余金额返回至账户余额",
                            B_Status = 0,
                            B_Time = aliOrder.A_Time,
                            B_Title = aliOrder.A_Remark + "[超出返还]",
                            B_UId = aliOrder.A_UId,
                            B_Type = (int) MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;

                    }
                }

                Core_BillMember bill2 = new Core_BillMember() { B_Flag = 0, B_Module = (int)BillEnum.便民缴费, B_Money = money, B_OrderId = aliOrder.A_Id, B_Phone = aliOrder.A_Phone, B_Remark = "银联便民缴费", B_Status = 0, B_Time = aliOrder.A_Time, B_Title = aliOrder.A_Remark + "[超出返还]", B_UId = aliOrder.A_UId, B_Type = (int)MemberRoleEnum.会员 };
                db.Entry<Core_BillMember>(bill2).State = EntityState.Added;

                db.SaveChanges();//单元操作，批量提交
                return 0;
            }
            return 1;
        }



        /// <summary>
        /// 确认下单
        /// </summary>
        /// <param name="order">订单</param>
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
                        db.Entry(balance).State = EntityState.Modified; //1.变更减少余额

                        //2.新增流水账单
                        Core_BillMember bill2 = new Core_BillMember()
                        {
                            B_Flag = (int)BillFlagModuleEnum.第三方平台,
                            B_Module = (int) BillEnum.便民缴费,
                            B_Money = -money,
                            B_OrderId = order.D_Id,
                            B_Phone = order.D_UserPhone,
                            B_Remark = order.D_DeptName,
                            B_Status = 0,
                            B_Time = order.D_Time,
                            B_Title = string.Format("{0}:{1}", order.D_ProjectName, order.C_billNo),
                            B_UId =order.D_UserId,
                            B_Type = (int) MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill2).State = EntityState.Added;


                        db.Entry<Core_ChinapayOrder>(order).State = EntityState.Added;  //3.添加银联的订单

                        if (db.SaveChanges()>0)
                        {
                            rs.Msg = "ok";
                            rs.State = 0;
                        }
                    }
                    else
                    {

                        rs.Msg = "用户帐号余额不足，请先行充值！";
                    }
                }
                else
                {
                    rs.Msg = "用户帐号存在";
                }
            }
            return rs;
        }
    }
}
