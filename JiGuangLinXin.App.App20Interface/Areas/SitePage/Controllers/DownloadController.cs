using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.App20Interface.Areas.SitePage.Controllers
{
    public class DownloadController : Controller
    {
        public ActionResult Index()
        {
            return Content("欢迎下载邻信App");
        }

    }
}
