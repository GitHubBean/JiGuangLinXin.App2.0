using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 充值失败，退款；兑换积分失败，退积分（一般都是特么账户余额不足）
    /// </summary>
    public class RefundController : BaseController
    {
        private UserCore uCore = new UserCore();
        private CheckHistoryCore chCore = new CheckHistoryCore();
        /// <summary>
        /// 退款记录
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Edit(FormCollection obj)
        {
            int billNo = Convert.ToInt32(obj["uBillId"]);
            Guid uid = Guid.Parse(obj["uid"]);
            int uFlag = Convert.ToInt32(obj["uFlag"]);
            decimal billMoney = Convert.ToDecimal(obj["uNum"]);

            string rs = new RefundCore().Back(billNo, uid, uFlag, billMoney, obj["uRemark"], GetUser().A_Id, GetUser().A_Name);
            if ("ok".Equals(rs))
            {
                return RedirectToAction("Index");
            }
            ViewBag.tips = rs;
            return View(obj);
        }

        [HttpPost]
        public JsonResult QueryUser(string con)
        {
            var userInfo = uCore.LoadEntities(o => o.U_AuditingState == 1 && o.U_Status == 0 && (o.U_LoginPhone.Equals(con) || o.U_NickName.Equals(con)));

            if (userInfo.Any())
            {
                if (userInfo.Count() == 1)
                {
                    var user = userInfo.First();
                    return Json(new { user.U_LoginPhone, user.U_Id, user.U_NickName });
                }
            }
            return Json("err");
        }

    }
}
