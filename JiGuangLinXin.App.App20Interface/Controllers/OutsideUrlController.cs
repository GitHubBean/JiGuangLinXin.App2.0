using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 所有第三方的链接,分享H5 页面的URL
    /// </summary>
    public class OutsideUrlController : Controller
    {
        private string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

        /// <summary>
        /// 分享邻里团
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GroupBuy(Guid id)
        {
            return Redirect("/html/llt/index.html?gid=" + id);
        }
        /// <summary>
        /// 分享社区活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Event(string userId, string proId)
        {
            return Content(userId + "********" + proId);
            return View();
        }
        /// <summary>
        /// 分享新家推荐
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Building(Guid id)
        {
            return Redirect("/html/xjtj/index.html?share=true");
            return Content(id.ToString());
            return View();
        }

        /// <summary>
        /// 查看新家推荐的活动详情（有机会得红包）。另外活动里面的报名参见 API Building/Apply
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="buildingId">楼盘ID</param>
        /// <returns></returns>
        public ActionResult BuildingActivity(string userId, string buildingId)
        {
            return Redirect("/html/xjtj/index.html?share=true");
            BuildingCore bCore = new BuildingCore();
            Guid uId = Guid.Parse(userId);  //楼盘ID
            Guid bId = Guid.Parse(buildingId);  //用户ID

            //  return Content(string.Format("用户ID：{0}, 楼盘ID:{1},报名",uId,bId));

            var rs = bCore.Activity(bId, uId);
            //rs.Data.coverImg = StaticHttpUrl + rs.Data.coverImg;
            if (rs.State != 9999)
            {
                if (rs.State == 1)  //本次领到了红包
                {
                    var binfo = bCore.LoadEntity(o => o.B_Id == bId);
                    #region 消息推送

                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = (int)PushMessageEnum.默认,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "楼盘红包",
                        title = "您收到一个楼盘红包",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = " 您收到楼盘【" + binfo.B_Name + "】的红包，" + binfo.B_HongbaoMoney.ToString("N") + "元",
                    };

                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uId.ToString("N").ToLower());

                    #endregion
                }
                return Content(bId.ToString());
                return View();
            }
            return Content("刷新一下吧" + rs.Msg);
        }


        /// <summary>
        /// 协议公告 通用页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Protocol(string id = "1")
        {
            BaseInfoCore core = new BaseInfoCore();
            var info = core.LoadEntity(o => o.B_Code == id);
            return View(info);
        }

    }
}
