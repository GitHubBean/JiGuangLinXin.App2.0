using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.Core
{
    public class AlipayOrderCore : BaseRepository<Core_AlipayOrder>
    {


        /// <summary>
        /// 银联便民接口支付，添加支付宝订单
        /// </summary>
        /// <param name="obj">添加的订单</param>
        /// <returns>0成功 1订单已经存在 2订单已经销帐</returns>
        public int ChinapayAlipayOrder(Core_AlipayOrder obj)
        {
            var od = LoadEntity(o => o.A_OrderNo == obj.A_OrderNo);
            if (od != null && od.A_Status == 0)
            {
                return 2;
            }
            if (od == null)
            {

                using (DbContext db = new LinXinApp20Entities())
                {
                    //1添加支付宝订单
                    db.Entry<Core_AlipayOrder>(obj).State = EntityState.Added;
                    //2添加个人账单
                    Core_BillMember bill = new Core_BillMember() { B_Flag = (int)BillFlagEnum.平台流水, B_Module = (int)BillEnum.便民缴费, B_Money = obj.A_Money, B_OrderId = obj.A_Id, B_Phone = obj.A_Phone, B_Remark = "", B_Status = 0, B_Time = obj.A_Time, B_Title = obj.A_Remark, B_UId = obj.A_UId, B_Type = (int)MemberRoleEnum.会员 };
                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;


                    db.SaveChanges();//单元操作，批量提交
                    return 0;
                }

            }
            //else if (od.A_Status ==1)  //已经存在，未销帐
            //{

            //} 
            return 1;
        }


        /// <summary>
        /// 阿里缴费
        /// </summary>
        /// <param name="aliOrder">支付宝订单</param> 
        /// <returns></returns>
        public bool Pay(Core_AlipayOrder aliOrder)
        {

           
            using (DbContext db = new LinXinApp20Entities())
            {
                var od = db.Set<Core_AlipayOrder>().FirstOrDefault(o => o.A_OrderNo == aliOrder.A_OrderNo);

                if (od != null)  //账单已经存在
                {
                    return false;
                }


                //用户流水
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Flag = (int)BillFlagEnum.普通流水,
                    B_Module = (int)BillEnum.充值,
                    B_Money = aliOrder.A_Money,
                    B_OrderId = aliOrder.A_Id,
                    B_Phone = aliOrder.A_Phone,
                    B_Remark = "",
                    B_Status = 0,
                    B_Time = aliOrder.A_Time,
                    B_Title = aliOrder.A_Remark,
                    B_UId = aliOrder.A_UId,
                    B_Type = (int)MemberRoleEnum.会员
                };

                //平台流水
                Sys_BillMaster billMaster = new Sys_BillMaster()
                {
                    B_Flag = (int)BillFlagEnum.平台流水,
                    B_Module = (int)BillEnum.充值,
                    B_Money = aliOrder.A_Money,
                    B_OrderId = aliOrder.A_Id,
                    B_Phone = aliOrder.A_Phone,
                    B_Remark = "",
                    B_Status = 0,
                    B_Time = aliOrder.A_Time,
                    B_Title = aliOrder.A_Remark,
                    B_UId = aliOrder.A_UId,
                    B_Type = (int)MemberRoleEnum.会员
                };


                //1添加支付宝订单
                db.Entry<Core_AlipayOrder>(aliOrder).State = EntityState.Added;
                //2添加个人账单
                db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                //3.添加平台账单
                db.Entry<Sys_BillMaster>(billMaster).State = EntityState.Added;

                //4变更余额
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                balance.B_Balance += aliOrder.A_Money;
                db.Set<Core_Balance>().Attach(balance);
                db.Entry<Core_Balance>(balance).State = EntityState.Modified;


                return db.SaveChanges() > 0;//单元操作，批量提交

            }


            return false;
        }
    }
}
