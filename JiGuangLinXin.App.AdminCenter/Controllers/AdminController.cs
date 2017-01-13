using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.EnumHelper;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 后台管理员
    /// </summary>
    public class AdminController : BaseController
    {

        private AdminCore aCore = new AdminCore();
        //
        // GET: /Admin/
        /// <summary>
        /// 管理人员的个人信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Info()
        {
            return View();
        }

        /// <summary>
        /// 后台管理员管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Sys_Admin, Boolean>> expr = t => true;
            key = key.Trim();
            Guid cid;
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.A_Account.Contains(key) || t.A_Name.Contains(key);
            }

            var list =
                aCore.LoadEntities(expr)
                    .OrderByDescending(o => o.A_Time)
                    .ToPagedList(pn, rows);
            return View(list);
        }


        [HttpPost]
        public string Forzen(Guid id)
        {
            var info = aCore.LoadEntity(o => o.A_Id == id);

            if (info != null)
            {
                var state = info.A_Status;
                if (state == (int)UserStatusEnum.冻结)
                {
                    state = (int)UserStatusEnum.正常;
                }
                else if (state == (int)UserStatusEnum.正常)
                {
                    state = (int)UserStatusEnum.冻结;
                }
                info.A_Status = state;
                if (aCore.UpdateEntity(info)) //更改状态
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
        public ActionResult Edit(Guid? id)
        {
            Sys_Admin obj = null;

            TempData["RoleList"] = EnumHelper.GetEnumKeysSelectListItems<ManagerRoleEnum>();

            if (id.HasValue)
            {
                obj = aCore.LoadEntity(o => o.A_Id == id);
            }
            else
            {
                obj = new Sys_Admin();
            }
            return View(obj);
        }

        /// <summary>
        /// 保存管理员
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Sys_Admin obj)
        {
            obj.A_Time = DateTime.Now;

            string pwdCode = new CreateRandomStr().GetRandomString(4);//登录密码干扰码 
            obj.A_Pwd = Md5Extensions.MD5Encrypt(obj.A_Pwd + pwdCode);
            obj.A_EncryptCode = pwdCode;
            if (obj.A_Id == Guid.Empty) //新增
            {
                obj.A_Id = Guid.NewGuid();



                aCore.AddEntity(obj);
            }
            else //修改
            {
                aCore.UpdateEntity(obj);
            }

            return RedirectToAction("Index");
        }

    }
}
