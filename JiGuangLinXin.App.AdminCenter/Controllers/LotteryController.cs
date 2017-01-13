using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 星际大冲关、抽奖记录
    /// </summary>
    public class LotteryController : BaseController
    {

        private PrizeDetailCore vCore = new PrizeDetailCore();

        public ActionResult Index(int id = 0, int rows = 10)
        {
            var list = vCore.LoadEntities().OrderByDescending(o => o.PD_Time).ToPagedList(id, rows);
            return View(list);
        }

        /// <summary>
        /// 查看业主的收货地址
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        public string Address(Guid uid)
        {
            string sb = "";

            DeliveryAddressCore aCore = new DeliveryAddressCore();

            var list = aCore.LoadEntities(o => o.A_UserId == uid).OrderBy(o => o.A_Default).ToList();

            if (list.Any())  //留有收货地址
            {
                list.ForEach(o =>
                {
                    sb += o.A_Default == 1 ? "【默认】" + o.A_Address + "<br/>" : o.A_Address + "<br/>";
                });

            }
            else
            {
                sb = "暂未设置收货地址，请直接电话联系中奖人！";
            }

            return sb;
        }

    }
}
