using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Antlr.Runtime;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;
using WebGrease.Css.Extensions;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 邻里圈，带电影票
    /// </summary>
    public class TicketController : BaseController
    {

        private ViewTopicMovieTicketCore tCore = new ViewTopicMovieTicketCore();
        private TopicMovieTicketCore hisCore = new TopicMovieTicketCore();
        private TopicCore topicCore = new TopicCore();
        private ViewTopicMovieTicketReceiveCore receiveCore = new ViewTopicMovieTicketReceiveCore();
        private ViewTopicMovieTicketSendCore sendCore = new ViewTopicMovieTicketSendCore();

        /// <summary>
        /// 领取记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid topicId = obj.topicId;

            var info = tCore.LoadEntities(o => o.T_TopicId == topicId).OrderByDescending(o => o.C_Time);
            rs.Data = new
            {
                baseImgUrl = StaticHttpUrl,
                info
            };
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 领券
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Receive([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid topicId = obj.topicId;
            Guid receiveUserId = obj.receiveUserId;
            int ticketId = obj.ticketId;

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string logo = obj.logo;
            string nickname = obj.nickname;

            var topic =
                topicCore.LoadEntity(
                    o => o.T_Status == 0 && o.T_UserId == uid && o.T_Id == topicId && o.T_Ticket == (int)LuckGiftFlagEnum.有红包);
            if (topic != null)
            {
                topic.T_Ticket = (int)LuckGiftFlagEnum.红包被领光;
                topicCore.UpdateEntitiesNoSave(topic);

                var ticket = hisCore.LoadEntity(o => o.T_Id == ticketId && o.T_ReceiveUserId == null);
                if (ticket != null)
                {
                    ticket.T_ReceiveUserId = receiveUserId;
                    ticket.T_ReceiveTime = DateTime.Now;
                    hisCore.UpdateEntitiesNoSave(ticket);

                    if (hisCore.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";



                        #region 消息推送
                        JPushMsgModel jm = new JPushMsgModel()
                        {
                            code = (int)MessageCenterModuleEnum.邻里圈,
                            nickname = nickname,
                            proFlag = (int)PushMessageEnum.用户跳转,
                            proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            tags = "电影券",
                            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            title = "您收到一张电影券",
                            tips = " 给你一张电影票",
                            uid = uid.ToString()
                        };
                        Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), receiveUserId.ToString("N").ToLower());

                        #endregion
                    }
                }
            }
            else
            {
                rs.Msg = "暂无更多可分配的电影票";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 收到电影票历史记录
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ReceiveHistory()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            var info = receiveCore.LoadEntities(o => o.T_ReceiveUserId == uid).OrderByDescending(o => o.T_ReceiveTime);
            UserCore uCore = new UserCore();
            List<dynamic> list = new List<dynamic>();
            foreach (var o in info)
            {
                var user = uCore.LoadEntity(p => p.U_Id == o.U_Id);

                list.Add(new
                {
                    uid = user.U_Id,
                    nickname = user.U_NickName,
                    logo = StaticHttpUrl + user.U_Logo,
                    age = user.U_Age,
                    sex = user.U_Sex,
                    buildingId = user.U_BuildingId,
                    buildingName = user.U_BuildingName,
                    chatId = user.U_ChatID,
                    topicId = o.T_TopicId,
                    topicName = o.T_TopicName,
                    ticketId = o.T_Id,
                    datetime = o.T_ReceiveTime,

                    movieAddress = o.T_Cinema,
                    movieCode = o.T_MovieCode,
                    movieName = o.T_MovieName
                });
            }
             

            //rs.Data = info.Select(o => );
            rs.Data = list;

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 发送电影票历史记录
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage SendHistory()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            var info = sendCore.LoadEntities(o => o.U_Id == uid).OrderByDescending(o => o.T_DatetTime);
            UserCore uCore = new UserCore();
            List<dynamic> list = new List<dynamic>();
            foreach (var o in info)
            {
                Core_User user = new Core_User();
                if (o.T_ReceiveUserId!= null && o.T_ReceiveUserId!=Guid.Empty)
                {
                    user = uCore.LoadEntity(p => p.U_Id == o.T_ReceiveUserId);
                }

                list.Add(new
                {
                    uid = user.U_Id,
                    nickname = user.U_NickName,
                    logo = StaticHttpUrl + user.U_Logo,
                    age = user.U_Age,
                    sex = user.U_Sex,
                    buildingId = user.U_BuildingId,
                    buildingName = user.U_BuildingName,
                    chatId = user.U_ChatID,
                    topicId = o.T_TopicId,
                    topicName = o.T_TopicName,
                    ticketId = o.T_Id,
                    datetime = o.T_ReceiveTime,

                    movieAddress = o.T_Cinema,
                    movieCode = o.T_MovieCode,
                    movieName = o.T_MovieName
                });
            }
            rs.Data = list;


            return WebApiJsonResult.ToJson(rs);
        }


    }
}
