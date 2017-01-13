using System;
using System.Collections.Generic;
using System.Configuration;
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
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class ServicesController : BaseController
    {
        private LifestyleTypeCore serviceTypeCore = new LifestyleTypeCore();
        private BusinessCore busCore = new BusinessCore();
        private BusinessServiceCore serviceCore = new BusinessServiceCore();
        private VoucherCardCore vcCore = new VoucherCardCore();
        private AttachmentCore attCore = new AttachmentCore();
        private LifestyleServicesLikeCore likeCore = new LifestyleServicesLikeCore();
        private MallGoodsCore gCore = new MallGoodsCore();
        private VillageCore villCore = new VillageCore();

        private VoucherCardHistoryCore hisCore = new VoucherCardHistoryCore();
        /// <summary>
        /// 社区服务，分类
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Category()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            rs.Data =
                serviceTypeCore.LoadEntities(o => o.T_State == 0)
                    .OrderByDescending(o => o.T_Recom)
                    .ThenBy(o => o.T_Rank)
                    .ToList().Select(o => new
                    {
                        cid = o.T_Id,
                        title = o.T_Title
                    });


            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 服务（商家）列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Business([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            int type = obj.typeId;
            Guid buildingId = obj.buildingId;

            //var list =
            //    busCore.LoadEntities(
            //        o => o.B_Category == type && o.B_Status == 0 && o.B_AuditingState == (int) AuditingEnum.认证成功)
            //        .OrderByDescending(o => o.B_Level).Select(o=>new
            //        {
            //            busId=o.B_Id,
            //            coverImg =StaticHttpUrl + o.B_ServiceImg,
            //            nickname = o.B_NickName
            //        });


            //var list = busCore.GetBusinessServiceByBuildingId(buildingId, type);

            var list = serviceCore.LoadEntities(o => o.categoryId == type && o.buildingId == buildingId);
            if (list != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    baseUrl = StaticHttpUrl,
                    list
                };

            }
            else
            {
                rs.Msg = "暂无社区服务";
            }

            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 商家优惠卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage VoucherCard([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid bid = obj.busId;
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            rs.Data = vcCore.LoadEntities(o => o.C_BusId == bid && o.C_State == 0 && o.C_ETime > DateTime.Now).OrderByDescending(o => o.C_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                cId = o.C_Id,
                title = o.C_Title,
                money = o.C_Money,
                sTime = o.C_STime,
                eTime = o.C_ETime,
                busName = o.C_BusNickname,
                busId = o.C_BusId,
                flag = "0"
            });

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 社区服务中心
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ServiceCenter([FromBody] JObject value)
        {
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid bid = obj.serviceId;

            var bus = busCore.LoadEntity(o => o.B_Id == bid && o.B_Status == 0);
            //附件
            var atts = attCore.LoadEntities(o => o.A_PId == bid).Select(p => new
            {
                imgUrl = StaticHttpUrl + p.A_Folder + "/" + p.A_FileName
            });

            //是否收藏
            var isLike = likeCore.LoadEntity(o => o.L_UserId == uid && o.L_ServiceId == bid && o.L_State == 0) == null ? "0" : "1";
            string servicePhone = bus.B_Phone.Split(',')[0];
            if (string.IsNullOrEmpty(servicePhone))
            {
                servicePhone = bus.B_Phone.Split(',')[1];
            }
            rs.Data = new
            {
                bid = bid,
                logo = StaticHttpUrl + bus.B_Logo,
                nickName = bus.B_NickName,
                address = bus.B_Address,
                isHot = bus.B_IsHot,
                isReturn = bus.B_IsReturns,
                isAuthentic = bus.B_IsAuthentic,
                isFree = bus.B_IsFree,
                isFamous = bus.B_IsFamous,

                description = bus.B_Desc,
                imgList = atts,

                serviceContent = bus.B_ServiceItem,
                servicePhone,
                isLike,

                shareTitle = string.Format("{0}", bus.B_NickName),
                shareDesc = string.Format("{0}", bus.B_Desc),
                shareUrl = string.Format("{0}html/ServiceCenter/index.html?serviceId={1}&tip=1", ConfigurationManager.AppSettings["OutsideUrl"], bid),
            };

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 商家精品汇
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Goods([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid bid = obj.busId;
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            var goods = gCore.LoadEntities(o => o.G_BusId == bid && o.G_Status == 0 && o.G_AuditingState == (int)AuditingEnum.认证成功).OrderByDescending(o => o.G_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                gId = o.G_Id,
                logo = StaticHttpUrl + o.G_Img,
                title = o.G_Name,
                tags = o.G_Tags,
                price = o.G_Price,
                sales = o.G_Sales,
                clicks = o.G_Clicks,
                expFee = o.G_ExtraFee
            });

            if (goods.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = goods;
            }
            else
            {
                rs.Msg = "暂无数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 收藏社区服务（商家）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Collection([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid bid = obj.busId;
            string nickName = obj.nickname;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            //是否已经收藏过服务
            var isExist = likeCore.LoadEntities(p => p.L_UserId == uid && p.L_ServiceId == bid).ToList();
            if (isExist.Any())  //已经收藏过
            {
                if (likeCore.DeleteEntitys(isExist))  //删除收藏
                {
                    rs.Msg = "ok";
                    rs.State = 0;
                    rs.Data = new { flag = 0 };
                }
            }
            else  //添加收藏
            {

                var bus = busCore.LoadEntity(o => o.B_Id == bid);

                Core_LifestyleServicesLike like = new Core_LifestyleServicesLike()
                {
                    L_ServiceId = bid,
                    L_ServiceImg = bus.B_ServiceImg,
                    L_ServiceName = bus.B_NickName,
                    L_State = 0,
                    L_Time = DateTime.Now,
                    L_UserId = uid,
                    L_UserName = nickName
                };
                var r = likeCore.AddEntity(like);

                if (r != null)
                {
                    rs.Msg = "ok";
                    rs.State = 0;
                    rs.Data = new { flag = 1 };
                }
                else
                {
                    rs.Msg = "社区服务添加到收藏失败";
                }
            }



            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage TakeVoucherCard([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string nickName = obj.nickname;
            Guid cardId = obj.cardId;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();

            var card = vcCore.LoadEntity(o => o.C_Id == cardId && o.C_State == 0);  //获取优惠卡

            if (card != null)
            {
                if (card.C_ETime < DateTime.Now)
                {
                    rs.Msg = "已过期，不能领取";
                }
                else
                {
                    var h = hisCore.LoadEntity(o => o.H_CardId == card.C_Id && o.H_UserId == uid);
                    if (h != null)
                    {
                        rs.Msg = "您已经领取过，请勿重复领取";
                    }
                    else
                    {


                        Core_VoucherCardHistory his = new Core_VoucherCardHistory()
                        {
                            H_BusId = (Guid)card.C_BusId,
                            H_CardId = card.C_Id,
                            H_ETime = card.C_ETime,
                            H_Id = Guid.NewGuid(),
                            H_Money = card.C_Money,
                            H_Nickname = card.C_BusNickname,
                            H_State = 0,
                            H_STime = card.C_STime,
                            H_Time = DateTime.Now,
                            H_Title = card.C_Title,
                            H_UserId = uid,
                            H_UserNickname = nickName,
                            H_UserPhone = phone
                        };

                        hisCore.AddEntity(his);

                        rs.State = 0;
                        rs.Msg = "ok";

                    }
                }

            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 获得小区服务信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ServicesInfoByBuildingId([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid bid = obj.buildingId;

            //var bus = busCore.LoadEntity(o => o.B_Id == bid && o.B_AuditingState == (int)AuditingEnum.认证成功 && o.B_Status == 0);
            var vill = villCore.LoadEntity(o => o.V_Id == bid);
            var services = serviceCore.LoadEntities(o => o.buildingId == bid).Count();

            rs.Data = new
            {
                buildingId = bid,
                buildingName = vill.V_BuildingName,
                buildingLogo = StaticHttpUrl + vill.V_Img,
                serviceCount = services
            };

            return WebApiJsonResult.ToJson(rs);
        }


    }
}
