using System.Web.Mvc;

namespace JiguangLinXin.App.SpecialEvent.Areas.ControlCenter
{
    public class ControlCenterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ControlCenter";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ControlCenter_default",
                "cp/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
