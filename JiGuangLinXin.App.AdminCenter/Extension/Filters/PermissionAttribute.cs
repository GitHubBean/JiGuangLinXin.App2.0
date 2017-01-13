using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using JiGuangLinXin.App.Provide.Auth;

namespace JiGuangLinXin.App.AdminCenter.Extension.Filters
{
    /// <summary>
    /// 后台权限验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class PermissionAttribute : ActionFilterAttribute
    {



        #region 定义角色
        /// <summary>
        /// 角色：老总
        /// </summary>
        public bool IsAdmin { get; set; }
        /// <summary>
        /// 角色：经理
        /// </summary>
        public bool IsManager { get; set; }
        /// <summary>
        /// 角色：业务员
        /// </summary>
        public bool IsOperator { get; set; }
        /// <summary>
        /// 角色：财务
        /// </summary>
        public bool IsFinance { get; set; }
        #endregion

        /// <summary>
        /// 验证权限
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            StringBuilder sb = new StringBuilder(); //组合Action允许访问的role字符串，与访问会员的role字符串做对比

            if (!IsManager && !IsOperator && !IsFinance) //如果没有特别的身份，默认是普通会员
            {
                sb.Append((int)RoleEnum.Nomal);
                sb.Append(",");
            }
            else
            {
                if (IsAdmin)
                {
                    sb.Append((int)RoleEnum.Admin);
                    sb.Append(",");
                }
                if (IsManager)
                {
                    sb.Append((int)RoleEnum.Manager);
                    sb.Append(",");
                }
                if (IsOperator)
                {
                    sb.Append((int)RoleEnum.Operator);
                    sb.Append(",");
                }
                if (IsFinance)
                {
                    sb.Append((int)RoleEnum.Finance);
                    sb.Append(",");
                }
            }

            string userInfo = FormTicketHelper.GetUserName(); //cookie中的用户信息

            //如果不存在身份信息,跳转登录
            if (string.IsNullOrEmpty(userInfo))
            {
                filterContext.RequestContext.HttpContext.ClearError();
                filterContext.HttpContext.Response.Clear();
                filterContext.Result = new RedirectToRouteResult(new
            RouteValueDictionary(new { controller = "Passport", action = "Login" }));
            }
            else
            {
                string userRole = FormTicketHelper.GetUserData().Split(',')[0]; //获取用户角色Id

                if (!"1".Equals(userRole) && !sb.ToString().Split(',').Contains(userRole)) //不具有访问此Action权限(role为)
                {

                    FilterContextInfo fc = new FilterContextInfo(filterContext);

                    filterContext.RequestContext.HttpContext.ClearError(); 
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    //filterContext.Result = new RedirectResult(errUrl); //跳转至错误提示页面 
                    filterContext.Controller.TempData["Controller"] = fc.controllerName;
                    filterContext.Controller.TempData["Action"] = fc.actionName;
                    filterContext.Controller.TempData["DomainName"] = fc.domainName;
                    filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));

                }
            }

        }

        /// <summary>
        /// 过滤器中，获取来访url的信息
        /// </summary>
        public class FilterContextInfo
        {
            public FilterContextInfo(ActionExecutingContext filterContext)
            {
                #region 获取链接中的字符
                // 获取域名
                if (filterContext.HttpContext.Request.Url != null)
                    domainName = filterContext.HttpContext.Request.Url.Authority;

                //获取 controllerName 名称
                controllerName = filterContext.RouteData.Values["controller"].ToString();

                //获取ACTION 名称
                actionName = filterContext.RouteData.Values["action"].ToString();

                #endregion
            }
            /// <summary>
            /// 域名
            /// </summary>
            public string domainName { get; set; }
            /// <summary>
            /// controllerName 名称
            /// </summary>
            public string controllerName { get; set; }
            /// <summary>
            /// action 名称
            /// </summary>
            public string actionName { get; set; }

        }

        /// <summary>
        /// 前台角色枚举
        /// </summary>
        public enum RoleEnum
        {
            /// <summary>
            /// 超管(老总)
            /// </summary>
            Admin = 1,
            /// <summary>
            /// 经理
            /// </summary>
            Manager = 2,
            /// <summary>
            /// 业务员
            /// </summary>
            Operator = 3,
            /// <summary>
            /// 财务
            /// </summary>
            Finance = 4,
            /// <summary>
            /// 普通角色
            /// </summary>
            Nomal = 5
        }
    }
}