using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JiGuangLinXin.App.App20Interface
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //新家推荐H5分享
            routes.MapRoute(
                name: "BuildingShare",
                url: "html/building/index.html",
                defaults: new { controller = "BuildingShare", action = "Index", id = UrlParameter.Optional }
            );

            //协议
            routes.MapRoute(
                name: "Protocol",
                url: "Protocol/{id}",
                defaults: new { controller = "Protocol", action = "Index", id = UrlParameter.Optional }
            );

            //楼盘活动
            routes.MapRoute(
                name: "BuildingActivity",
                url: "sp/{userId}_{buildingId}",
                defaults: new { controller = "OutsideUrl", action = "BuildingActivity" }
            );

            //商家活动
            routes.MapRoute(
                name: "Event",
                url: "event/{userId}_{proId}",
                defaults: new { controller = "OutsideUrl", action = "BuildingActivity" }
            );




            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}