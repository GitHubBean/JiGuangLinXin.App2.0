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
    /// <summary>
    /// 邻里圈话题管理
    /// </summary>
    public class TopicController : BaseController
    {
        private TopicCore tpCore = new TopicCore();
        /// <summary>
        /// 所有 话题
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int id = 0, int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_Topic, Boolean>> expr = t => t.T_Status == id;
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.T_Title.Contains(key.Trim());
            }

            var list =
                tpCore.LoadEntities(expr)
                    .OrderByDescending(o => o.T_Date)
                    .ToPagedList(pn, rows);
            return View(list);
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string Remove(Guid id)
        {
            var info = tpCore.LoadEntity(o => o.T_Id == id);

            if (info != null)
            {
                info.T_Status = 1;  //标识删除
                if (tpCore.UpdateEntity(info)) //更改状态
                {
                    return "ok";
                }
            }
            return "更新数据失败";
        }



    }
}
