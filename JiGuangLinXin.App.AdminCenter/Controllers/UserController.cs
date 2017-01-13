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
using JiGuangLinXin.App.AdminCenter.Extension.MvcHelper;
namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 会员管理
    /// </summary>
    public class UserController : BaseController
    {
        private UserCore uCore = new UserCore();
      /// <summary>
      /// 所有用户
      /// </summary>
      /// <param name="id">小区ID</param>
      /// <param name="pn">分页</param>
      /// <param name="rows">条数</param>
      /// <param name="key">关键字</param>
      /// <returns></returns>
        public ActionResult Index(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_User, Boolean>> expr = t => true;
            key = key.Trim();
            Guid cid;
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.U_LoginPhone.Contains(key) || t.U_NickName.Contains(key);
            } 

            var list =
                uCore.LoadEntities(expr)
                    .OrderByDescending(o => o.U_LastLoginTime)
                    .ToPagedList(pn, rows);
            return View(list);
        }


        [HttpPost]
        public string Forzen(Guid id)
        {
            var info = uCore.LoadEntity(o => o.U_Id== id);

            if (info != null )
            {
                var state = info.U_Status ;
                if (state == (int)UserStatusEnum.冻结)
                {
                    state = (int) UserStatusEnum.正常;
                }
                else if(state == (int) UserStatusEnum.正常)
                {
                    state = (int) UserStatusEnum.冻结;
                }
                info.U_Status = state;
                if (uCore.UpdateEntity(info)) //更改状态
                {
                    return "ok";
                }
            }
            return "更新数据失败";
        }



        /// <summary>
        /// 所有群主
        /// </summary>
        /// <param name="id">小区ID</param>
        /// <param name="pn">分页</param>
        /// <param name="rows">条数</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public ActionResult Manager(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_User, Boolean>> expr = t => t.U_AuditingManager==(int)AuditingEnum.认证成功;
            key = key.Trim(); 
            if (!string.IsNullOrEmpty(key))
            {
                expr = expr.And(t => t.U_LoginPhone.Contains(key) || t.U_NickName.Contains(key) || t.U_BuildingName.Contains(key)); 
            }

            var list =
                uCore.LoadEntities(expr)
                    .OrderByDescending(o => o.U_LastLoginTime)
                    .ToPagedList(pn, rows);
            return View(list);
        }


        /// <summary>
        /// 解除管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string Fire(Guid id)
        {
            var info = uCore.LoadEntity(o => o.U_Id== id);

            if (info != null )
            { 
                //撤销管理员
                info.U_AuditingManager = (int)AuditingEnum.未认证;
                if (uCore.UpdateEntity(info)) //更改状态
                {
                    return "ok";
                }
            }
            return "更新数据失败";
        }

    }
}
