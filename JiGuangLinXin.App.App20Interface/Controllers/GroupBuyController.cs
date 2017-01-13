using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class GroupBuyController : BaseController
    {
        private GroupbuyCore buyCore = new GroupbuyCore();
        private GroupbuyOrderCore orderCore = new GroupbuyOrderCore();
        private AttachmentCore attCore = new AttachmentCore();
        private GroupbuyTypeCore buyTypeCore = new GroupbuyTypeCore();
        /// <summary>
        /// 获取用户的邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;
            Guid buildingId = obj.buildingId;


            int orderBy = obj.orderBy;
            int typeId = obj.typeId;
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            var list = buyCore.GetGroupbuyByVilliageId(buildingId, orderBy,typeId,  pn, rows);

            
            if (list.Any())
            {
                rs.Data = new
                {
                    baseImgUrl = StaticHttpUrl,
                    list
                };
                rs.State = 0;
                rs.Msg = "ok";

            }
            else
            {
                rs.Msg = "暂无数据";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 邻里团详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Show([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            Guid buildingId = obj.buildingId;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            var proInfo = buyCore.LoadEntity(o => o.GB_Id == proId && o.GB_State == 0 && o.GB_STime < DateTime.Now && o.GB_ETime > DateTime.Now);
            if (proInfo != null)
            {
                //查看本社区抱团情况,返回抱团人的头像
                var order =
                    orderCore.LoadEntities(o => o.BO_GroupBuyId == proInfo.GB_Id && o.BO_BuildignId == buildingId)
                        .Select(o => new {userLogo = StaticHttpUrl + o.BO_ULogo, uid = o.BO_UId}).ToList();

                int state = 2;  //团购失败
                int isJoin = order.Any(o => o.uid == uid) ? 1 : 0; //自己是否参团了，0否 1是
                if (proInfo.GB_AuditingState == (int)AuditingEnum.认证成功 && proInfo.GB_State == 0)
                {
                    if (proInfo.GB_STime < DateTime.Now &&
                            proInfo.GB_ETime > DateTime.Now && order.Count() < proInfo.GB_PeopleCount)
                    {

                        state = 0;  //团购中
                    }
                    else if (order.Count() == proInfo.GB_PeopleCount)
                    {
                        state = 1;//团购成功
                    } 
                }


                var att = attCore.LoadEntities(o => o.A_PId == proId); //附件图片

                rs.Data = new
                {
                    coverImg = StaticHttpUrl + proInfo.GB_CoverImg,
                    attList = att.Select(o => new
                    {
                        imgUrl = StaticHttpUrl + o.A_Folder + "/" + o.A_FileName  // 存储的是中尺寸图片，输出需要调整成最大尺寸
                    }),
                    peopleCount = proInfo.GB_PeopleCount,
                    price = proInfo.GB_Price.ToString("N"),
                    priceOld = proInfo.GB_PriceOld.ToString("N"),
                    sales = proInfo.GB_Sales,
                    title = proInfo.GB_Titlte,
                    tags = proInfo.GB_Tags,
                    desc = proInfo.GB_Desc,
                    etime = proInfo.GB_ETime,
                    stime = proInfo.GB_STime,
                    busId = proInfo.GB_BusId,
                    busPhone = proInfo.GB_BusPhone,
                    busName = proInfo.GB_BusName,

                    joinCount = order.Count(),
                    flag = order.Count() < proInfo.GB_PeopleCount ? 0 : 1,   //0团未满 1团已满
                    isJoin ,
                    state,//团购状态
                    order
                };
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "暂无数据";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 查询所有邻里团分类
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage QueryAllGroupBuyCategory()
        {

            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            rs.Data = buyTypeCore.LoadEntities(o => o.T_State == 0).OrderBy(o => o.T_Rank).Select(c => new
            {
                cid = c.T_Id,
                title = c.T_Title,
                remark = c.T_Remark
            });

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 用户购买邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Order([FromBody]JObject value)
        {
            var rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            decimal ownerCardMoney = obj.ownerCardMoney;
            int addressId = obj.addressId;
            string enPayPwd = obj.payPwd; //支付密码
            string remark = obj.remark;

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            rs = buyCore.Order(uid,proId,ownerCardMoney,addressId,enPayPwd,remark);



            #region 消息推送

            if (rs.State == 0)
            {
                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "邻里团",
                    title = "恭喜您，成功参团",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您参加的邻里团已成功下单，成团之后将会尽快与您联系，请保持联系方式畅通，我们将竭诚为您服务。",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
            }
            #endregion

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 我的邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage MyGroupbuy([FromBody]JObject value)
        {
            var rs = new ResultMessageViewModel();
            dynamic obj = value;
            int state = obj.state;

            int pn = obj.pn;
            pn = pn - 1;

            int rows = obj.rows;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            var list = orderCore.LoadEntities(o => o.BO_UId == uid);
            if (state == 0)  //组团中
            {
                list = list.Where(o => o.BO_OrderState == (int)GroupBuyOrderStateEnum.待发货);
            }
            else if(state == 1) //已成团
            {
                list = list.Where(o => o.BO_OrderState == (int)GroupBuyOrderStateEnum.已发货 || o.BO_OrderState == (int)GroupBuyOrderStateEnum.已完成);
            }
            else if(state ==2)  //组团失败
            {
                list = list.Where(o => o.BO_OrderState == (int)GroupBuyOrderStateEnum.已退款);
            }

            var rsList  = list.OrderByDescending(o => o.BO_Time).Skip(pn*rows).Take(rows).ToList().Select(o=>new
            {
                orderId  = o.BO_Id,
                proId = o.BO_GroupBuyId,
                proTitle = o.BO_Titlte,
                img = StaticHttpUrl+o.BO_CoverImg,
                peopleCount = o.BO_PeopleCount,
                price = o.BO_Price.ToString("F2"),
                time = o.BO_Time,
                orderNo = o.BO_OrderNo,
                state = Enum.GetName(typeof(GroupBuyOrderStateEnum), o.BO_OrderState),
                flag = Enum.GetName(typeof(GroupBuyOrderFlagEnum), o.BO_OrderState),

            });
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = rsList;

            }
            else
            {
                rs.Msg = "暂无数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }


    }
}
