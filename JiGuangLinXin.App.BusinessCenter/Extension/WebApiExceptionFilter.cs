using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.BusinessCenter.Extension
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
            LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Business, LogType.Daily);
            log.Write(context.Request.RequestUri +"\n"+ context.Exception.Message + "\n" + context.Exception.StackTrace, LogLevel.Error);

            //var message = context.Exception.StackTrace;
            //if (context.Exception.InnerException != null)
            //    message = context.Exception.InnerException.StackTrace;

            //context.Response = new HttpResponseMessage() { Content = new StringContent("{'Status':'1','Msg':'" + message + "','Data':''}") };

            //context.Response = new HttpResponseMessage() { Content = new StringContent("{'state':'9999','msg':'数据请求异常，请确认后再试！','data':''}") };

            var obj = new ResultMessageViewModel() { State = 9999, Msg = context.Exception.StackTrace + "--数据请求异常，请确认后再试！", Data = null };
            string str = JsonSerialize.Instance.ObjectToJson(obj);
            //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            context.Response = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            base.OnException(context);
        }
    }
}