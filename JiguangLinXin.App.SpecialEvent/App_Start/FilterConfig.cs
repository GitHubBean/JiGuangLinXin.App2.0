using System.Web;
using System.Web.Mvc;

namespace JiguangLinXin.App.SpecialEvent
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}