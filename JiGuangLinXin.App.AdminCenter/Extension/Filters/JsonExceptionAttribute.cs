using System;
using System.Web.Mvc;
using JiGuangLinXin.App.Log;

namespace JiGuangLinXin.App.AdminCenter.Extension.Filters
{
    /// <summary>
    /// ajax 返回内容为json时，错误过滤
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class JsonExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            new LogHelper().Write(filterContext.Exception, LogLevel.Error); //记录日志
            if (!filterContext.ExceptionHandled)
            {
                //返回异常JSON
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = filterContext.Exception.Message }
                };
            }
        }
    }
}