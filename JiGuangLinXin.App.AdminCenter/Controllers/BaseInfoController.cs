using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.EnumHelper;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 系统基础设置
    /// 
    /// 各种约定数据：公告、协议、第三方游戏中心url
    /// 
    /// 
    /// </summary>
    public class BaseInfoController : BaseController
    {
        private BaseInfoCore infoCore = new BaseInfoCore();

        public ActionResult Index()
        {
            var list = infoCore.LoadEntities().OrderByDescending(o => o.B_Time).ToList();
            return View(list);
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            TempData["Flag"] = EnumHelper.GetEnumKeysSelectListItems<ProtocolEnum>();
            Sys_BaseInfo info = new Sys_BaseInfo();
            if (id.HasValue)
            {
                info = infoCore.LoadEntity(o => o.B_Id == id);
                if (info == null)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(info);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Sys_BaseInfo obj)
        {
            bool rs = false;
            obj.B_Time = DateTime.Now;
            if (obj.B_Id > 0) //修改
            {
                rs = infoCore.UpdateEntity(obj);
            }
            else //新增
            {
                rs = infoCore.AddEntity(obj) == null;
            }
            if (rs)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Msg = "编辑失败,请检查数据";
            return View(obj);
        }
    }
}
