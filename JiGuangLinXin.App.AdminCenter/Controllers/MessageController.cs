using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 消息管理：推送给客户端的各种消息
    /// </summary>
    public class MessageController : Controller
    {
        /// <summary>
        /// 推送记录
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <returns></returns>
        public ActionResult Push()
        {
            return View();
        }

    }
}
