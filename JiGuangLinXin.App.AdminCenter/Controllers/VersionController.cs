using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// app应用版本升级控制
    /// </summary>
    public class VersionController : BaseController
    {
        private AppVersionCore vCore = new AppVersionCore();

        public ActionResult Index(int id = 0, int rows = 10)
        {
            var list = vCore.LoadEntities().OrderByDescending(o => o.V_Id).ToPagedList(id, rows);
            return View(list);
        }


        /// <summary>
        /// 新增版本
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            return View(new Sys_AppVersion());
        }

        /// <summary>
        /// 新增版本
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(Sys_AppVersion obj)
        {
            obj.V_Time = DateTime.Now;

            Sys_AppVersion vs;
            if (obj.V_Flag == 0) //andorid 插件
            {
                vs = vCore.LoadEntities(o => o.V_Flag == obj.V_Flag && o.V_FileName == obj.V_FileName).OrderByDescending(o => o.V_Time).FirstOrDefault();
            }
            else
            {
                vs = vCore.LoadEntities(o => o.V_Flag == obj.V_Flag).OrderByDescending(o => o.V_Time).FirstOrDefault();
            }
            if (vs != null)  //存在老版本
            {
                if (obj.V_Flag == 0)  //插件
                {
                    if (Convert.ToInt32(vs.V_Code) >= Convert.ToInt32(obj.V_Code))
                    {
                        ViewBag.tips = "版本号不正确(不能小于之前上传的版本)";
                        return View(obj);
                    }
                }
                else  //应用包
                {

                    Version v1 = new Version(obj.V_Code);
                    Version v2 = new Version(vs.V_Code);
                    if (v1 <= v2)
                    {
                        ViewBag.tips = "版本号不正确(不能小于之前上传的版本)";
                        return View(obj);
                    }
                }
            }

            ViewBag.tips = "添加新版本应用失败,刷新后再试！";
            if (vCore.AddEntity(obj) != null)
            {
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        /// <summary>
        /// 删除版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string Remove(int id)
        {
            var info = vCore.LoadEntity(o => o.V_Id == id);
            if (vCore.DeleteEntity(info))
            {
                return "ok";

            }
            return "更新数据失败";
        }



        /// <summary>
        /// 冻结版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string Forzen(int id, string tips)
        {
            var info = vCore.LoadEntity(o => o.V_Id == id);
            info.V_ForzenTips = tips;
            info.V_State = 1;//冻结
            if (vCore.UpdateEntity(info))
            {
                return "ok";
            }

            return "更新数据失败";
        }

    }
}
