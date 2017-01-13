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
        /// 系统反馈；订单退款
        /// </summary>
        /// <param name="id">反馈ID</param>
        /// <param name="content">备注内容</param>
        /// <param name="source">订单来源</param>
        /// <param name="orderNo">订单号</param>
        /// <returns></returns>
        public bool BackOrderMoney(int id, string content, int source, string orderNo)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var fb = db.Core_Feedback.FirstOrDefault(o => o.F_Id == id);
                if (fb != null)
                {

                    Guid on = Guid.Empty;  // 流水账单关联的项目单号
                    decimal money = 0;
                    if (source == 1) //聚合
                    {
                        on = Guid.Parse(orderNo);

                        var order = db.Core_JuheOrder.FirstOrDefault(o => o.O_Id == on && o.O_Status == (int)PayOffEnum.销帐失败);  //只有销帐失败的才手动退款
                        order.O_Status = (int)PayOffEnum.已退款;  //更新订单状态

                        db.Set<Core_JuheOrder>().Attach(order);
                        db.Entry(order).State = EntityState.Modified;
                        money = order.O_Money;

                        //用户的订单状态，改为无效（删除）
                        //var bill = db.Core_BillMember.FirstOrDefault(o=>o.B_OrderId == on && o.B_Status == 0);

                    }

                    if (source == 0) //银联
                    {
                        var order = db.Core_ChinapayOrder.FirstOrDefault(o => o.C_ordId == orderNo.ToString() && o.D_ordStat == "221" && o.D_scBillStat == "3");  //只有销帐失败的才手动销帐
                        order.D_ordStat = "000";  //更新订单状态
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

                        //1累计用户的帐号余额
                        balance.B_Balance += money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;




                        //2.添加帐号的流水账单
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = (int)BillFlagModuleEnum.官方平台,
                            B_Module = (int)BillEnum.便民退款,
                            B_Money = money,
                            B_OrderId = on,
                            B_Phone = fb.F_Phone,
                            B_Remark = source == 1 ? "【官方便民】退款,备注：" + content : "【银联便民】退款,备注：" + content,
                            B_Status = 0,
                            B_Time = DateTime.Now,
                            B_Title = source == 1 ? "【官方便民】退款" : "【银联便民】退款",
                            B_UId = fb.F_UId,
                            B_Type = (int)MemberRoleEnum.会员
                        };

                        //3.反馈状态
                        fb.F_Status = 1;  //已处理
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
