using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessCenter.Extension;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    public class MallGoodsController : BaseController
    {
        private MallGoodsCore goodsCore = new MallGoodsCore();
        private MallTypeCore mtCore = new MallTypeCore();
        /// <summary>
        /// 发布精品汇
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Core_MallGoods goods = new Core_MallGoods();
            goods.G_Img = obj.coverImg;
            goods.G_Name = obj.goodsName;
            goods.G_TypeId = obj.typeId;
            goods.G_Desc = obj.desc;
            goods.G_Price = obj.price;
            goods.G_PriceOld = goods.G_Price;
            goods.G_Remark = string.Format("配送时段：{0} 至 {1}", obj.ExtraSTime, obj.ExtraETime);
            goods.G_ExtraFee = obj.extraFee;

            goods.G_BusId = obj.busId;
            goods.G_BusName = obj.busName;
            goods.G_BusPhone = obj.busPhone;

            goods.G_Id = Guid.NewGuid();
            goods.G_Discount = 0;
            goods.G_Store = 999;
            goods.G_Sales = 0;
            goods.G_RemainCount = 0;
            goods.G_Clicks = 0;
            goods.G_Recom = 0;
            goods.G_Top = 0;
            goods.G_Status = 0;
            goods.G_AuditingState = (int) AuditingEnum.认证中;
            goods.G_Time = DateTime.Now;


            IEnumerable<dynamic> images = obj.images;  //如果是投票项目


            if (goodsCore.AddOneGoods(goods, images))
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    goodsId = goods.G_Id
                };
            }
            return rs;
        }

        /// <summary>
        /// 编辑精品汇
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid goodsId = obj.goodsId;

            Core_MallGoods goods = goodsCore.LoadEntity(o => o.G_Id == goodsId);
            if (goods != null)
            {
                goods.G_Img = obj.coverImg;
                goods.G_Name = obj.goodsName;
                goods.G_TypeId = obj.typeId;
                goods.G_Desc = obj.desc;
                goods.G_Price = obj.price;
                goods.G_PriceOld = goods.G_Price;
                goods.G_Remark = string.Format("配送时段：{0} 至 {1}", obj.ExtraSTime, obj.ExtraETime);
                goods.G_ExtraFee = obj.extraFee;

                goods.G_BusId = obj.busId;
                goods.G_BusName = obj.busName;
                goods.G_BusPhone = obj.busPhone;



                IEnumerable<dynamic> images = obj.images;  //如果是投票项目


                if (goodsCore.EditOneGoods(goods, images))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "精品汇不存在";
            }
            return rs;
        }

        /// <summary>
        /// 商家发布的精品汇
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel List([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int state = obj.state;
            int audState = obj.audState;
            string title = obj.title;
            int typeId = obj.typeId;
            string busId = obj.busId;
            string busName = obj.busName;

            int pn = obj.pn;
            int rows = obj.rows;

            var typeList = mtCore.LoadEntities().ToList();

            Expression<Func<Core_MallGoods, Boolean>> exp = t => t.G_Status!=2;  //筛选条件,2标识已被删除
            if (!string.IsNullOrEmpty(busId))
            {
                Guid gi = Guid.Parse(busId);
                exp = exp.And(o=>o.G_BusId == gi);
            }



            if (!string.IsNullOrEmpty(busName))
            {
                exp = exp.And(o => o.G_BusName.Contains(busName));
            }

            if (state > -1)
            {
                exp = exp.And(o => o.G_Status == state);
            } if (audState >-1)
            {
                exp = exp.And(o => o.G_AuditingState == audState);
            } if (typeId > -1)
            {
                exp = exp.And(o => o.G_TypeId == typeId);
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.G_Name.Contains(title));
            }
            var list = goodsCore.LoadEntities(exp).OrderByDescending(o => o.G_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                gid = o.G_Id,

                time = o.G_Time,
                title = o.G_Name,
                price = o.G_Price,
                expFee = o.G_ExtraFee,

                state = o.G_Status,
                audState = o.G_AuditingState,
                salse = o.G_Sales,
                typeId= o.G_TypeId ,
                busId = o.G_BusId,
                busName = o.G_BusName,
                busPhone = o.G_BusPhone,

                checkTime = o.G_CheckTime,
                checkTips = o.G_CheckTips
            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    list,
                    typeList,

                };
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }


        /// <summary>
        /// 精品汇下架
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid goodsId = obj.goodsId;
            int state = obj.state; //1下架 2删除


            var info = goodsCore.LoadEntity(o => o.G_BusId == busId && o.G_Status == 0 && o.G_Id == goodsId);
            if (info != null && state > 0)
            {
                info.G_Status = state; //标识下架

                if (goodsCore.UpdateEntity(info)) //下架
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

        /// <summary>
        /// 商品的详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid goodsId = obj.goodsId;

            var goods = goodsCore.LoadEntity(p => p.G_Id == goodsId);
            if (goods != null)
            {
                rs.Data = new
                {
                    goods.G_Id,
                    goods.G_Img,
                    goods.G_Name,
                    goods.G_TypeId,
                    goods.G_Desc,
                    goods.G_Price,
                    goods.G_PriceOld,
                    goods.G_Remark,
                    goods.G_ExtraFee,

                    goods.G_BusId,
                    goods.G_BusName,
                    goods.G_BusPhone
                };
            }

            return rs;
        }
    }
}
