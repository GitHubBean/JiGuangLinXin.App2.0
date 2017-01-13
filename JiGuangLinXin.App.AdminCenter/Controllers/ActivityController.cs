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
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 活动管理
    /// </summary>
    public class ActivityController : BaseController
    {
        private ActivityCore acCore = new ActivityCore();
        private ActivityGoodsCore gCore = new ActivityGoodsCore();
        /// <summary>
        /// 所有活动
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int id = 1, int size = 10, string key = "")
        {

            Expression<Func<Core_Activity, Boolean>> expr = t => true;
            if (!string.IsNullOrWhiteSpace(key))
            {
                expr = t => t.A_Title.Contains(key.Trim());
            }
            var list = acCore.LoadEntities(expr).OrderByDescending(o => o.A_Top).ThenBy(o => o.A_Sort).ToPagedList(id, size);
            return View(list);
        }
        #region  编辑活动
        /// <summary>
        /// 新增活动
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(Guid? id)
        {
            Core_Activity build = null;
            IEnumerable<SelectListItem> ActiveTypeList = null;
            if (id.HasValue)
            {
                build = acCore.LoadEntity(o => o.A_Id == id);
                ActiveTypeList = EnumHelper.GetEnumKeysSelectListItems<ActiveTypeEnum>(false, build.A_Type.ToString());
            }
            else
            {
                ActiveTypeList = EnumHelper.GetEnumKeysSelectListItems<ActiveTypeEnum>();
                build = new Core_Activity() { A_ETime = DateTime.Now, A_STime = DateTime.Now };
            }

            TempData["ActiveType"] = ActiveTypeList;
            return View(build);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection obj)
        {
            //hidGoodsChange
            Core_Activity activity = new Core_Activity();
            TryUpdateModel<Core_Activity>(activity, obj);
            activity.A_Time = DateTime.Now;
            activity.A_BusinessId = Guid.Empty;
            if (activity.A_Id == Guid.Empty)  //新增
            {
                var aId = Guid.NewGuid();
                activity.A_Id = aId;

                if (activity.A_Type == (int)ActiveTypeEnum.商品活动)  //包含有商品
                {
                    Core_ActivityGoods goods = new Core_ActivityGoods();
                    TryUpdateModel<Core_ActivityGoods>(goods, obj);
                    if (!string.IsNullOrEmpty(goods.AG_Name)) //商品信息不为空
                    {
                        goods.AG_ActivityId = activity.A_Id;
                        goods.AG_Time = activity.A_Time;
                       // activity.Core_ActivityGoods.Add(goods);
                    }
                }
                acCore.AddEntity(activity);
            }
            else  //修改
            {
                if (obj["hidGoodsChange"] == "1" && activity.A_Type == (int)ActiveTypeEnum.商品活动)
                {
                    Core_ActivityGoods goods = new Core_ActivityGoods();
                    TryUpdateModel<Core_ActivityGoods>(goods, obj);
                    if (!string.IsNullOrEmpty(goods.AG_Name)) //商品信息不为空
                    {
                        gCore.UpdateByExtended(j => new Core_ActivityGoods() { AG_Status = 1 },
                           o => o.AG_ActivityId == activity.A_Id);  //批量修改之前活动商品的状态为 冻结

                        goods.AG_ActivityId = activity.A_Id;
                        goods.AG_Time = activity.A_Time;
                        //activity.Core_ActivityGoods.Add(goods);
                        gCore.AddEntity(goods);
                    }
                }
                acCore.UpdateEntity(activity);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 删除活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public string Forzen(Guid id, int status)
        {
            string rs = "ok";
            var obj = acCore.LoadEntity(o => o.A_Id == id);
            obj.A_Status = status;
            if (!acCore.UpdateEntity(obj))  //更新活动状态
            {
                rs = "error";
            }
            return rs;
        }

        #endregion

        /// <summary>
        /// 活动商品
        /// </summary>
        /// <returns></returns>
        public ActionResult Goods()
        {
            return View();
        }/// <summary>
        /// 活动订单
        /// </summary>
        /// <returns></returns>
        public ActionResult Order()
        {
            return View();
        }
        /// <summary>
        /// 活动报名
        /// </summary>
        /// <returns></returns>
        public ActionResult Join()
        {
            return View();
        }



    }
}
