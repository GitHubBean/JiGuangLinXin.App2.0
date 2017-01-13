using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ResultMessageViewModel Get()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "not authorization",null);
            return null;
        }
    }
}
