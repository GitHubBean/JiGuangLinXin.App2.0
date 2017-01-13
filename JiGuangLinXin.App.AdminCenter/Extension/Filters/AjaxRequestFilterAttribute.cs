using System;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Extension.Filters
{
    /// <summary>
    /// 标记action 只能是ajax访问
    /// </summary>
    public class AjaxRequestFilterAttribute : FilterAttribute, IActionFilter
    {
        #region IActionFilter 成员

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                throw  new Exception("访问非法！~");
            }
        }
        #endregion
    }
}
