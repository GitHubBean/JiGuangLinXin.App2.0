using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 开发商
    /// </summary>
    public class DeveloperController : Controller
    {
        /// <summary>
        /// 所有开发商
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增开发商
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            return View();
        }


    }
}
