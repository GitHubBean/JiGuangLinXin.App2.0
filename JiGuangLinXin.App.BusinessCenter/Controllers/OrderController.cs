using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessCenter.Extension;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    /// <summary>
    /// 订单管理
    /// </summary>
    public class OrderController : BaseController
    {
        private MallOrderCore oCore = new MallOrderCore();

        /// <summary>
        /// 商家订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel List([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;
            int state = obj.state;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_MallOrder, Boolean>> exp = t => t.GO_BusId == busId;  //筛选条件
            if (state > 0)
            {
                exp = exp.And(o => o.GO_OrderState == state);
            }

            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.GO_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.GO_Time < et);
                }
            }

            var list = oCore.LoadEntities(exp).OrderByDescending(o => o.GO_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                oid = o.GO_Id,
                orderNo = o.GO_OrderNo,
                time = o.GO_Time,

                address = o.GO_TargetAddress,
                buyer = o.GO_TargetName + "-" + o.GO_TargetPhone,
                goodsCount = o.GO_GoodsCount,
                price = o.GO_OrderMoney,
                remark = o.GO_Remark

            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list;
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }


        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Operation([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid orderId = obj.orderId;


            var info = oCore.LoadEntity(o => o.GO_Id == orderId && o.GO_BusId == busId && o.GO_OrderState == (int)OrderStateEnum.待发货);
            if (info != null)
            {

                info.GO_OrderState = (int)OrderStateEnum.已完成; //标识 发货（完成）

                if (oCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "订单不存在";
            }
            return rs;
        }
    }
}
