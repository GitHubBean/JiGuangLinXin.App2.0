using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers.Prize
{
    /// <summary>
    /// 老虎机抽电影票
    /// </summary>
    public class FilmController : ApiController
    {

        private ScoreExchangeCore exCore = new ScoreExchangeCore();
        private UserCore uCore = new UserCore();
        private  List<Dictionary<string, string>> events = JsonSerialize.Instance.JsonToObject<List<Dictionary<string, string>>>(ConfigurationManager.AppSettings["EventIdList"]);
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = obj.uid; //用户ID


            #region 检测活动是否合法（存在、过期）
            bool acFlag = false;
            foreach (var ev in events)
            {
                if (ev["key"] == "EventFilm")
                {
                    acFlag = new EventCore().IsActivity(ev["value"]);
                    break;
                }
            }
            if (!acFlag)
            {

                rs.State = 1;
                rs.Msg = "暂无有效的活动";
                return WebApiJsonResult.ToJson(rs);
            }
            #endregion


            var user = uCore.LoadEntity(o => o.U_Id == uid && o.U_Status == 0 && o.U_AuditingState == 1);
            if (user != null)
            {
                //中奖
                var info =
                    exCore.LoadEntities(o => o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票 && o.E_UId == uid)
                        .FirstOrDefault();

                string title = "";
                int flag = 0;
                int num = 0;
                if (info != null)
                {
                    title = info.E_Title;
                    flag = info.E_Flag;
                    if (info.E_Flag == (int)FilmFlagEnum.电影票)
                    {
                        var yl = 80 - exCore.LoadEntities(
                            o =>
                                o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票 && o.E_Flag == (int)FilmFlagEnum.电影票 &&
                                o.E_BuildingId == user.U_BuildingId)
                            .Count();
                        num = yl > 0 ? yl : 0;
                    }
                }
                rs.Data = new
                {
                    title,
                    flag,
                    phone = user.U_LoginPhone,
                    num
                };
            }
            else
            {
                rs.State = 1;
                rs.Msg = "本次活动，当前用户暂无资格参加";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 玩玩老虎机
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage JustPlay([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = obj.uid; //用户ID

            #region 检测活动是否合法（存在、过期）
            bool acFlag = false;
            foreach (var ev in events)
            {
                if (ev["key"] == "EventFilm")
                {
                    acFlag = new EventCore().IsActivity(ev["value"]);
                    break;
                }
            }
            if (!acFlag)
            {

                rs.State = 1;
                rs.Msg = "暂无有效的活动";
                return WebApiJsonResult.ToJson(rs);
            }
            #endregion

            rs = exCore.PlaySlot(uid);
            if (rs.State == 0)
            {
                var arr = rs.Msg.Split('#');
                if (arr.Count() == 2)
                {
                    sms sms = new sms();
                    var smsrs = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                                    arr[1], arr[0]); //发送短消息
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 这是一个蛋疼的接口，给小区中奖的业主返业主卡金额到帐号（不通过实体卡兑换）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Danteng([FromBody] JObject value)
        {
            ResultMessageViewModel rs = exCore.Danteng();

            sms sms = new sms();
            var smsrs = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                           "15825942359", "恭喜您在“邻信电影季”活动抽中30元现金业主卡一张，您所在社区已领取业主卡20张，总数100张，领完为止。赶紧邀请邻居也入驻邻信参加活动，更有机会观看免费包场电影，赶紧行动起来吧！"); //发送短消息
            return WebApiJsonResult.ToJson(rs);
        }
    }
}
