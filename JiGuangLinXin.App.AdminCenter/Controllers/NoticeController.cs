using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    public class NoticeController : BaseController
    {
        private NoticeCore nCore = new NoticeCore();

        /// <summary>
        /// 系统公告
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ActionResult Index(string id = "", int pn = 0, int rows = 10)
        {
            var list =
                nCore.LoadEntities()
                    .OrderByDescending(o => o.N_Date)
                    .ToPagedList(pn, rows);
            return View(list);
        }


        /// <summary>
        /// 上下架、公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string Forzen(int id)
        {
            var info = nCore.LoadEntity(o => o.N_Id == id);
            if (info != null)
            {
                var state = info.N_State;
                if (state == (int)UserStatusEnum.冻结)
                {
                    state = (int)UserStatusEnum.正常;
                }
                else if (state == (int)UserStatusEnum.正常)
                {
                    state = (int)UserStatusEnum.冻结;
                }
                info.N_State = state;
                if (nCore.UpdateEntity(info)) //更改状态
                {
                    return "ok";
                }
            }
            return "更新数据失败";
        }


        /// <summary>
        /// 编辑管理员
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            var info = new Sys_Notice();
            if (id.HasValue)
            {
                info = nCore.LoadEntity(o => o.N_Id == id);
            }

            return View(info);
        }

        /// <summary>
        /// 保存管理员
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Sys_Notice obj)
        {
            obj.N_Date = DateTime.Now;
            //obj.N_Flag = 0;
            if (obj.N_Id > 0)
            {
                nCore.UpdateEntity(obj);
            }
            else
            {
                nCore.AddEntity(obj);
            }

            return RedirectToAction("Index");
        }


    }
}
