using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Provide.Common;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    public class ConsoleController : BaseController
    {
        //
        // GET: /Console/
        /// <summary>
        /// 控制台首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //int i = 0;
            //var id = 5/i;

            ViewBag.OS = NetHelper.GetOSVersion();
            ViewBag.Brower = NetHelper.GetBrowser();

            return View();
        }

    }
}
