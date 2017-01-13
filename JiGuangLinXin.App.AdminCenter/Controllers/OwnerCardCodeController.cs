using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.EnumHelper;
using JiGuangLinXin.App.AdminCenter.Extension.MvcHelper;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 业主卡密管理
    /// </summary>
    public class OwnerCardCodeController : BaseController
    {
        private OwnerCardCodeCore codeCore = new OwnerCardCodeCore();
        private OwnerCardCore cardCore = new OwnerCardCore();
        /// <summary>
        /// 所有卡密
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id ="",int pn = 0,int rows = 10,string key = "")
        {
            Expression<Func<Sys_OwnerCardCode, Boolean>> expr = t => true;
             
            Guid cid;
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.C_Code.Contains(key);
            }
            if (Guid.TryParse(id,out cid))
            {
               expr =  expr.And(o => o.C_PId == cid);
            }

            var list =
                codeCore.LoadEntities(expr)
                    .OrderByDescending(o => o.C_ActiveTime)
                    .ThenBy(o => o.C_Time)
                    .ToPagedList(pn,rows);
            return View(list);
        }


        /// <summary>
        /// 批量生成卡密
        /// </summary>
        /// <returns></returns>
        public ActionResult Build()
        {
            TempData["CardFlag"] = EnumHelper.GetEnumKeysSelectListItems<OwnerCardFlagEnum>();
            return View(new Sys_OwnerCard());
        }

        /// <summary>
        /// 提交生成卡密
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Build(Sys_OwnerCard obj)
        {
            obj.OC_RemainCount = obj.OC_TotalCount;
            obj.OC_Time = DateTime.Now;
            obj.OC_AdminName = GetUser().A_Account;
            obj.OC_AdminId= GetUser().A_Id;
            obj.OC_Id = Guid.NewGuid();

            if (codeCore.BatchBuildCardCode(obj))
            {
                return RedirectToAction("Index");
            } 
            return View(new Sys_OwnerCard());
        }
       /// <summary>
       /// 制卡记录
       /// </summary>
       /// <returns></returns>
        public ActionResult Card()
       {
           var list = cardCore.LoadEntities().OrderByDescending(o => o.OC_Time).ToList();
           return View(list);
        }
    }
}
