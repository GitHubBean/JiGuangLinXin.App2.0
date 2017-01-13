using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace JiGuangLinXin.App.BusinessSystem.Extension
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
