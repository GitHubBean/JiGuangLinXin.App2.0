using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Helpers;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessCenter.Extension;


namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    public class VoucherCardController : BaseController
    {
        private VoucherCardCore vcCore = new VoucherCardCore();
        /// <summary>
        /// 商家抵用券
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



            Expression<Func<Core_VoucherCard, Boolean>> exp = t => t.C_BusId == busId;  //筛选条件
            if (state == 2)  //仓库
            {
                exp = exp.And(o => o.C_State == (int)CardSaleStateEnum.已下架);
            }
            else if (state == 0) //发布中
            {
                exp = exp.And(o => o.C_STime < DateTime.Now && DateTime.Now < o.C_ETime);
            }
            else if (state == 1)  //过期
            {
                exp = exp.And(o => DateTime.Now > o.C_ETime);
            }


            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.C_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.C_Time < et);
                }
            }

            var list = vcCore.LoadEntities(exp).OrderByDescending(o => o.C_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                cid = o.C_Id,
                time = o.C_Time,
                money = o.C_Money,
                stime = o.C_STime,
                etime = o.C_ETime,

                state = o.C_State,
                remark = o.C_Remark
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
        /// 发布优惠卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Publish([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Core_VoucherCard card = new Core_VoucherCard()
            {
                C_BusId = obj.busId,
                C_BusNickname = obj.busNickname,
                C_BusPhone =  obj.busPhone,
                C_ETime =  obj.etime,
               C_Id =  Guid.NewGuid(),
               C_Money = obj.money,
               C_Remark = obj.remark,
               C_State = 0,
               C_STime = obj.stime,
               C_Time = DateTime.Now,
               C_Title = obj.title               
            };

            if (vcCore.AddEntity(card)!=null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    cardId = card.C_Id
                };
            }

            return rs;
        }

        /// <summary>
        /// 下架、删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid cardId = obj.cardId;
            Guid busId = obj.busId;
            int state = obj.state; //1下架 2删除


            var info = vcCore.LoadEntity(o => o.C_BusId == busId && o.C_State == 0 && o.C_Id == cardId);
            if (info != null && state > 0)
            {
                info.C_State = state; //标识下架

                if (vcCore.UpdateEntity(info)) //下架
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }


    }
}
