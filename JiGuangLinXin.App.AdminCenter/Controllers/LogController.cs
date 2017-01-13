using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 日志管理
    /// </summary>
    public class LogController : Controller
    {
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <returns></returns>
        public ActionResult Error()
        {
            return View();
        }
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <returns></returns>
        public ActionResult Operation()
        {
            return View();
        }



    }
}
