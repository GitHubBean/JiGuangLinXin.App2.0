using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Provide.EncryptHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class GiftSendController : BaseController
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        //private AppBuildingCore buildingCore = new AppBuildingCore();
        private LuckyGiftHistoryCore historyCore = new LuckyGiftHistoryCore();
        private UserCore userCore = new UserCore();


        //
        // GET: /SendGift/
        /// <summary>
        /// 发红包
        /// </summary>
        /// <param name="value">红包信息</param>
        /// <returns></returns>
        public HttpResponseMessage Group([FromBody]JObject value)
        {
            dynamic obj = value;
             
            string enmoney = obj.money;
            int count = obj.count;
            Guid uid = obj.uid;
            string tips = obj.tips;
            string paypwd = obj.payPwd;

            double money = Convert.ToDouble(DESProvider.DecryptString(enmoney));
            var rs = giftCore.SendGiftGroup(money, count, uid, "社区群聊天红包", tips,paypwd);

            return WebApiJsonResult.ToJson(rs);
            //return new ResultViewModel() { Status = 0, Msg = "success", Data = "{user:'" + obj.user + "',age:'"+obj.age+"'}" };
            //  return rs;
        }
        /// <summary>
        /// 发送单个红包
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Single([FromBody]JObject value)
        {
            dynamic obj = value;

            string enmoney = obj.money;
            Guid uid = obj.uid;
            Guid targetUid = obj.targetUid; //环信ID
            string tips = obj.tips;
            string paypwd = obj.payPwd;

            decimal money = Convert.ToDecimal(DESProvider.DecryptString(enmoney));

            var rs = giftCore.SendGiftSingle(money, uid, targetUid, tips, "社区聊天单发红包", paypwd);
            return WebApiJsonResult.ToJson(rs);

            //return new ResultViewModel() { Status = 0, Msg = "success", Data = "{user:'" + obj.user + "',age:'"+obj.age+"'}" };
            //  return rs;
        }


        /// <summary>
        /// 获取会员发送的所有红包
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ListByUid([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;

            int pn = obj.pn;
            int rows = obj.rows;

            Guid uid = obj.uid;

            var user = userCore.LoadEntity(o => o.U_Id == uid);

            if (user != null)
            {
                var allList = giftCore.LoadEntities(o => o.LG_UserId == uid);//发放的所有的红包
                if (allList.Any())//有记录
                {
                    var list = allList.OrderByDescending(o => o.LG_CreateTime).Skip((pn - 1) * rows).Take(rows).ToList();//发红包分页记录

                    //返回红包列表的部分字段
                    var gList = from g in list
                        select
                            new
                            {
                                giftId = g.LG_Id,
                                flag = g.LG_Type,
                                title = g.LG_Title,
                                money = g.LG_Money.ToString("N"),
                                createTime = g.LG_CreateTime
                            };

                    var rdata = new
                    {
                        userId = user.U_Id,
                        headImg = StaticHttpUrl + user.U_Logo,
                        nikeName = user.U_NickName,
                        totalCount = allList.Count(),
                        totalMoney = allList.Sum(o => o.LG_Money).ToString("N"),
                        giftList = gList
                    };
                    rs.Data = rdata;
                    rs.State = 0;
                }
                else
                {

                    rs.State = 2;
                    rs.Msg = "还没有发过红包哦";
                    var rdata = new
                    {
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


        /// <summary>
        /// 获取会员发送的单个红包详细记录
        /// </summary>
        /// <param name="giftId">红包ID</param>
        /// <param name="uid">会员ID</param>
        /// <returns></returns>
        public HttpResponseMessage GiftDetail([FromBody]JObject v)
        {
            var rs = new ResultViewModel();
            dynamic value = v;
            //int uid = int.Parse(value.uid);
            Guid giftId = value.giftId;
            var gift = giftCore.LoadEntity(o => o.LG_Id == giftId);  //会员发送的红包
            var giftHistory = historyCore.LoadEntities(o => o.LH_GiftId == giftId).ToList();

            if (gift == null)
            {
                rs.State = 1;
                rs.Msg = "红包不存在";
            }
            else
            {
                var user = userCore.LoadEntity(o => o.U_Id == gift.LG_UserId);  //发红包的会员
                if (user == null)
                {
                    rs.State = 1;
                    rs.Msg = "用户帐号不存在";
                }
                else
                {
                    if (giftHistory.Any())//有记录
                    {
                        //返回红包领取记录的部分字段
                        var hList = from g in giftHistory
                            select
                                new
                                {
                                    headImg = StaticHttpUrl+ g.LH_UserLogo,
                                    nickname = g.LH_UserNickName,
                                    tips = g.LH_Remark,
                                    money = g.LH_Money.ToString("N"),
                                    createTime = g.LH_CreateTime
                                };

                        var rdata = new
                        {
                            flag = gift.LG_Status,
                            headImg = StaticHttpUrl + user.U_Logo,
                            nickname = user.U_NickName,
                            buildingName = user.U_BuildingName,
                            giftTips = gift.LG_Title,
                            money = gift.LG_Money.ToString("N"),
                            remainMoney = gift.LG_RemainMoney.ToString("N"),
                            count = gift.LG_Count,
                            remainCount = gift.LG_RemainCount,
                            history = hList
                        };
                        rs.Data = rdata;
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                    else
                    {
                        rs.State = 2;
                        rs.Msg = "还没有小伙伴领取该红包哦";
                        var rdata = new
                        {
                            flag = gift.LG_Status,
                            headImg = StaticHttpUrl + user.U_Logo,
                            nickname = user.U_NickName,
                            buildingName = user.U_BuildingName,
                            giftTips = gift.LG_Title,
                            money = gift.LG_Money.ToString("N"),
                            remainMoney = gift.LG_RemainMoney.ToString("N"),
                            count = gift.LG_Count,
                            remainCount = gift.LG_RemainCount,
                            history = ""
                        };
                        rs.Data = rdata;
                    }

                }
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 获取会员发送的红包领取记录
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PostSendGiftDetail([FromBody]JObject value)
        {
            dynamic obj = value;
            Guid uid = obj.uid;
            Guid giftId = obj.giftId;

            var temp = historyCore.GetSendGiftDetail(giftId, uid);
            //var rs = new ResultViewModel()
            //{
            //    Data = JsonSerialize.Instance.NewtonsoftSerialize(temp)
            //};

            return WebApiJsonResult.ToJson(temp);
        }


    }
}
