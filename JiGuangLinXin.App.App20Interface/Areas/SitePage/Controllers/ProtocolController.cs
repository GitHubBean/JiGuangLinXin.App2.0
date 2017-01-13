using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;

namespace JiGuangLinXin.App.App20Interface.Areas.SitePage.Controllers
{
    /// <summary>
    /// 关于我们、协议配置
    /// </summary>
    public class Protocol1Controller : Controller
    {
        //
        // GET: /SitePage/Protocol/

        public ActionResult Go()
        {
            return Content("abc");
        }

        public ActionResult Index(string id = "1")
        {
            BaseInfoCore core = new BaseInfoCore();
            var info = core.LoadEntity(o => o.B_Code == id);
            return View(info);
        }
    }
}
