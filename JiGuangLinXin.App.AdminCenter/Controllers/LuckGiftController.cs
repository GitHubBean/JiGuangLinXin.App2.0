using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 红包管理
    /// </summary>
    public class LuckGiftController : Controller
    {
        /// <summary>
        /// 楼盘红包
        /// </summary>
        /// <returns></returns>
        public ActionResult Developer()
        {
            return View();
        }
        /// <summary>
        /// 聊天红包
        /// </summary>
        /// <returns></returns>
        public ActionResult Chat()
        {
            return View();
        }
        /// <summary>
        /// 红包退款记录
        /// </summary>
        /// <returns></returns>
        public ActionResult BackOrder()
        {
            return View();
        }


    }
}
