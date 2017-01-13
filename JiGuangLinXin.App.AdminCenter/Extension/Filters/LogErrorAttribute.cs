using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using JiGuangLinXin.App.Log;

namespace JiGuangLinXin.App.AdminCenter.Extension.Filters
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class LogErrorExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Result is JsonResult)
            {
                //当结果为json时，设置异常已处理
                filterContext.ExceptionHandled = true;
            }

            try
            {
                if (!filterContext.ExceptionHandled)
                {
                    //获取异常信息
                    Exception exception = filterContext.Exception;
                    #region 错误日志保存
                    //LogHelper.WriteLog(string.Format("--错误详情】:{0}", exception.Message + exception.StackTrace), exception);

                    string controllerName = (string)filterContext.RouteData.Values["controller"];
                    string actionName = (string)filterContext.RouteData.Values["action"];
                    string msgTemplate = "在执行 controller[{0}] 的 action[{1}] 时产生异常:【{2}】，详情:{3}";

                    string tip = string.Format("--【访问地址】:{0} *注：*{4}    ###    【访问来源】:{1}   ###    【访问IP】:{2}  --------{3} ##",
                        filterContext.RequestContext.HttpContext.Request.Url,
                        filterContext.RequestContext.HttpContext.Request.UrlReferrer,
                        filterContext.RequestContext.HttpContext.Request.UserHostAddress,
                        DateTime.Now.ToString(),
                        string.Format(msgTemplate, controllerName, actionName, filterContext.Exception.Message, filterContext.Exception.StackTrace));  //日志记录

                    new LogHelper(AppDomain.CurrentDomain.BaseDirectory + "\\log\\", LogType.Daily).Write(tip, LogLevel.Error);
                    #endregion


                    //判断当前的请求是否为后台路径
                    //bool flag = false;
                    //if (filterContext.RequestContext.RouteData.DataTokens.Count > 2)
                    //{
                    //    string areas = filterContext.RequestContext.RouteData.DataTokens.ElementAt(1).Value.ToString().ToLower();
                    //    flag = "admin".Equals(areas);
                    //}


                    HttpException httpException = exception as HttpException;
                    string errorAction = "index";
                    if (httpException != null)
                    {
                        switch (httpException.GetHttpCode())
                        {
                            case 404:
                                errorAction = "HttpError404";
                                break;
                            case 500:
                                // Server error.
                                errorAction = "HttpError500";
                                break;
                            default:
                                errorAction = "General";
                                break;
                        }
                    }
                    //如果是后台请求，跳转到后台错误页面；前台则回前台处理
                    //string errUrl = flag
                    //    ? string.Format("/admin/error/{0}/{1}", errorAction, HttpUtility.UrlEncode(exception.Message))
                    //    : string.Format("/error/{0}/{1}", errorAction, HttpUtility.UrlEncode(exception.Message));

                    filterContext.RequestContext.HttpContext.ClearError();
                    filterContext.ExceptionHandled = true;
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    //filterContext.Result = new RedirectResult(errUrl); //跳转至错误提示页面 
                    filterContext.Controller.TempData["ErrorMsg"] = exception.Message;
                    filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Error", action = errorAction }));


                    //否则调用原始设置
                    base.OnException(filterContext);
                }
            }
            catch (Exception ee)
            {
                new LogHelper(AppDomain.CurrentDomain.BaseDirectory + "\\log\\", LogType.Daily).Write(ee, LogLevel.Warning);
                base.OnException(filterContext);
            }

        }
    }
}