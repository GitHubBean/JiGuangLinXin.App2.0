using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.Rpg;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    public class BusinessController : BaseController
    {
        private BusinessCore uCore = new BusinessCore();
        /// <summary>
        /// 商家管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_Business, Boolean>> expr = t => true;
            key = key.Trim();
            Guid cid;
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.B_LoginPhone.Contains(key) || t.B_NickName.Contains(key) ||t.B_TrueName.Contains(key);
            } 

            var list =
                uCore.LoadEntities(expr)
                    .OrderByDescending(o => o.B_RegisterDate)
                    .ToPagedList(pn, rows);

            List<Core_LifestyleType> typeList = CacheHelper.GetCache("typeList") as List<Core_LifestyleType>;
            if (typeList ==null)
            {
                typeList = new LifestyleTypeCore().LoadEntities(o => o.T_State == 0).ToList();
                CacheHelper.SetCache("typeList",typeList);
            }

            ViewBag.TypeList = typeList;

            return View(list);
        }


        [HttpPost]
        public string Forzen(Guid id)
        {
            var info = uCore.LoadEntity(o => o.B_Id== id);

            if (info != null )
            {
                var state = info.B_Status ;
                if (state == (int)UserStatusEnum.冻结)
                {
                    state = (int) UserStatusEnum.正常;
                }
                else if(state == (int) UserStatusEnum.正常)
                {
                    state = (int) UserStatusEnum.冻结;
                }
                info.B_Status = state;
                if (uCore.UpdateEntity(info)) //更改状态
                {
                    return "ok";
                }
            }
            return "更新数据失败";
        }

    }
}
