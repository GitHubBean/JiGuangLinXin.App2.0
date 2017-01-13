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
        /// 便民缴费
        /// </summary>
        /// <param name="order">聚合的订单</param>
        /// <returns></returns>
        public ResultMessageViewModel Payment(Core_JuheOrder order, string enPaypwd, string moneyList = "")
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //查询余额
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == order.O_UId);

                if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                {
                    rs.State = 2;
                    rs.Msg = "您还未设置支付密码,请立即前去设置！";
                    return rs;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// 加密支付密码
                    if (!balance.B_PayPwd.Equals(payPwd))
                    {
                        rs.Msg = "支付密码错误！";
                        return rs;
                    }
                }

                if (balance.B_Balance >= order.O_Money)  //余额足够缴费
                {
                    //1减少用户的帐号余额
                    balance.B_Balance -= order.O_Money;
                    db.Set<Core_Balance>().Attach(balance);
                    db.Entry(balance).State = EntityState.Modified;

                    //2.添加帐号的流水账单
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagModuleEnum.官方平台,
                        B_Money = -order.O_Money,
                        B_OrderId = order.O_Id,
                        B_Phone = order.O_Phone,
                        B_Remark = order.O_Remark,
                        B_Status = 0,
                        B_Time = order.O_Time,
                        B_Module = (int)BillEnum.便民缴费,
                        B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type) + ":" + order.O_OrderNo,
                        B_UId = order.O_UId,
                        B_Type = (int)MemberRoleEnum.会员
                    };

                    var moneyArr = moneyList.Split('|');
                    bool flag = false;
                    foreach (var item in moneyArr)  //通过折扣金额得到实际到账金额
                    {
                        var tem = item.Split(',');
                        if (Convert.ToDecimal(tem[0]) == order.O_Money) //打折金额合法
                        {
                            order.O_Money = Convert.ToDecimal(tem[1]);  //实际的订单金额还是原价
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        rs.Msg = "数据访问异常，请检查更新APP，谢谢！";
                        return rs;
                    }


                    #region 85折充值 活动
                    /**
                       var bl = db.Core_BillMember.FirstOrDefault(o => o.B_UId == order.O_UId && o.B_Module == (int)BillEnum.话费充值85折 && o.B_Status == 0);

                    if (bl != null)   //已经享受过85折充值优惠
                    {
                        bill.B_Module = (int)BillEnum.便民缴费;
                    }
                    else
                    {

                        bill.B_Module = (int)BillEnum.话费充值85折;
                        bill.B_Remark += "【话费充值85折优惠】";

                        //order.O_Money = Convert.ToDecimal((order.O_Money / Convert.ToDecimal(0.85)).ToString("F0"));  //订单85折
                        order.O_Remark += "【话费充值85折优惠】";

                        var moneyArr = moneyList.Split('|');
                        bool flag = false;
                        foreach (var item in moneyArr)  //通过折扣金额得到实际到账金额
                        {
                            var tem = item.Split(',');
                            if (Convert.ToDecimal(tem[0]) == order.O_Money) //打折金额合法
                            {
                                order.O_Money = Convert.ToDecimal(tem[1]);  //订单85折缴费,但是实际的订单金额还是原价
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            rs.Msg = "数据访问异常，请检查更新APP，谢谢！";
                            return rs;
                        }
                    }
                     * **/
                    #endregion


                    int juInt = 0;
                    if (int.TryParse(order.O_Money.ToString(), out juInt)) //充值必须是int整数
                    {
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //添加业主账单
                        //3.添加聚合账单
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
                        rs.Msg = "数据访问异常，请检查更新APP，谢谢！";
                        return rs;
                    }
                }
                else
                {
                    rs.Msg = "用户的账户余额不足！";
                }
            }
            return rs;
        }


        /// <summary>
        /// 销帐失败，退款
        /// </summary>
        /// <param name="order">销帐的订单</param>
        /// <returns></returns>
        public bool WriteOffFail(Core_JuheOrder order,string source = "【android】")
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var od = db.Core_JuheOrder.FirstOrDefault(o => o.O_Id == order.O_Id);
                if (od != null)
                {
                    od.O_Status = (int)PayOffEnum.销帐失败;  //更新订单状态

                    db.Set<Core_JuheOrder>().Attach(od);
                    db.Entry(od).State = EntityState.Modified;



                    //如果销帐失败，通知管理员销帐
                    Core_Feedback fb = new Core_Feedback()
                    {
                        F_Content =
                            string.Format("官方(聚合)接口便民，支付成功，但第三方销帐失败。#流水号订单号：{0},日期：{1},支付金额:{2}，应缴金额{3}#,{4}",
                                order.O_Id, order.O_Time, order.O_Money, order.O_Money,source),
                        F_Flag = (int)FeedbackEnum.系统,
                        F_Phone = order.O_Phone,
                        F_Status = 0,
                        F_Time = DateTime.Now,
                        F_Title = "聚合便民缴费，支付成功但销帐失败",
                        F_UId = order.O_UId
                    };


                    db.Entry(fb).State = EntityState.Added;
                    return db.SaveChanges() > 0;
                }
                /*  作废逻辑  ；如果销帐失败，通知管理员销帐
                var ban = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId== order.O_UId);
                if (ban != null)
                {
                    ban.B_Balance += order.O_Money;
                    db.Set<Core_Balance>().Attach(ban);
                    //1更新余额
                    db.Entry(ban).State = EntityState.Modified;


                    //2.添加帐号的流水账单
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Module = (int)BillEnum.便民缴费返还,
                        B_Money = order.O_Money,
                        B_OrderId = order.O_Id,
                        B_Phone = order.O_Phone,
                        B_Remark = order.O_Remark,
                        B_Status = 0,
                        B_Time = order.O_Time,
                        B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type),
                        B_UId = order.O_UId,
                        B_Type = (int)MemberRoleEnum.会员
                    };

                     
                    return db.SaveChanges() > 0;

                }
                */
            }

            return false;
        }

    }
}
