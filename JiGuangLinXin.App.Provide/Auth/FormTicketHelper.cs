using System;
using System.Web;
using System.Web.Security;

namespace JiGuangLinXin.App.Provide.Auth
{
    public class FormTicketHelper
    {
        /// <summary>
        /// 创建一个票据，放在cookie中
        /// 票据中的数据经过加密，解决了cookie的安全问题。
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="userData">用户数据</param>
        /// <param name="rememberMe">是否记住当前用户（如果记住，则保持cookie 1个月）</param>
        /// <param name="prefix">票据前缀</param>
        public static void SetCookie(string username, string userData, bool rememberMe = false, string prefix = "ucenter_admin_")
        {
            var timeout = DateTime.Now.Add(FormsAuthentication.Timeout);  //默认过期时间，30分钟
            if (rememberMe)  //记住密码，记住1个月
            {
                timeout = DateTime.Now.AddDays(30);
            }
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                                       1,
                                                       prefix + username,
                                                       DateTime.Now,
                                                       timeout,
                                                       rememberMe,
                                                       userData,
                                                       FormsAuthentication.FormsCookiePath
                                                   );

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        /// <summary>
        /// 通过此法判断登录
        /// </summary>
        /// <returns>已登录返回true</returns>
        public static bool IsLogin()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }
        /// <summary>
        /// 取得登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            return HttpContext.Current.User.Identity.Name;
        }
        /// <summary>
        /// 取得票据中数据
        /// </summary>
        /// <returns></returns>
        public static string GetUserData()
        {
            var fid = HttpContext.Current.User.Identity as FormsIdentity;
            return fid == null ? "" : fid.Ticket.UserData;
        }
    }
}
