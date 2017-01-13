using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 邀约管理
    /// </summary>
    public class InviteController : Controller
    {
        /// <summary>
        /// 所有邀约
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 邀约报名
        /// </summary>
        /// <returns></returns>
        public ActionResult Join()
        {
            return View();
        }
        /// <summary>
        /// 所有邀约评论
        /// </summary>
        /// <returns></returns>
        public ActionResult Comment()
        {
            return View();
        }
        /// <summary>
        /// 邀约圈子管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Group()
        {
            return View();
        }

    }
}
