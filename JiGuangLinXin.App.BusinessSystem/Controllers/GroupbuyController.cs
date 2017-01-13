using System;
using System.Collections.Generic;
using System.Drawing;
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
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class GroupbuyController : BaseController
    {
        private GroupbuyCore buyCore = new GroupbuyCore();
        private GroupbuyBuildingCore gbCore = new GroupbuyBuildingCore();

        private GroupbuyOrderCore orderCore = new GroupbuyOrderCore();
        private AttachmentCore attCore = new AttachmentCore();
        /// <summary>
        /// 商家发布邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Core_GroupBuy buy = new Core_GroupBuy();
            buy.GB_BusId = obj.busId;
            buy.GB_BusName = obj.busName;
            buy.GB_BusPhone = obj.busPhone;
            buy.GB_CoverImg = obj.coverImg;
            buy.GB_PeopleCount = obj.pcount;
            buy.GB_Titlte = obj.title;
            buy.GB_Tags = obj.tags;
            buy.GB_STime = obj.stime;
            buy.GB_ETime = obj.etime;
            buy.GB_Price = obj.price;
            buy.GB_PriceOld = obj.priceOld;
            buy.GB_Desc = obj.desc; 
            buy.GB_TypeId = obj.typeId;
            buy.GB_TypeName = obj.typeName;

            buy.GB_Target = obj.target;
            buy.GB_Remark = obj.remark;

            buy.GB_Id = Guid.NewGuid();
            buy.GB_LimitCount = 1;
            buy.GB_State = 0;
            buy.GB_Time = DateTime.Now;
            buy.GB_Discount = 1;
            buy.GB_ExtraFee = 0;
            buy.GB_Sales = 0;
            buy.GB_Clicks = 0;
            buy.GB_Top = 0;
            buy.GB_AuditingState = (int)AuditingEnum.认证中;

            IEnumerable<dynamic> images = obj.images;  //如其他图片附件

            IEnumerable<dynamic> buildings = obj.buildings;  //邻里团发布的区域
            #region 作废
            //buyCore.AddEntityNoSave(buy);  //添加邻里团
            //if (images.Any())
            //{
            //    foreach (var img in images)
            //    {
            //        Sys_Attachment am = new Sys_Attachment();

            //        am.A_Id = Guid.NewGuid();
            //        am.A_PId = buy.GB_Id;
            //        am.A_Type = (int)AttachmentTypeEnum.图片;
            //        am.A_Time = buy.GB_Time;

            //        am.A_FileNameOld = img.A_FileNameOld;
            //        am.A_FileName = img.A_FileName;
            //        am.A_Size = img.A_Size;
            //        am.A_Folder = img.A_Folder;
            //        am.A_Rank = img.A_Rank;

            //        //添加项目附件
            //        attCore.AddEntityNoSave(am);  //添加附件
            //    }

            //}
            //if (attCore.SaveChanges() > 0)  //批量提交
            //{
            //    rs.State = 0;
            //    rs.Msg = "ok";
            //    rs.Data = new
            //    {
            //        proId = buy.GB_Id
            //    };
            //}
            #endregion
            rs = buyCore.Edit(buy,images,buildings,true);
            return rs;
        }


        /// <summary>
        /// 编辑邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;

            Core_GroupBuy buy = buyCore.LoadEntity(o => o.GB_Id == proId);

            buy.GB_BusId = obj.busId;
            buy.GB_BusName = obj.busName;
            buy.GB_BusPhone = obj.busPhone;
            buy.GB_CoverImg = obj.coverImg;
            buy.GB_PeopleCount = obj.pcount;
            buy.GB_Titlte = obj.title;
            buy.GB_Tags = obj.tags;
            buy.GB_STime = obj.stime;
            buy.GB_ETime = obj.etime;
            buy.GB_Price = obj.price;
            buy.GB_PriceOld = obj.priceOld;
            buy.GB_Desc = obj.desc;
            buy.GB_TypeId = obj.typeId;
            buy.GB_TypeName = obj.typeName;


            buy.GB_Target = obj.target;
            buy.GB_Remark = obj.remark;


            //buy.GB_Id = Guid.NewGuid();
            //buy.GB_LimitCount = 1;
            //buy.GB_State = 0;
            //buy.GB_Time = DateTime.Now;
            //buy.GB_Discount = 1;
            //buy.GB_ExtraFee = 0;
            //buy.GB_Sales = 0;
            //buy.GB_Clicks = 0;
            //buy.GB_Top = 0;
            buy.GB_AuditingState = (int)AuditingEnum.认证中;

            IEnumerable<dynamic> images = obj.images;  //如其他图片附件
            IEnumerable<dynamic> buildings = obj.buildings;  //邻里团发布的区域

            #region 作废
            //buyCore.UpdateEntitiesNoSave(buy);  //更新邻里团，不提交
            //if (images !=null && images.Any())
            //{
            //    attCore.DeleteByExtended(o => o.A_PId == buy.GB_Id);

            //    foreach (var img in images)
            //    {
            //        Sys_Attachment am = new Sys_Attachment();

            //        am.A_Id = Guid.NewGuid();
            //        am.A_PId = buy.GB_Id;
            //        am.A_Type = (int)AttachmentTypeEnum.图片;
            //        am.A_Time = buy.GB_Time;

            //        am.A_FileNameOld = img.A_FileNameOld;
            //        am.A_FileName = img.A_FileName;
            //        am.A_Size = img.A_Size;
            //        am.A_Folder = img.A_Folder;
            //        am.A_Rank = img.A_Rank;

            //        //添加项目附件
            //        attCore.AddEntityNoSave(am);  //添加附件
            //    }

            //}
            //if (attCore.SaveChanges() > 0)  //批量提交
            //{
            //    rs.State = 0;
            //    rs.Msg = "ok";
            //    rs.Data = new
            //    {
            //        proId = buy.GB_Id
            //    };
            //}

            #endregion 
            rs = buyCore.Edit(buy, images, buildings, false);  //提交编辑
            return rs;
        }



        /// <summary>
        /// 发布的所有邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ProList([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            string busIdString = obj.busId;

            string title = obj.title;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            int pn = obj.pn;
            int rows = obj.rows;
            int state = obj.state;

            Expression<Func<Core_GroupBuy, Boolean>> exp = o => true;  //筛选条件


            Guid busId;
            if (!string.IsNullOrEmpty(busIdString) && Guid.TryParse(busIdString, out busId))
            {
                exp = exp.And(o => o.GB_BusId == busId);
            }
            if (state == 0)  //团购中
            {
                exp =
                    exp.And(
                        o =>
                            o.GB_AuditingState == (int)AuditingEnum.认证成功 && o.GB_State == 0);//&& o.GB_STime < DateTime.Now &&o.GB_ETime > DateTime.Now 
            }

            if (state == 1)  //审核中
            {
                exp =
                    exp.And(
                        o =>
                            o.GB_AuditingState == (int)AuditingEnum.认证中 && o.GB_State == 0);
            }

            if (state == 2)  //审核失败
            {
                exp =
                    exp.And(
                        o =>
                            o.GB_AuditingState == (int)AuditingEnum.认证失败 && o.GB_State == 0);
            }

            if (state == 3)  //已结束
            {
                exp =
                    exp.And(
                        o => o.GB_ETime < DateTime.Now);
            }




            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.GB_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.GB_Time < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.GB_Titlte.Contains(title));
            }


            var list = buyCore.LoadEntities(exp).OrderByDescending(o => o.GB_Time).Skip(pn * rows).Take(rows);
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list.Select(o => new
                {
                    o.GB_Id,
                    img = StaticHttpUrl + o.GB_CoverImg,
                    o.GB_Tags,
                    o.GB_Titlte,
                    o.GB_Desc,
                    o.GB_Price,
                    o.GB_PriceOld,
                    o.GB_PeopleCount,
                    o.GB_STime,
                    o.GB_ETime,
                    o.GB_Time,
                    o.GB_State,
                    o.GB_AuditingState,
                    o.GB_CheckTips,
                    o.GB_CheckTime,
                    o.GB_BusId,
                    o.GB_BusName,
                    o.GB_Top,
                    o.GB_BusPhone
                });
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂无数据";
            }
            return rs;
        }

        private GroupbuyStateEnum GetStateStr(int peoCount, int joinCount, DateTime stime, DateTime etime)
        {
            if (joinCount == peoCount)
            {
                return GroupbuyStateEnum.团购成功;
            }
            else if (stime < DateTime.Now && etime > DateTime.Now && joinCount < peoCount)
            {
                return GroupbuyStateEnum.团购中;
            }
            return GroupbuyStateEnum.团购失败;
        }

        /// <summary>
        /// 邻里团，社区开团记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ProBuildingList([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            string title = obj.title;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_GroupBuyVillage, Boolean>> exp = o => o.VB_GroupBuyId == proId;  //筛选条件


            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.VB_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.VB_Time < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.VB_BuildignName.Contains(title));
            }

            var list = gbCore.LoadEntities(exp).OrderByDescending(o => o.VB_Time).Skip(pn * rows).Take(rows).ToList();
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list.Select(o => new
                {
                    o.VB_BuildignName,
                    o.VB_BuildingId,
                    o.VB_GroupBuyId,
                    o.VB_Id,
                    o.VB_PeopleCount,
                    o.VB_JoinCount,
                    o.VB_Time,
                    state = o.VB_State,  //0待发货  1已发货 2已退款
                    stateStr = (int)GetStateStr(o.VB_PeopleCount, o.VB_JoinCount, o.VB_STime, o.VB_ETime)

                });
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂无数据";
            }
            return rs;
        }

        /// <summary>
        /// 邻里团订单记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ProOrderList([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            Guid buildingId = obj.buildingId;

            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            int pn = obj.pn;
            int rows = obj.rows;
            Expression<Func<Core_GroupBuyOrder, Boolean>> exp =
                o => o.BO_GroupBuyId == proId && o.BO_BuildignId == buildingId;  //筛选条件


            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.BO_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.BO_Time < et);
                }
            }
            var list = orderCore.LoadEntities(exp).OrderByDescending(o => o.BO_Time).Skip(pn * rows).Take(rows);
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list.Select(o => new
                {
                    o.BO_Id,
                    o.BO_Time,
                    o.BO_Titlte,
                    o.BO_TargetName,
                    o.BO_TargetPhone,
                    o.BO_TargetAddress,
                    o.BO_OrderNo,
                    o.BO_GoodsCount,
                    o.BO_OrderState
                });
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂无数据";
            }
            return rs;
        }

        /// <summary>
        /// 邻里团下架
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid proId = obj.proId;
            //int state = obj.state;//1下架 2删除

            var info = buyCore.LoadEntity(o => o.GB_BusId == busId && o.GB_State == 0 && o.GB_Id == proId);
            if (info != null)
            {
                info.GB_State = 1; //标识下架

                if (buyCore.UpdateEntity(info)) //下架
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
        /// 邻里团的详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid proId = obj.proId;

            var goods = buyCore.LoadEntity(p => p.GB_Id == proId);

            AttachmentCore attCore = new AttachmentCore();
            var attList = attCore.LoadEntities(o => o.A_PId == proId).OrderBy(o => o.A_Rank).Select(o => new
            {
                imgUrl = StaticHttpUrl + o.A_Folder + "/" + o.A_FileName,
                o.A_FileNameOld,
                o.A_FileName,
                o.A_Size,
                o.A_Folder,
                o.A_Rank

            });

            if (goods != null)
            {
                rs.Data = new
                {
                    goods.GB_Id,
                    goods.GB_CoverImg,
                    goods.GB_Titlte,
                    goods.GB_TypeId,
                    goods.GB_Tags,
                    goods.GB_Price,
                    goods.GB_PriceOld,
                    goods.GB_PeopleCount,
                    goods.GB_Desc,
                    goods.GB_Remark,
                    goods.GB_STime,
                    goods.GB_ETime,
                    goods.GB_BusName,
                    attList,
                };
            }

            return rs;
        }



        /// <summary>
        /// 邻里团推荐
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Top([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid proId = obj.proId;
            var info = buyCore.LoadEntity(o => o.GB_State == 0 && o.GB_Id == proId);
            if (info != null)
            {
                info.GB_Top = info.GB_Top == 0 ? 1 : 0;

                if (buyCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        state = info.GB_State
                    };
                }
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }


        /// <summary>
        /// 社区订单批量发货
        /// 
        /// 每个小区批量发货
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Deliver([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid proId = obj.proId;
            rs = orderCore.Deliver(proId, busId);
            return rs;
        }
        /// <summary>
        /// 社区订单批量退款
        /// 
        /// 每个小区批量退款
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel BackMoney([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid proId = obj.proId;

            rs = orderCore.BackMoney(proId, busId);

            return rs;
        }

    }
}
