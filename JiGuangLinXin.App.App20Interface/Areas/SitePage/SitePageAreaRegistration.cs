using System.Web.Mvc;

namespace JiGuangLinXin.App.App20Interface.Areas.SitePage
{
    /// <summary>
    /// APP第三方链接：关于我们、协议地址、楼盘推荐、楼盘活动等
    /// </summary>
    public class SitePageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SitePage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SitePage_default",
                "Site/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "JiGuangLinXin.App.App20Interface.Areas.SitePage.Controllers" }
            );
        }
    }
}
