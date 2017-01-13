using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;


namespace JiGuangLinXin.App.App20Interface.Extension
{
    /// <summary>
    /// 是否通过实名认证的验证
    /// 
    /// 某一些操作是必须经过实名认证才能操作
    /// </summary>
    public class AuditingAttributeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
        }
    }
}
