using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.Filters;

namespace JiGuangLinXin.App.AdminCenter
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new LogErrorExceptionAttribute());
        }
    }
}