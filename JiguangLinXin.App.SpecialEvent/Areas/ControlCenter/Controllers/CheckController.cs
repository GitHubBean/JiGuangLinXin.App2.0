using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiguangLinXin.App.SpecialEvent.Core;
using Webdiyer.WebControls.Mvc;

namespace JiguangLinXin.App.SpecialEvent.Areas.ControlCenter.Controllers
{
    /// <summary>
    /// 申请审核
    /// </summary>
    public class CheckController : BaseController
    {
        private VillageApplyCore applyCore = new VillageApplyCore();
        public ActionResult Building(int id=0)
        {
            var list = applyCore.LoadEntities().OrderByDescending(o => o.A_Time).ToPagedList(id, 10);
            return View(list);
        }

        [HttpPost]
        public string Allow(Guid id, int state)
        {
            var aud = applyCore.LoadEntity(o => o.A_Id == id && o.A_State == 0);
            if (aud != null)  //记录存在
            {
                aud.A_State = state;


                if (applyCore.UpdateEntity(aud))
                {
                    if (state == 1)
                    {
                        StatisticsCore sCore = new StatisticsCore();
                        var info = sCore.LoadEntity();
                        info.S_VillageCount += 1;
                        sCore.UpdateEntity(info);
                    }
                    return "ok";
                }
            }
            return "err";
        }
    }
}
