using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace JiguangLinXin.App.SpecialEvent.Extension
{
    /// <summary>
    /// 是否通过社区认证的验证
    /// 
    /// 某一些操作是必须经过社区认证才能操作
    /// </summary>
    public class AuditingAttributeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
        }
    }
}
