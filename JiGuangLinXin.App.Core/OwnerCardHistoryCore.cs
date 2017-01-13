using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class OwnerCardHistoryCore : BaseRepository<Core_OwnerCardHistory>
    {
        /// <summary>
        /// 购买业主卡
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="nickName">昵称</param>
        /// <param name="phone">电话</param>
        /// <param name="money">金额</param>
        /// <param name="way">购买途径</param>
        /// <param name="flag">业主卡标识</param>
        /// <returns></returns>
        public ResultMessageViewModel BuyOwnerCard(Guid uid, string nickName, string phone, decimal money, int way, int flag, string enPayPwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            string ways = Enum.GetName(typeof(OwnerCardWays), way);
            using (DbContext db = new LinXinApp20Entities())
            {
                //余额

                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                {
                    rs.State = 2;
                    rs.Msg = "您还未设置支付密码,请立即前去设置！";
                    return rs;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPayPwd);  //支付密码
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// 加密支付密码
                    if (!balance.B_PayPwd.Equals(payPwd))
                    {
                        rs.Msg = "支付密码错误！";
                        return rs;
                    }
                }


                //获得当前系统设置有限的折扣
                var rate = db.Set<Sys_DiscountRate>().FirstOrDefault(p => p.R_State == 0);
                var cardRate = rate.R_PayRate;
                double disMoney = Convert.ToDouble(money) * cardRate;  //购买卡所要消耗的余额
                if (Convert.ToDouble(balance.B_Balance) < disMoney)
                {
                    rs.Msg = "用户账户余额不足！";
                }
                else
                {
                    var cardCode =
                        db.Set<Sys_OwnerCardCode>()
                            .Where(o => o.C_State == 0 && o.C_Flag == flag&& o.C_Money == money)
                            .OrderBy(o => o.C_Time);   //暂未激活的业主卡密

                    if (cardCode.Any()) //还有在售的卡
                    {
                        var card = cardCode.FirstOrDefault();
                        card.C_State = 1;
                        card.C_ActiveTime = DateTime.Now;
                        db.Set<Sys_OwnerCardCode>().Attach(card);
                        db.Entry(card).State = EntityState.Modified;  //1.已被激活

                        var cardStore = db.Set<Sys_OwnerCard>().FirstOrDefault(o => o.OC_Id == card.C_PId);
                        cardStore.OC_RemainCount -= 1;
                        db.Set<Sys_OwnerCard>().Attach(cardStore);
                        db.Entry(cardStore).State = EntityState.Modified;  //变更 业主卡 库存

                        balance.B_Balance -= Convert.ToDecimal(disMoney);
                        balance.B_CouponMoney += money;
                        db.Set<Sys_OwnerCardCode>().Attach(card);
                        db.Entry(card).State = EntityState.Modified;  //2.变更可用余额、累计业主卡余额

                        Guid hId = Guid.NewGuid();
                        Core_OwnerCardHistory history = new Core_OwnerCardHistory()
                        {
                            O_Code = card.C_Code,
                            O_CodeId = card.C_Id,
                            O_Discount = cardRate,
                            O_Flag = flag,
                            O_Way = way,
                            O_Id = hId,
                            O_Money = money,
                            O_PayOrder = Guid.NewGuid().ToString("N"),
                            O_Time = DateTime.Now,
                            O_UserId = uid,
                            O_UserNickname = nickName,
                            O_UserPhone = phone
                        };
                        db.Entry(history).State = EntityState.Added;//3.添加领卡记录


                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.平台业主卡,
                            B_Money = money,
                            B_OrderId = hId,
                            B_Phone = phone,
                            B_Remark = string.Format("{0}渠道，平台折扣:{1} 获得", ways, cardRate),
                            B_Status = 0,
                            B_Time = history.O_Time,
                            B_Title = string.Format("业主卡{0}元", money),
                            B_UId = uid,
                            B_Type = (int)MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4.添加业主卡账单



                        Core_BillMember bill2 = new Core_BillMember()
                        {
                            B_Flag = 0,
                            B_Module = (int)BillEnum.商品购买,
                            B_Money = -Convert.ToDecimal(disMoney),
                            B_OrderId = hId,
                            B_Phone = phone,
                            B_Remark = string.Format("{0}渠道，平台折扣:{1} 获得业主卡抵用金额{2}", ways, cardRate, money),
                            B_Status = 0,
                            B_Time = history.O_Time,
                            B_Title = "购买业主卡",
                            B_UId = uid,
                            B_Type = (int)MemberRoleEnum.会员
                        };
                        db.Entry<Core_BillMember>(bill2).State = EntityState.Added;  //5.添加用户余额消费账单

                        if (db.SaveChanges() > 0)  //提交
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                            rs.Data = new
                            {
                                title = bill.B_Title,
                                orderNo = history.O_PayOrder,
                                payMoney = disMoney,
                                postFee = 10,
                                historyId = history.O_Id
                            };
                        }
                    }
                    else
                    {
                        rs.Msg = "该业主卡已售罄！";
                    }
                }
            }

            return rs;
        }


        /// <summary>
        /// 卡密兑换业主卡
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="nickName">昵称</param>
        /// <param name="phone">手机</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public ResultMessageViewModel KeyOwnerCard(Guid uid, string nickName, string phone, string key)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                var card =
                    db.Set<Sys_OwnerCardCode>()
                        .FirstOrDefault(
                            o => o.C_State == 0 && o.C_Code.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                if (card != null)  //卡存在
                {
                    card.C_State = 1;
                    card.C_ActiveTime = DateTime.Now;
                    db.Set<Sys_OwnerCardCode>().Attach(card);
                    db.Entry(card).State = EntityState.Modified;  //1.已被激活

                    var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                    balance.B_CouponMoney += card.C_Money;
                    db.Set<Sys_OwnerCardCode>().Attach(card);
                    db.Entry(card).State = EntityState.Modified;  //2.累计业主卡余额


                    Guid hId = Guid.NewGuid();
                    Core_OwnerCardHistory history = new Core_OwnerCardHistory()
                    {
                        O_Code = card.C_Code,
                        O_CodeId = card.C_Id,
                        O_Discount = -1,
                        O_Flag = (int)OwnerCardWays.卡密兑换,
                        O_Id = hId,
                        O_Money = card.C_Money,
                        O_PayOrder = Guid.NewGuid().ToString("N"),
                        O_Time = DateTime.Now,
                        O_UserId = uid,
                        O_UserNickname = nickName,
                        O_UserPhone = phone
                    };
                    db.Entry(history).State = EntityState.Added;//3.添加领卡记录


                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = 0,
                        B_Module = (int)BillEnum.平台业主卡,
                        B_Money = card.C_Money,
                        B_OrderId = hId,
                        B_Phone = phone,
                        B_Remark = string.Format("{0}渠道获得业主卡", OwnerCardWays.卡密兑换),
                        B_Status = 0,
                        B_Time = history.O_Time,
                        B_Title = string.Format("业主卡{0}元", card.C_Money),
                        B_UId = uid,
                        B_Type = (int)MemberRoleEnum.会员
                    };
                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //4.添加业主卡账单

                    if (db.SaveChanges() > 0)  //提交
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = history;
                    }
                }
                else
                {
                    rs.Msg = "卡密不正确";
                }
            }
            return rs;
        }
        /// <summary>
        /// 申请实体卡邮寄
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ResultMessageViewModel PostOwnerCard(Core_VoucherCardPost obj, string enPaypwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {

                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == obj.P_UId);


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


                if (balance.B_Balance < obj.P_PostMoney)  //不足以支付邮费
                {
                    rs.Msg = "用户帐号余额不足";
                }
                else
                {
                    balance.B_Balance -= obj.P_PostMoney;
                    db.Set<Core_Balance>().Attach(balance);
                    db.Entry(balance).State = EntityState.Modified;  //1.扣用户的卡余额


                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = 0,
                        B_Module = (int)BillEnum.商品购买,
                        B_Money = -obj.P_PostMoney,
                        B_OrderId = obj.P_Id,
                        B_Phone = obj.P_LinkPhone,
                        B_Remark = "申请邮寄用户实体卡，面额：" + obj.P_CardMoney,
                        B_Status = 0,
                        B_Time = obj.P_Time,
                        B_Title = "用户" + obj.P_CardMoney + "卡，实体卡邮寄",
                        B_UId = obj.P_UId,
                        B_Type = (int)MemberRoleEnum.会员
                    };
                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;  //2.添加消费账单

                    db.Entry<Core_VoucherCardPost>(obj).State = EntityState.Added;  //3.添加申请邮寄记录


                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }
            }
            return rs;
        }

    }
}
