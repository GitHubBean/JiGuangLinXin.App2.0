using System.Web.Mvc;
using JiGuangLinXin.App.Log;

namespace JiGuangLinXin.App.App20Interface.Extension
{
    public class CustHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext actionContext)
        {
            LogHelper log = new LogHelper();
            log.Write(actionContext.Exception, LogLevel.Error);
         //   base.OnException(filterContext);

            //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}