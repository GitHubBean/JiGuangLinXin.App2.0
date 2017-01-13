using System.Web.Mvc;
using JiGuangLinXin.App.Provide.Auth;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.Rpg;
using JiguangLinXin.App.SpecialEvent.Core;

namespace JiguangLinXin.App.SpecialEvent.Areas.ControlCenter.Controllers
{
    public class PassportController : Controller
    {
        private AdminCore aCore = new AdminCore();
        //
        // GET: /Passport/

        public ActionResult Login()
        {
            //ViewBag.LoginTips = Md5Extensions.MD5Encrypt("123456888");
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection obj)
        {
            string name = obj["uname"];
            string pwd = obj["pwd"];
            string rememberMe = obj["rememberMe"];
            var admin = aCore.LoadEntity(o => o.A_Account.Equals(name));
            if (admin != null && admin.A_Pwd.Equals(Md5Extensions.MD5Encrypt(pwd + admin.A_EncryptCode)))  //通过登录验证
            {
                FormTicketHelper.SetCookie(admin.A_Account, string.Format("{0},{1}", admin.A_Role, admin.A_Id), "on".Equals(rememberMe));
                return RedirectToAction("Index", "Console");
                ViewBag.LoginTips = "登录成功咯哦" + rememberMe + "on".Equals(rememberMe) + FormTicketHelper.IsLogin();
            }
            else
            {
                ViewBag.LoginTips = "帐号密码不匹配";
            }
            return View();
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            FormTicketHelper.Logout();
            CookieHelper.ClearCookie("manager");
            return RedirectToAction("Login");
        }


    }
}
