using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{

    public class ErrorController : BaseController
    {
        public ActionResult Index()
        {
            ViewData["Code"] = "Error";
            ViewData["Msg"] = " - WebSite 网站内部错误";
            return View("Index");
        }
        public ActionResult HttpError404(string id)
        {
            ViewData["Code"] = "HTTP 404";
            ViewData["Msg"] = " - 无法找到文件";
            return View("Index");
        }
        public ActionResult HttpError500(string id)
        {
            ViewData["Code"] = "HTTP 500";
            ViewData["Msg"] = " - 内部服务器错误";
            return View("Index");
        }
        public ActionResult General(string id)
        {
            ViewData["Code"] = "HTTP ";
            ViewData["Msg"] = " 发生错误";
            return View("Index");
        }
        /// <summary>
        /// 访问拒绝
        /// </summary>
        /// <returns></returns>
        public ActionResult AccessDenied()
        {
            return View();
        }
         

    }
}