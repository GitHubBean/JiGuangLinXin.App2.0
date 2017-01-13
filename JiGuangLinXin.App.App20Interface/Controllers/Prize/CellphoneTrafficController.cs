using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ServicesModel;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers.Prize
{
    /// <summary>
    /// 手机流量
    /// </summary>
    public class CellphoneTrafficController : ApiController
    {
        private CellphoneTrafficCore tCore = new CellphoneTrafficCore();
        private ScoreCore scoreCore = new ScoreCore();
        private ScoreExchangeCore exCore = new ScoreExchangeCore();
        private UserCore ucore = new UserCore();


        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            string uidStr = obj.uid; //用户ID

            var trafficList = tCore.GetCellphoneTrafficList();  //流量套餐列表
            //中奖
            var orderList = exCore.LoadEntities(o => o.E_Module == (int)EventH5ModuleEnum.签到送流量).OrderByDescending(o => o.E_Time).Take(50).Select(o => new
            {
                bName = o.E_BuildingName,
                phone = o.E_Phone.Replace(o.E_Phone.Substring(2, 5), "*****"),
                title = o.E_Title
            });
            //积分
            int score = 0;
            if (!string.IsNullOrEmpty(uidStr))
            {
                Guid uid = Guid.Parse(uidStr);
                var scoreInfo = scoreCore.LoadEntity(o => o.S_AccountId == uid);

                if (scoreInfo != null)
                {
                    score = scoreInfo.S_Score;
                }
            }



            string timeLimit = ConfigurationManager.AppSettings["TrafficTimeLimit"];

            rs.Data = new
            {
                score,
                trafficList,
                orderList,
                timeLimit
            };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage SignIn([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = obj.uid; //用户ID

            rs = tCore.SingIn(uid); //签到

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 流量充值
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage TrafficPay([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid uid = obj.uid;  //用户的id
            int proId = obj.proId;//流量套餐编号 Convert.ToInt32(HttpContext.Current.Request["proId"]);
            var user = ucore.LoadEntity(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 && o.U_AuditingState == (int)AuditingEnum.认证成功);
            if (user == null)
            {
                rs.State = 1;
                rs.Msg = "用户不存在";
                return WebApiJsonResult.ToJson(rs);
            }

            string phone = user.U_LoginPhone;//用户的电话
            int phoneType = tCore.GetPhoneType(phone);
            if (phoneType == -1)
            {
                rs.State = 1;
                rs.Msg = "该号段暂未开通流量便捷充值";
                return WebApiJsonResult.ToJson(rs);
            }


            //根据套餐获取对应的积分数量
            var traList = tCore.GetCellphoneTrafficList();
            var proInfo = traList.First(o => o.TraId == proId);

            string timeLimit = ConfigurationManager.AppSettings["TrafficTimeLimit"];
            int countLimit = proInfo.LimitCount;
            rs = scoreCore.CellphoneTraffic(uid, proInfo.Score, proInfo.Traffic, timeLimit, countLimit);

            if (rs.State == 0)  //服务端处理成功
            {
                dynamic cc = JsonSerialize.Instance.JsonToObject<dynamic>(rs.Data);

                //根据自定义的不同套餐、不同的运营商，组合计算套餐ID
                int[] plans = new int[] { };
                switch (proId)
                {
                    case 1:  //20M
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 34 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 3, 3 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 8, 8 };
                        }
                        break;
                    case 2:  //50M
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 1 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 3, 3, 4 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 32 };
                        }
                        break;
                    case 3: //100M
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 35 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 4, 5 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 10 };
                        }
                        break;
                    case 4://150M
                        if (phoneType == 1)//联通k
                        {
                            plans = new int[] { 1, 35 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 3, 5, 5 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 10, 32 };
                        }
                        break;
                    case 5://200M
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 2 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 4, 4, 5, 5 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 11 };
                        }
                        break;
                    case 6://500M
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 36 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 7 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 12 };
                        }
                        break;
                    case 7://1G
                        if (phoneType == 1)//联通
                        {
                            plans = new int[] { 36, 36 };
                        }
                        else if (phoneType == 2)//移动
                        {
                            plans = new int[] { 26 };
                        }
                        else if (phoneType == 3) //电信
                        {
                            plans = new int[] { 28 };
                        }
                        break;
                }
                //分批充值流量
                bool flag = false;
                List<string> orders = new List<string>();
                foreach (var item in plans)
                {
                    string orderId = phone + DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                                     new Random(Guid.NewGuid().GetHashCode()).Next(0, 100); // Guid.NewGuid().ToString("N");//订单号 
                    string sign =
                        Juhe.MakeMD5(Juhe.OpenID + Juhe.TrafficKey + phone + item + orderId);

                    string json = Juhe.Client(Juhe.TrafficSite + "?key=" + Juhe.TrafficKey
                                              + "&phone=" + phone
                                              + "&pid=" + item
                                              + "&orderid=" + orderId
                                              + "&sign=" + sign
                        );
                    dynamic rsOrder = JsonSerialize.Instance.JsonToObject<dynamic>(json);

                    flag = rsOrder.error_code == 0;//聚合充值成功
                    orders.Add(orderId);
                }

                if (!orders.Any())
                {
                    rs.State = 1;
                    return WebApiJsonResult.ToJson(rs);
                }
                string orderStr = string.Join(",", orders);

                int billId = cc.billId; //订单号
                var exInfo = exCore.LoadEntity(o => o.E_Id == billId);
                exInfo.E_OrderNo = orderStr;

                if (!flag)  //聚合平台充值失败
                {
                    //退积分,这里后期可以调用 refund 退款接口
                    LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Service, LogType.Daily);
                    log.Write(HttpContext.Current.Request.UserHostAddress + "\n" + Request.RequestUri + "\n 平台流量充值失败，退积分:" + orderStr, LogLevel.Error);

                    exInfo.E_Status = 1;
                    exCore.UpdateEntity(exInfo);

                    rs.State = 2;
                    rs.Msg = "通信运营商正在维护，请稍后再试";
                }
                else  //充值成功
                {
                    exInfo.E_Status = 0;
                    if (exCore.UpdateEntity(exInfo))  //充值成功，修改对应的订单号 
                    {
                        rs.State = 0;
                        rs.Msg = "流量充值成功，请注意查收！";
                    }
                    else
                    {
                        rs.State = 2;
                        rs.Msg = "通信运营商正在维护，请稍后再试..";
                    }
                }

            }
            return WebApiJsonResult.ToJson(rs);
        }


    }
}
