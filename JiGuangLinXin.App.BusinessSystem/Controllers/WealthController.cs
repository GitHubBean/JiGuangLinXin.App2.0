using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 账户余额管理
    /// </summary>
    public class WealthController : BaseController
    {
        private AuditingCashCore acCore = new AuditingCashCore();
        private BalanceCore blaCore = new BalanceCore();

        /// <summary>
        /// 申请提现列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel CashHistory([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            int state = obj.state;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            int pn = obj.pn;
            int rows = obj.rows;



            Expression<Func<Core_AuditingCash, Boolean>> exp = t => t.M_UId == busId;  //筛选条件
            if (state > 0)
            {
                exp = exp.And(o => o.M_Status == state);
            }

            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.M_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.M_Time < et);
                }
            }

            var list = acCore.LoadEntities(exp).OrderByDescending(o => o.M_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                cid = o.M_Id,
                time = o.M_Time,
                money = o.M_Money,
                account = o.M_BankAccount,
                accountPhone = o.M_Phone,

                state = o.M_Status,
                remark = o.M_Remark

            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list;
            }
            else
            {
                rs.Msg = "没有更多数据";
            }

            return rs;
        }


        /// <summary>
        /// 账户余额
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Balance([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;

            Guid busId = obj.busId;

            var balance = blaCore.LoadEntity(o => o.B_AccountId == busId);
            rs.Data = new
            {
                balance = balance.B_Balance,
                forzen = balance.B_ForzenMoney
            };
            return rs;
        }
        /// <summary>
        /// 余额申请提现
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel CashApply([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
             

            Core_AuditingCash apply = new Core_AuditingCash()
            {
                M_BankAccount = obj.account,
                M_Phone = obj.phone,
                M_Money = obj.money,
                M_UId = obj.busId,
                M_Bank = obj.bank,
                M_BankName = obj.userName,



                M_Id = Guid.NewGuid(),
                M_Time = DateTime.Now,
                M_Status = (int)AuditingEnum.未认证,
                M_Flag = obj.flag,
                M_Role = (int)MemberRoleEnum.商家
            };


            if (acCore.ApplyCash(apply))
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "提现失败，请检查余额是否足够";
            }

             

            return rs;
        }

    }
}
