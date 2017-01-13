using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.BusinessCenterInterface
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}