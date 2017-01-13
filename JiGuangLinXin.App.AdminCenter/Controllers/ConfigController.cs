using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 通用配置
    /// </summary>
    public class ConfigController : Controller
    {
        /// <summary>
        /// 说明书
        /// </summary>
        /// <returns></returns>
        public ActionResult Instructions()
        {
            return View();
        }

    }
}
