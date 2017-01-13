using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers.Prize
{
    /// <summary>
    /// 老虎机送电影票活动
    /// </summary>
    public class FilmController : BaseController
    {
        private ScoreExchangeCore exCore = new ScoreExchangeCore();

        /// <summary>
        /// 中奖记录
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id = "", int pn = 0, int rows = 10, string key = "")
        {

            Expression<Func<Core_ScoreExchange, Boolean>> expr = t => t.E_Module == (int)EventH5ModuleEnum.老虎机送电影票;
            key = key.Trim();
            Guid bid;
            if (!string.IsNullOrEmpty(key))
            {
                if (Guid.TryParse(key, out bid))
                {
                    expr = t => t.E_BuildingId == bid;
                }
                else
                {
                    expr = t => t.E_BuildingName.Contains(key) || t.E_Phone.Contains(key);
                }

            }


            var list =
                exCore.LoadEntities(expr)
                    .OrderByDescending(o => o.E_Time)
                    .ToPagedList(pn, rows);
            return View(list);
        }

        /// <summary>
        /// 楼盘用户中奖记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Building(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_ScoreExchange, Boolean>> expr = t => t.E_Module == (int)EventH5ModuleEnum.老虎机送电影票;
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.E_BuildingName.Contains(key);
            }


            PagedList<dynamic> list =
                exCore.LoadEntities(expr).GroupBy(o => new { o.E_BuildingId, o.E_BuildingName }).ToList().Select(o => new
                {
                    bid = o.Key.E_BuildingId,
                    bname = o.Key.E_BuildingName,
                    filmCount = o.Count(c => c.E_Flag == (int)FilmFlagEnum.电影票),
                    cardCount = o.Count(c => c.E_Flag == (int)FilmFlagEnum.业主卡),
                    luckyCount = o.Count(c => c.E_Flag == (int)FilmFlagEnum.红包),

                    filmTotal= o.Where(c => c.E_Flag == (int)FilmFlagEnum.电影票).Sum(c=>c.E_Score),
                    cardTotal = o.Where(c => c.E_Flag == (int)FilmFlagEnum.业主卡).Sum(c => c.E_Score),
                    luckyTotal = o.Where(c => c.E_Flag == (int)FilmFlagEnum.红包).Sum(c => c.E_Score),
                    
                    count = o.Count()
                }).OrderByDescending(o => o.count).ToPagedList<dynamic>(pn, rows);

            string jsonStr = JsonSerialize.Instance.ObjectToJson(list);
            //dynamic rs = new ExpandoObject();
            //rs.vmList = list;
            ViewBag.jsonStr = jsonStr;
            ViewBag.vmObj = list;
            return View();
        }


        /// <summary>
        /// 签到送流量记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult CellphoneTraffic(string id = "", int pn = 0, int rows = 10, string key = "")
        {
            Expression<Func<Core_ScoreExchange, Boolean>> expr = t => t.E_Module == (int)EventH5ModuleEnum.签到送流量;
            key = key.Trim();
            Guid bid;
            if (!string.IsNullOrEmpty(key))
            {
                if (Guid.TryParse(key, out bid))
                {
                    expr = t => t.E_BuildingId == bid;
                }
                else
                {
                    expr = t => t.E_BuildingName.Contains(key) || t.E_Phone.Contains(key);
                }

            }
            var list =
                exCore.LoadEntities(expr)
                    .OrderByDescending(o => o.E_Time)
                    .ToPagedList(pn, rows);
            return View(list);
        }

    }
}
