using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    public class BillController : BaseController
    {
        /// <summary>
        /// 会员账单
        /// </summary>
        /// <returns></returns>
        public ActionResult User(Guid? id, int pn=0,int size = 10)
        {
            BillMemberCore bmCore = new BillMemberCore();

            var iqList = bmCore.LoadEntities();
            Guid uid ;
            if (id.HasValue)
            {
                iqList = iqList.Where(o=>o.B_UId == id);
            }
            var list = iqList.OrderByDescending(o => o.B_Time).ToPagedList(pn, size);



            return View(list);
        }

        /// <summary>
        /// 支付宝账单
        /// </summary>
        /// <returns></returns>
        public ActionResult Alipay()
        {
            return View();
        }
        /// <summary>
        /// 平台账单
        /// </summary>
        /// <returns></returns>
        public ActionResult Platform(int pn = 0, int size = 10)
        {
            BillMasterCore bmCore = new BillMasterCore();

            var list = bmCore.LoadEntities().OrderByDescending(o => o.B_Time).ToPagedList(pn, size);
            return View(list);
        }
    }
}
