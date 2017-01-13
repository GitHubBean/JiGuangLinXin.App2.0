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
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// app 首页  
    /// </summary>
    public class MainController : BaseController
    {

        private UserCore uCore = new UserCore();
        private VillageCore vCore = new VillageCore();
        //private EventViewCore eCore = new EventViewCore();
        private EventCore eCore = new EventCore();
        //private LifestyleServicesCore lifeCore = new LifestyleServicesCore();
        private BusinessServiceCore serviceCore = new BusinessServiceCore();
        private NoticeCore nCore = new NoticeCore();
        private IndexRecommendCore indexCore = new IndexRecommendCore();
        private BuildingCore bCore = new BuildingCore();
        /// <summary>
        /// APP 首页
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Index([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            Guid buildingId = obj.buildingId;
            string areaCode = obj.areaCode;
            string imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

            var village = vCore.LoadEntity(o => o.V_Id == buildingId);
            string vImg = imgUrl + (village.V_Img ?? AttachmentFolderEnum.community + "/default.jpg");  //如果该小区没有图片，采用默认图片
            //int vNumber = village.V_Number;  //社区用户数量

            //我的邻居
            var neighbor =
                uCore.LoadEntities(o => o.U_Status != (int)UserStatusEnum.冻结 && o.U_BuildingId == buildingId);

            int vNumber = neighbor.Count();  //邻居总数


            // int eventCount = eCore.LoadEntities(o => o.E_VillageId == buildingId && o.E_Status == 0).Count();//社区活动总数
            //int lifeCount = lifeCore.LoadEntities(o => o.G_VillageId == buildingId && o.G_Status == 0).Count();  //社区服务总数
            int lifeCount = serviceCore.LoadEntities(o => o.buildingId == buildingId).Count();

            dynamic notice = null;
            var notList = nCore.LoadEntities(
                o =>
                    o.N_State == 0 && o.N_Flag == (int)PageEnum.首页)
                .OrderByDescending(o => o.N_Date)
                .Take(8)
                .Select(o => new
                {
                    title = o.N_SubTitle,
                    tags = o.N_Tags,
                    endTime = o.N_Remark,
                    id = o.N_Id
                });  //公告

            if (!notList.Any())
            {
                notice = new List<dynamic>()
                {
                    new
                    {
                        title = "欢迎使用邻信APP",
                        tags = "公告",
                        endTime = "",
                        id = "",
                    }
                };

            }
            else
            {
                notice = notList;
            }


            //var services =
            //    serviceCore.LoadEntities(o => o.buildingId == buildingId)
            //        .Take(10)
            //        .Select(o =>
            //            new
            //            {
            //                title = o.nickname,
            //                img = imgUrl + o.coverImg,
            //                id = o.busId
            //            });  

            var services = indexCore.GetProject(buildingId, (int)IndexRecommedEnum.社区服务, 3).Select(o => new
            {
                title = o.R_BusName,
                img = imgUrl + o.R_ImgUrl,
                id = o.R_BusId,
                typeId = o.R_Type
            });//社区服务

            //var goods = new MallGoodsCore().QueryGoods(buildingId).Select(o => new
            //{
            //    title = o.G_Name,
            //    img = StaticHttpUrl + o.G_Img,
            //    id = o.G_Id
            //});  
            var goods = indexCore.GetProject(buildingId, (int)IndexRecommedEnum.便民购, 8).Select(o => new
            {
                title = o.R_ProName,
                img = imgUrl + o.R_ImgUrl,
                id = o.R_ProId,
                typeId = o.R_Type

            }); //社区精品购物

            /*
                goodsCore.LoadEntities(
                    o => o.G_Status == 0 && o.G_VillageId == buildingId && o.G_Top == 1 && o.G_RemainCount > 0)
                    .OrderByDescending(o => o.G_Sales)
                    .Take(10)
                    .Select(o => new
                    {
                        title = o.G_Name,
                        img = imgUrl + o.G_ImgTop,
                        id = o.G_Id

                    }); 便民购**/

            //var eventsList =
            //    eCore.LoadEntities(
            //        o =>
            //            o.E_Top == 1 && o.E_Status == 0 &&
            //            (o.E_BuildingId.Contains(buildingId.ToString()) || o.E_Target == (int)MemberRoleEnum.平台));

            //var events = eventsList.OrderBy(o => o.E_Rank)
            //    .Take(10)
            //    .Select(o => new
            //    {
            //        //tags = o.E_BusRole==0?"社区":"商家",
            //        title = o.E_Title,
            //        img = imgUrl + o.E_ImgTop,
            //        id = o.E_Id
            //    }); 

            var events = indexCore.GetProject(buildingId, (int)IndexRecommedEnum.活动中心, 4).ToList().Select(o => new
            {
                title = o.R_ProName,
                img = imgUrl + o.R_ImgUrl,
                id = o.R_ProId,
                typeId = "社区活动".Equals(o.R_Tags) ? 1 : 2 ,       // 活动中心分为 ： 1社区活动 ，2 邻里团
                proJson = string.IsNullOrEmpty(o.R_Remark)?null:JsonSerialize.Instance.JsonToObject(o.R_Remark),  //社区活动，记录活动的ID/跳转的链接等
                
            }); //社区活动

            int eventsCount = eCore.GetBuildingEvents(buildingId).Count() + eCore.LoadEntities(o => o.E_Status == 0 && o.E_Target == 0).Count();


            //var houses =
            //    bCore.LoadEntities(o => o.B_Top == 1 && o.B_Status == 0)
            //        .OrderByDescending(o => o.B_Time)
            //        .Take(6)
            //        .Select(o => new
            //        {
            //            img = imgUrl + o.B_ImgTop,
            //            id = o.B_Id,
            //            title = o.B_Name
            //        });  


            var houses = indexCore.GetProject(buildingId, (int)IndexRecommedEnum.楼盘推荐, 3).Select(o => new
            {
                title = o.R_ProName,
                img = imgUrl + o.R_ImgUrl,
                id = o.R_ProId,
                typeId = o.R_Type
            });  //新盘推荐

            rs.State = 0;
            rs.Msg = "ok";
            rs.Data = new
            {
                buildingImg = vImg,
                buidlingNum = vNumber,
                eventCount = eventsCount,
                lifeCount,
                notice,
                services,
                goods,
                events,
                houses,
            };
            
          //  Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
           // var user = uCore.LoadEntity(o => o.U_Id == uid);
            //if (user!=null && user.U_AuditingState == (int)AuditingEnum.未认证)
            //{

            //    #region 新用户注册，若没有通过用户认证成功，推送:课堂、认证流程

            //    JPushMsgModel jm1 = new JPushMsgModel()
            //    {
            //        code = (int)MessageCenterModuleEnum.邻妹妹,
            //        proFlag = (int)PushMessageEnum.社区活动跳转,
            //        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tags = "邻信玩法",
            //        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tips = "邻妹妹30秒玩转了邻信，都来试试看还能更快点吗？",
            //        title = "邻信课堂开课啦",
            //        logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/ways/index.html",
            //        proName = "才30秒，能慢点吗？"
            //    };
            //    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), user.U_Id.ToString("N").ToLower());



            //    JPushMsgModel jm2 = new JPushMsgModel()
            //    {
            //        code = (int)MessageCenterModuleEnum.邻妹妹,
            //        proFlag = (int)PushMessageEnum.社区活动跳转,
            //        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tags = "用户认证",
            //        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tips = "第一款基于自家小区的邻居实时通讯的交友平台",
            //        title = "邻信提醒您，您还未进行社区认证",
            //        logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/authflow/index.html",
            //        proName = "社区认证真实交友新玩法"
            //    };
            //    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm2.title, jm2.title, JsonSerialize.Instance.ObjectToJson(jm2), user.U_Id.ToString("N").ToLower());


            //    #endregion
            //}
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 初始化推送消息（用户登录成功消息推送）
        /// </summary>
        /// <returns></returns>
         public HttpResponseMessage InitPushMsg()
        {
            ResultViewModel rs = new ResultViewModel(0,"ok",null);
           // Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            //var user = uCore.LoadEntity(o => o.U_Id == uid);
            //if (user != null && (user.U_AuditingState == (int)AuditingEnum.未认证 || user.U_AuditingState == (int)AuditingEnum.认证失败))
            //{

            //    #region 新用户注册，若没有通过用户认证成功，推送:课堂、认证流程

            //    JPushMsgModel jm1 = new JPushMsgModel()
            //    {
            //        code = (int)MessageCenterModuleEnum.邻妹妹,
            //        proFlag = (int)PushMessageEnum.社区活动跳转,
            //        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tags = "邻信玩法",
            //        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tips = "邻妹妹30秒玩转了邻信，都来试试看还能更快点吗？",
            //        title = "邻信课堂开课啦",
            //        logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/ways/index.html",
            //        proName = "才30秒，能慢点吗？"
            //    };
            //    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), user.U_Id.ToString("N").ToLower());



            //    JPushMsgModel jm2 = new JPushMsgModel()
            //    {
            //        code = (int)MessageCenterModuleEnum.邻妹妹,
            //        proFlag = (int)PushMessageEnum.社区活动跳转,
            //        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tags = "用户认证",
            //        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //        tips = "第一款基于自家小区的邻居实时通讯的交友平台",
            //        title = "邻信提醒您，您还未进行社区认证",
            //        logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/authflow/index.html",
            //        proName = "社区认证真实交友新玩法"
            //    };
            //    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm2.title, jm2.title, JsonSerialize.Instance.ObjectToJson(jm2), user.U_Id.ToString("N").ToLower());


            //    #endregion
            //}
            return WebApiJsonResult.ToJson(rs);
        }
    }
}

