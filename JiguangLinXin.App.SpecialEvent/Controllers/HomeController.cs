using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Provide.Common;
using JiguangLinXin.App.SpecialEvent.Core;

namespace JiguangLinXin.App.SpecialEvent.Controllers
{
    public class HomeController : Controller
    {
        private VillageCore vCore = new VillageCore();
        private VillageVoteCore vvCore = new VillageVoteCore();
        public ActionResult Index()
        {
            Guid id = Guid.Parse("9AB671A7-79CE-4B57-B5AB-081B6D34BFD2");
                 
            //var vill = vCore.LoadEntities(o).FirstOrDefault().V_BuildingName;
            var oo = vCore.LoadEntity(o => o.V_Id == id);
            
            //oo.V_Votes = 10;
            //return Content(vCore.UpdateEntity(oo).ToString());

            //return Content(vvCore.Send(id,"aaa").ToString());

            //return Content(vill);

            //NetHelper
            return View();
        }
    }
}
