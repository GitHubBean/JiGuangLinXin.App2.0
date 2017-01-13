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
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 领红包
    /// </summary>
    public class GiftReceiveController : BaseController
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        private LuckyGiftHistoryCore historyCore = new LuckyGiftHistoryCore();
        private UserCore userCore = new UserCore();
        private ViewReceiveGiftListCore receiveCore = new ViewReceiveGiftListCore();

        /// <summary>
        /// 抢群里的红包
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ReceiveGroup([FromBody]JObject value)
        {
            dynamic obj = value;


            var tips = System.Web.Configuration.WebConfigurationManager.AppSettings["giftTip"];

            var arr = tips.Split('|');
            Random ran = new Random();
            string tip = arr[ran.Next(0, arr.Length - 1)]; //随机获取祝福语


            Guid giftId = obj.giftId;
            Guid uid = obj.uid;
            Guid groupId = obj.groupId;
            var temp = giftCore.ReceiveGiftGroup(uid, groupId, giftId, tip);


            ResultViewModel rs = new ResultViewModel(temp.State, temp.Msg, new
            {
                imgUrl = StaticHttpUrl,
                giftInfo = temp.Data
            });
            //var rs = new ResultViewModel()
            //{
            //    Data = JsonSerialize.Instance.NewtonsoftSerialize(temp)
            //};

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 收好友发的红包
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ReceiveSingle([FromBody]JObject value)
        {
            dynamic obj = value;


            var tips = System.Web.Configuration.WebConfigurationManager.AppSettings["giftTip"];

            var arr = tips.Split('|');
            Random ran = new Random();
            string tip = arr[ran.Next(0, arr.Length - 1)]; //随机获取祝福语


            Guid giftId = obj.giftId;
            Guid uid = obj.uid;


            var temp = giftCore.ReceiveGiftSingle(uid, giftId, tip);


            ResultViewModel rs = new ResultViewModel(temp.State, temp.Msg, new
            {
                imgUrl = StaticHttpUrl,
                giftInfo = temp.Data
            });
            //var rs = new ResultViewModel()
            //{
            //    Data = JsonSerialize.Instance.NewtonsoftSerialize(temp)
            //};

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 收红包历史记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ReceiveHistory([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;

            int pn = obj.pn;
            pn = pn - 1;
            int rows = obj.rows;
            Guid uid = obj.uid;

            var user = userCore.LoadEntity(o => o.U_Id == uid);
            if (user != null)
            {

                var allList = receiveCore.LoadEntities(o => o.userReceiveId == uid);//收到的所有的红包

                if (allList.Any()) //有记录
                {
                    var list = allList.OrderByDescending(o => o.createTime).Skip(pn * rows).Take(rows);//红包分页记录

                    var rdata = new
                    {
                        imgUrl = StaticHttpUrl,
                        headImg =user.U_Logo,
                        nickname = user.U_NickName,
                        totalCount = allList.Count(),
                        totalMoney = allList.Sum(o => o.money).ToString("N"),
                        giftList = list
                    };
                    rs.Data = rdata; rs.State = 0;
                }
                else
                {
                    rs.State = 2;
                    rs.Msg = "还没有收到红包哦";
                    var rdata = new
                    {
                        imgUrl = StaticHttpUrl,
                        headImg = StaticHttpUrl + user.U_Logo,
                        nickname = user.U_NickName,
                        totalCount = 0,
                        totalMoney = 0,
                        giftList = ""
                    };
                    rs.Data = rdata; 
                }
            }
            else
            {
                rs.State = 1;
                rs.Msg = "用户帐号不存在";
            }

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
