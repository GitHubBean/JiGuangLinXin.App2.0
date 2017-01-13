using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.App20Interface.Extension
{
    /// <summary>
    /// 错误过滤
    /// </summary>
    public class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {

            // string errorMsg = string.Format("controller:{0} \n action:{1}",context.ActionContext.ControllerContext.RouteData.Route.);
            //context.RequestContext.RouteData.DataTokens
            LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.System, LogType.Daily);
            log.Write(HttpContext.Current.Request.UserHostAddress + "\n" + context.Request.RequestUri + "\n" + context.Exception.Message + "\n" + context.Exception.StackTrace, LogLevel.Error);

            //var message = context.Exception.StackTrace;
            //if (context.Exception.InnerException != null)
            //    message = context.Exception.InnerException.StackTrace;

            //context.Response = new HttpResponseMessage() { Content = new StringContent("{'Status':'1','Msg':'" + message + "','Data':''}") };

            //context.Response = new HttpResponseMessage() { Content = new StringContent("{'state':'9999','msg':'数据请求异常，请确认后再试！','data':''}") };

            //var obj = new ResultViewModel() { State = 9999, Msg = context.Exception.StackTrace + "--数据请求异常，请确认后再试！", Data = null };
            var obj = new ResultViewModel() { State = 9999, Msg = "网络数据请求异常，请稍后再试！", Data = null };
            string str = JsonSerialize.Instance.ObjectToJson(obj);
            //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            context.Response = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            base.OnException(context);
        }
    }
}