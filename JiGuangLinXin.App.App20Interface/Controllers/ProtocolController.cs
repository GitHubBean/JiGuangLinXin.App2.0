using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 各种约定数据：公告、协议、第三方游戏中心url
    /// </summary>
    public class ProtocolController : Controller
    {
        /// <summary>
        /// 协议公告 通用页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id = "1")
        { 
            BaseInfoCore core = new BaseInfoCore();
            var info  = core.LoadEntity(o => o.B_Code == id);
            return View(info);
        }

    }
}
