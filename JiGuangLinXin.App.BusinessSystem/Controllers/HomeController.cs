using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Antlr.Runtime;
using JiGuangLinXin.App.BusinessSystem.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class HomeController : BaseController
    {

        /// <summary>
        /// 精品汇统计
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel MallGoodsStatistics([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0,"ok",null);
            dynamic obj = value;
            DateTime stime = obj.stime;
            DateTime etime = obj.etime;

            BillMemberCore bmCore = new BillMemberCore();

            var list =
                bmCore.LoadEntities(p => p.B_Module == (int) BillEnum.商品购买 && p.B_Time > stime && p.B_Time < etime)
                    .ToList();

            //选定时间内的账单记录
            var billList = list.Select(o => new
            {
                time = o.B_Time,
                money = Math.Abs(o.B_Money),
            });

            var billCount = list.Count;  //订单数

            var billMoney = list.Sum(o => Math.Abs(o.B_Money)); //总金额


            //昨天
            DateTime ys = DateTime.Parse(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            DateTime ye = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

            var listYes =
                bmCore.LoadEntities(p => p.B_Module == (int)BillEnum.商品购买 && p.B_Time > ys && p.B_Time < ye)
                    .ToList();


            var billYsCount = listYes.Count;

            var billYsMoney = listYes.Sum(o => Math.Abs(o.B_Money));

            rs.Data = new
            {
                billList,
                billCount,
                billMoney,

                billYsCount,
                billYsMoney
            };


            return rs;
        }
    }
}
