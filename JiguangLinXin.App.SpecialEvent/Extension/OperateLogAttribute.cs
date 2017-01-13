using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace JiguangLinXin.App.SpecialEvent.Extension
{
    /// <summary>
    /// 记录管理员的操作Log
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OperateLogAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            ////用户id
            //string uid = actionContext.Request.Headers.GetValues("uid").FirstOrDefault();
            ////用户电话
            //string phone = actionContext.Request.Headers.GetValues("phone").FirstOrDefault();
            ////平台标识
            //string platform = actionContext.Request.Headers.GetValues("platform").FirstOrDefault();

            ////请求的url路径
            //string url = actionContext.Request.RequestUri.AbsoluteUri;

            //Sys_OperateLog log = new Sys_OperateLog() { L_Desc = Desc, L_DriverType = Convert.ToInt32(platform), L_Flag = Flag, L_Phone = phone, L_UId = Guid.Parse(uid), L_Url = url, L_Status = 0, L_Time = DateTime.Now };

            //new OperateLogCore().AddEntity(log);


            string uid = "AF854528-D063-47DC-A5E6-DDBC5725389D";//actionContext.HttpContext.Request.Headers.GetValues("uid").FirstOrDefault();
            string phone = "15825942359";// actionContext.HttpContext.Request.Headers.GetValues("phone").FirstOrDefault();
            string platform = "1";//actionContext.HttpContext.Request.Headers.GetValues("platform").FirstOrDefault();


            string url = actionContext.Request.RequestUri.AbsoluteUri;
        }
        /// <summary>
        /// 操作描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 模块标识，具体参见枚举
        /// </summary>
        public int Flag { get; set; }


        ///// <summary>
        ///// 结果集过滤
        ///// </summary>
        ///// <param name="actionContext"></param>
        //public override void OnResultExecuted(ResultExecutedContext actionContext)
        //{

        //    string uid = "AF854528-D063-47DC-A5E6-DDBC5725389D";//actionContext.HttpContext.Request.Headers.GetValues("uid").FirstOrDefault();
        //    string phone = "15825942359";// actionContext.HttpContext.Request.Headers.GetValues("phone").FirstOrDefault();
        //    string platform = "1";//actionContext.HttpContext.Request.Headers.GetValues("platform").FirstOrDefault();


        //    string url = actionContext.HttpContext.Request.Url.ToString();

        //    Sys_OperateLog log = new Sys_OperateLog() { L_Desc = Desc, L_DriverType = Convert.ToInt32(platform), L_Flag = Flag, L_Phone = phone, L_UId = Guid.Parse(uid), L_Url = url, L_Status = 0, L_Time = DateTime.Now };

        //    new OperateLogCore().AddEntity(log);
        //}
    }
}