using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 统计分析
    /// </summary>
    public class StatisticalController : Controller
    {
        /// <summary>
        /// 会员统计
        /// </summary>
        /// <returns></returns>
        public ActionResult User()
        {
            return View();
        }
        /// <summary>
        /// 便民消费统计
        /// </summary>
        /// <returns></returns>
        public ActionResult Consume()
        {
            return View();
        }

        /// <summary>
        /// 红包统计
        /// </summary>
        /// <returns></returns>
        public ActionResult LuckGift()
        {
            return View();
        }

        /// <summary>
        /// 商品订单统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GoodsOrder()
        {
            return View();
        }


    }
}
