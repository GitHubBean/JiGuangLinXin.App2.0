using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class AuditingCashCore : BaseRepository<Core_AuditingCash>
    {

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool ApplyCash(Core_AuditingCash info)
        {

            using (DbContext db = new LinXinApp20Entities())
            {
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == info.M_UId);
                if (balance.B_Balance >= info.M_Money)  //余额足够
                {
                    //【1】添加申请记录
                    db.Entry(info).State = EntityState.Added;

                    //【2】冻结申请提现的金额
                    balance.B_Balance -= info.M_Money;
                    balance.B_ForzenMoney += info.M_Money;

                    db.Set<Core_Balance>().Attach(balance);
                    db.Entry(balance).State = EntityState.Modified;
                    //【3】添加账单记录
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = "已收到提现申请，等待审核",
                        B_Money = -info.M_Money,
                        B_UId = info.M_UId,
                        B_Phone = info.M_Phone,
                        B_Module = (int)BillEnum.提现,
                        B_OrderId = info.M_Id,
                        B_Type = info.M_Role,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0
                    };
                    db.Entry(bill).State = EntityState.Added;


                    return db.SaveChanges() > 0;
                }
            }

            return false;
        }


        /// <summary>
        /// 提现审核
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ResultMessageViewModel ApplyCashAuditing(Core_AuditingCash info)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                if (info.M_Status == (int)AuditingEnum.认证成功)
                {
                    var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == info.M_UId);
                    if (balance.B_ForzenMoney > info.M_Money)  //冻结的金额足够
                    {
                        //【2】解冻申请提现的金额
                        balance.B_ForzenMoney -= info.M_Money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;

                        //【3】添加账单记录
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "提现申请，已通过审核，成功转款",
                            B_Money = -info.M_Money,
                            B_UId = info.M_UId,
                            B_Phone = info.M_Phone,
                            B_Module = (int)BillEnum.提现,
                            B_OrderId = info.M_Id,
                            B_Type = info.M_Role,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0,
                            B_Remark = info.M_CheckBack
                        };
                        db.Entry(bill).State = EntityState.Added;

                        //【4】添加平台流水记录
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "提现申请，已通过审核，成功转款",
                            B_Money = -info.M_Money,
                            B_UId = info.M_UId,
                            B_Phone = info.M_Phone,
                            B_Module = (int)BillEnum.提现,
                            B_OrderId = info.M_Id,
                            B_Type = info.M_Role,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0,

                            B_Remark = info.M_CheckBack
                        };
                        db.Entry(billMaster).State = EntityState.Added;

                    }
                }

                //【1】  审核记录状态变更
                db.Set<Core_AuditingCash>().Attach(info);
                db.Entry(info).State = EntityState.Modified;

                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            return rs;

        }
    }
}
