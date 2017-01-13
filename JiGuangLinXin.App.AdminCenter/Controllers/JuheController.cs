using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 第三方聚合数据
    /// </summary>
    public class JuheController : Controller
    {

        /// <summary>
        /// 聚合缴费机构
        /// </summary>
        /// <returns></returns>
        public ActionResult Unit()
        {
            return View();
        }

        /// <summary>
        /// 聚合缴费订单
        /// </summary>
        /// <returns></returns>
        public ActionResult Order()
        {
            return View();
        }

    }
}
