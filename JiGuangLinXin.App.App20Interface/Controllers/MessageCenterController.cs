using System;
using System.Data.Entity.Spatial;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 消息中心
    /// </summary>
    public class MessageCenterController : BaseController
    {
        private MessageCenterCore msgCore = new MessageCenterCore();
        private UserCore uCore = new UserCore();
        /// <summary>
        /// 社区认证记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BuildingAuditingCheck([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            int pn = obj.pn;
            pn = pn - 1;
            int rows = obj.rows;

            var user = uCore.LoadEntity(o => o.U_Id == uid && o.U_Status == 0 && o.U_AuditingState == (int)AuditingEnum.认证成功 && o.U_AuditingManager == (int)AuditingEnum.认证成功);
            if (user != null && user.U_BuildingId == buildingId)
            {

                var rsdata = uCore.GetAuditingUserList(buildingId, pn, rows);
                rs.Data = new
                {
                    baseImgUrl = StaticHttpUrl,
                    info = rsdata.Data
                };
            }
            else
            {
                rs.State = 1;
                rs.Msg = "非法操作";
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 官方推送的消息  精品汇、社区活动、游戏中心、新家推荐、社区服务
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OfficialPush([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            dynamic obj = value;
            string buildingId = obj.buildingId;
            int place = obj.place;  //消息推送的区域地址（社区服务、精品汇。。）

            int pn = obj.pn;
            pn = pn - 1;
            int rows = obj.rows;

            rs.Data =
                msgCore.LoadEntities(o => o.A_Status == 0 && o.A_Place == place && (o.A_Building.Contains(buildingId) || o.A_Target == 0))
                    .OrderByDescending(o => o.A_Time)
                    .Skip(pn * rows)
                    .Take(rows)
                    .Select(o => new
                    {
                        mId = o.A_Id,
                        place = o.A_Place,
                        target = o.A_Target,// 0全平台 1定向小区
                        coverImg = StaticHttpUrl + o.A_ImgUrl,
                        title = o.A_Title,
                        desc = o.A_Desc,
                        linkUrl = o.A_Linkurl,
                        content = o.A_Content,
                        time = o.A_Time,
                        remark = o.A_Remark,
                        clicks = o.A_Clicks,
                        proName = o.A_ProjectName,
                        proId = o.A_ProjectId,
                        tags = o.A_Tags

                    });

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 附近的邻友
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage NearbyLinyou([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            dynamic obj = value;
            float lng = obj.lng;
            float lat = obj.lat;
            int radius = obj.radius;

            if (Math.Abs(lng) < 0 || Math.Abs(lat) < 0)//定位失败
            {
                rs.State = 1;
                rs.Msg = "定位失败，未能搜寻到附近的人";
            }
            else
            {
                var uinfo = uCore.LoadEntity(o => o.U_Id == uid);

                DbGeography location = null;
                try
                {
                    location = DbGeography.FromText(string.Format("POINT({0} {1})", lng, lat), 4326);
                }
                catch (Exception)
                {

                    throw;
                }

                uinfo.U_CoordX = lat;
                uinfo.U_CoordY = lng;
                uinfo.U_Location = location;

                uCore.UpdateEntity(uinfo);  //刷新用户的登录坐标

                rs.Data = new
                {
                    baseImgUrl = StaticHttpUrl,
                    info = msgCore.QueryNearbyLinyou(lng, lat, radius)
                };
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        ///查询环信消息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage QueryPushMsg([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            string uid = Request.Headers.GetValues("uid").FirstOrDefault();

            dynamic obj = value;
            string buildingId = obj.buildingId;

            var msgCore = new JPushMessageCore();

            //rs.Data = msgCore.LoadEntities(o => o.M_State == 0 && (o.M_Type == 3 || o.M_TargetId == uid || o.M_TargetId == buildingId)).Select(o => new
            //{
            //    o.M_Id,
            //    o.M_Type,
            //    o.M_Title,
            //    o.M_Content,
            //    o.M_Time
            //});

            //单人 未读 消息
            var list1 = msgCore.LoadEntities(o => o.M_State == 0 && o.M_TargetId == uid);
            //群/全体 未读消息 （M_Type=3 标识全体）
            var list2 = msgCore.LoadEntities(o =>  o.M_State == 0  && !o.M_ReadList.Contains(uid) && (o.M_TargetId == buildingId || o.M_Type == 3));
            var list = list1.Concat(list2);
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list.Select(o => new
                {
                    o.M_Id,
                    o.M_Type,
                    o.M_Title,
                    o.M_Content,
                    o.M_Time
                });
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂没有消息";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 读取推送消息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ReadPushMsg([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            string uid = Request.Headers.GetValues("uid").FirstOrDefault();

            dynamic obj = value;
            int code = obj.code;
            string buildingId = obj.buildingId;

            var msgCore = new JPushMessageCore();

            //1.修改单人消息已读　
            var f1 = msgCore.UpdateByExtended(o => new Sys_JPushMessage() { M_State = 1 }, o => o.M_TargetId == uid && o.M_State == 0 && o.M_Code == code);

            //2.如果是群消息、或者是全体消息
            var list = msgCore.LoadEntities(o => o.M_State == 0 && (o.M_TargetId == buildingId || o.M_Type == 3) && o.M_Code == code && !o.M_ReadList.Contains(uid));

            foreach (var item in list)
            {
                item.M_ReadList += "," + uid;
                msgCore.UpdateEntitiesNoSave(item);
            }
            msgCore.SaveChanges();

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
