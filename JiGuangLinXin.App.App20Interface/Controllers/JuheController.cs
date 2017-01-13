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
using JiGuangLinXin.App.Entities.ServicesModel;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class JuheController : BaseController
    {
        private JuheDeptCore deptCore = new JuheDeptCore();
        private JuheOrderCore orderCore = new JuheOrderCore();
        private BalanceCore bCore = new BalanceCore();

        /// <summary>
        /// 根据城市和缴费项，查询可以缴费的单位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage QueryDept([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(1, "暂无数据", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());  //用户的id

            string cityName = obj.cityName;
            string proName = obj.proName;


            var deptList = deptCore.LoadEntities(p => p.D_CityName == cityName && p.D_ProjectName == proName);
            if (deptList.Any())//存在
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = deptList.Select(o => new
                {
                    ProductID = o.D_ProductId,
                    ProvinceID = o.D_ProvinceID,
                    ProvinceName = o.D_ProvinceName,
                    CityID = o.D_CityID,
                    CityName = o.D_CityName,
                    ProjectID = o.D_ProjectID,
                    ProjectName = o.D_ProjectName,
                    UnitID = o.D_UnitID,
                    UnitName = o.D_UnitName,
                    DeptId = o.D_Id


                });
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 用户查询 便民缴费的的金额
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ConvenienceArrears()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "暂无数据", null);

            //rs.State = 0;
            //rs.Msg = "ok";
            //rs.Data = new
            //{
            //    Balance = DESProvider.EncryptString("100"),
            //    Contract = Guid.NewGuid().ToString("N"),
            //    MentDay = "2016-03-18"
            //};

            //return WebApiJsonResult.ToJson(rs);


            string provname = HttpContext.Current.Request["ProvinceName"];
            string cityname = HttpContext.Current.Request["CityName"];
            string type = null;
            switch (HttpContext.Current.Request["ProjectName"])
            {
                case "水费":
                    type = "001";
                    break;
                case "电费":
                    type = "002";
                    break;
                case "燃气费":
                    type = "003";
                    break;
            }
            string code = HttpContext.Current.Request["UnitID"];
            string name = HttpContext.Current.Request["UnitName"];
            string account = HttpContext.Current.Request["Account"];//卡号
            string cardid = HttpContext.Current.Request["ProductID"];//商品ID

            string json = Juhe.Client(Juhe.PublicSite + "mbalance?key=" + Juhe.PublicKey
                + "&provname=" + provname
                + "&cityname=" + cityname
                + "&type=" + type
                + "&code=" + code
                + "&name=" + name
                + "&account=" + account
                + "&cardid=" + cardid
                + "&paymodeid=2");

            JuheBalance balance = JsonConvert.DeserializeObject<JuheBalance>(json);
            if (balance.result == null || balance.result.balances == null)
            {
                rs.Msg = balance.reason;
            }
            else
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("Balance", balance.result.balances.balance.balance);//欠费金额
                values.Add("Contract", balance.result.balances.balance.contractNo);//合同号
                values.Add("MentDay", balance.result.balances.balance.payMentDay);//账期
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = values;
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 便民缴费（水、电、气）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ConveniencePay([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());  //用户的id
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();//用户的电话

            string ProvinceID = obj.ProvinceID;
            string CityID = obj.CityID;
            string ProjectID = obj.ProjectID;
            string UnitID = obj.UnitID;
            string UnitName = obj.UnitName;
            string ProductID = obj.ProductID;
            string Account = obj.Account;
            string Contract = obj.Contract;
            string MentDay = obj.MentDay;
            decimal Money = 0;
            string ProName = obj.ProName;
            int deptId = obj.DeptId;
            string enMoney = obj.Money;

            string enPaypwd = obj.payPwd;  //必须输入支付密码
            if (!decimal.TryParse(DESProvider.DecryptString(enMoney), out Money))
            {
                rs.Msg = "金额有误";
                return WebApiJsonResult.ToJson(rs);
            }

            string orderid = Account + DateTime.Now.ToString("yyyyMMddHHmmss");//订单号

            Core_JuheOrder order = new Core_JuheOrder()
            {
                O_Account = Account,
                O_Id = Guid.NewGuid(),
                O_Money = Money,
                O_OrderNo = orderid,
                O_PayUnitName = UnitName,
                O_Phone = phone,
                O_Remark = "邻信官方便民平台",
                O_Status = (int)PayOffEnum.未销帐,
                O_Time = DateTime.Now,
                O_Type = (int)Enum.Parse(typeof(PaymentTypeEnum), ProName),
                O_UId = uid,
                O_DeptId = deptId,
                O_DeptProjectName = ProName

            };



            //#region 内网

            //rs = orderCore.Payment(order, enPaypwd);
            //rs.Msg = "已缴费成功";
            //rs.Data = new ConvenientViewModel
            //{
            //    AccountNo = order.O_Account,
            //    OrderDate = order.O_Time.ToString("yyyy-MM-dd HH:mm"),
            //    OrderNo = orderid,
            //    PayWay = "余额支付",
            //    UnitName = UnitName
            //};

            //rs.State = 0;

            //return WebApiJsonResult.ToJson(rs);
            //#endregion



            rs = orderCore.Payment(order, enPaypwd);
            if (rs.State == 0) //缴费、扣款成功
            {
                string sign =
                    MakeMD5(Juhe.OpenID + Juhe.PublicKey + ProductID + Money + orderid + ProvinceID + CityID + ProjectID +
                            UnitID + Account);

                string json = Juhe.Client(Juhe.PublicSite + "order?key=" + Juhe.PublicKey
                                          + "&provid=" + ProvinceID
                                          + "&cityid=" + CityID
                                          + "&type=" + ProjectID
                                          + "&code=" + UnitID
                                          + "&cardid=" + ProductID
                                          + "&account=" + Account
                                          + "&contract=" + Contract
                                          + "&payMentDay=" + MentDay
                                          + "&orderid=" + orderid
                                          + "&cardnum=" + Money
                                          + "&sign=" + sign
                    );


                JuheOrder rsOrder = JsonSerialize.Instance.JsonToObject<JuheOrder>(json);
                if (rsOrder.error_code == 0)  //销帐缴费成功
                {
                    //AppPaymentOrderDal.Insert(orderid, context.Session["UserPhone"].ToString(), money, 0);
                    int pw = orderCore.UpdateByExtended(o => new Core_JuheOrder() { O_Status = (int)PayOffEnum.已销帐 },
                         o => o.O_Id == order.O_Id);
                    if (pw > 0)  //销帐状态已变更
                    {
                        //todo:聚合缴费销帐成功 ，推送消息
                        //Tuisong.User(userid, new MessageInfo(202, new MessageData(orderid, account, money, mode)));
                        order.O_Status = (int)PayOffEnum.已销帐;
                        rs.Msg = "已缴费成功";
                        rs.Data = new ConvenientViewModel
                        {
                            AccountNo = order.O_Account,
                            OrderDate = order.O_Time.ToString("yyyy-MM-dd HH:mm"),
                            OrderNo = orderid,
                            PayWay = "余额支付",
                            UnitName = rsOrder.result.cardname
                        };

                        rs.State = 0;
                    }
                    else
                    {
                        order.O_Status = (int)PayOffEnum.销帐失败;
                        rs.Msg = "销帐成功，但数据失败！";
                    }
                }
                else  //销帐失败，退换金额到余额
                {
                    orderCore.WriteOffFail(order);
                    rs.Msg = "付款成功，但销帐失败，已通知管理员核查";
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 手机充值查询
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PhoneArrears()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "暂无数据", null);
            string phoneno = HttpContext.Current.Request["Phone"];
            string cardnum = HttpContext.Current.Request["Money"];
            if (string.IsNullOrEmpty(phoneno) || string.IsNullOrEmpty(cardnum))
            {
                rs.Msg = "参数错误，请传入正确的参数";
            }
            else
            {
                string json = Juhe.Client(Juhe.MobileSite + "telquery?key=" + Juhe.MobileKey
                    + "&phoneno=" + phoneno
                    + "&cardnum=" + cardnum);

                JuheTelcheck telcheck = JsonConvert.DeserializeObject<JuheTelcheck>(json);
                if (telcheck.error_code == 0)
                {
                    string key = DateTime.Now.ToString("yyyy-MM-dd");
                    //if (context.Application[key] == null)
                    //{
                    //    context.Application.Lock();
                    //    context.Application[key] = 1;
                    //    context.Application.UnLock();
                    //    telcheck.result.inprice = (float.Parse(cardnum) * 0.85f).ToString();
                    //}
                    //else
                    //{
                    //    int count = (int)context.Application[key];
                    //    context.Application.Lock();
                    //    context.Application[key] = count + 1;
                    //    context.Application.UnLock();
                    //    if (count < 1)
                    //    {
                    //        telcheck.result.inprice = (float.Parse(cardnum) * 0.85f).ToString();
                    //    }
                    //    else
                    //    {
                    telcheck.result.inprice = cardnum;
                    //    }
                    //}
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = telcheck.result;
                }
                else
                {
                    rs.Msg = telcheck.reason;
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 手机充值
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PhonePay()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());  //用户的id
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();//登录用户的电话
            int platform = int.Parse(Request.Headers.GetValues("platform").FirstOrDefault());//登录来源
            decimal money = 0;
            string phoneno = HttpContext.Current.Request["Phone"];  //充值到账的号码
            string cardnum = DESProvider.DecryptString(HttpContext.Current.Request["Money"]);

            string enPaypwd = HttpContext.Current.Request["payPwd"];  //必须输入支付密码


            if (cardnum == null || phoneno == null || (!decimal.TryParse(cardnum, out money)))
            {
                rs.Msg = "参数错误，请传入正确的参数";
            }
            else
            {
                string orderid = phoneno + DateTime.Now.ToString("yyyyMMddHHmmss");//订单号


                Core_JuheOrder order = new Core_JuheOrder()
                {
                    O_Account = phoneno,
                    O_Id = Guid.NewGuid(),
                    O_Money = money,
                    O_OrderNo = orderid,
                    O_PayUnitName = "邻信平台通信运营商",
                    O_DeptProjectName = "话费",
                    O_DeptId = 0,
                    O_Phone = phone,
                    O_Remark = "邻信官方便民平台，手机话费充值",
                    O_Status = (int)PayOffEnum.未销帐,
                    O_Time = DateTime.Now,
                    O_Type = (int)PaymentTypeEnum.话费,
                    O_UId = uid
                };



                #region 内网
                rs.Msg = "已缴费成功";
                rs.Data = new ConvenientViewModel
                {
                    AccountNo = order.O_Account,
                    OrderDate = order.O_Time.ToString("yyyy-MM-dd HH:mm"),
                    OrderNo = orderid,
                    PayWay = "余额支付",
                    UnitName = "话费" + money + "元直充"
                };
                rs.State = 0;
                rs = orderCore.Payment(order, enPaypwd);

                rs.Data = new ConvenientViewModel
                {
                    AccountNo = order.O_Account,
                    OrderDate = order.O_Time.ToString("yyyy-MM-dd HH:mm"),
                    OrderNo = orderid,
                    PayWay = "余额支付",
                    UnitName = "邻信平台通信运营商"
                };

                return WebApiJsonResult.ToJson(rs);
                #endregion


                var moneyList = ConfigurationManager.AppSettings["phoneMoneyList"];
                if (moneyList.Contains(order.O_Money.ToString()))  //金额必须是配置的金额
                {
                    rs = orderCore.Payment(order, enPaypwd, moneyList);   //充值逻辑

                    if (rs.State == 0) //缴费、扣款成功
                    {
                        int juInt = 0;
                        if (int.TryParse(rs.Msg, out juInt))  //充值必须是int整数
                        {
                            //rs.Msg 标识实际充值金额
                            string sign = MakeMD5(Juhe.OpenID + Juhe.MobileKey + phoneno + rs.Msg + orderid);

                            string json = Juhe.Client(Juhe.MobileSite + "onlineorder?key=" + Juhe.MobileKey
                                + "&phoneno=" + phoneno
                                + "&cardnum=" + juInt
                                + "&orderid=" + orderid
                                + "&sign=" + sign
                                );
                            JuheTelorder telorder = JsonConvert.DeserializeObject<JuheTelorder>(json);


                            if (telorder.error_code == 0)
                            {
                                int pw = orderCore.UpdateByExtended(o => new Core_JuheOrder() { O_Status = (int)PayOffEnum.已销帐 },
                                o => o.O_Id == order.O_Id);

                                if (pw > 0)  //销帐状态已变更
                                {
                                    //todo:聚合缴费销帐成功 ，推送消息
                                    //Tuisong.User(userid, new MessageInfo(202, new MessageData(orderid, account, money, mode)));
                                    order.O_Status = (int)PayOffEnum.已销帐;

                                    rs.Msg = "已缴费成功";
                                    rs.Data = new ConvenientViewModel
                                    {
                                        AccountNo = order.O_Account,
                                        OrderDate = order.O_Time.ToString("yyyy-MM-dd HH:mm"),
                                        OrderNo = orderid,
                                        PayWay = "余额支付",
                                        UnitName = telorder.result.cardname
                                    };
                                    rs.State = 0;
                                    #region 消息推送

                                    JPushMsgModel jm = new JPushMsgModel()
                                    {
                                        code = (int)MessageCenterModuleEnum.邻妹妹,
                                        proFlag = (int)PushMessageEnum.默认,
                                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        tags = "手机充值",
                                        title = "手机充值缴费成功",
                                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        tips = string.Format(" 手机号：{0}，充值缴费成功，本次缴费:{1}元【温馨提示：不同的通信运营商到账时间有所延迟，请耐心等待！】", phoneno, money),
                                    };
                                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                                    #endregion
                                }
                                else
                                {

                                    rs.State = 1;
                                    order.O_Money = money;//重置金额
                                    orderCore.WriteOffFail(order, Enum.GetName(typeof(DriversEnum), platform));
                                    order.O_Status = (int)PayOffEnum.销帐失败;
                                    rs.Msg = "销帐成功，但数据失败，已通知管理员核查！";
                                }
                            }//销帐失败，退换金额到余额
                            else
                            {
                                rs.State = 1;
                                order.O_Money = money;//重置金额
                                orderCore.WriteOffFail(order, Enum.GetName(typeof(DriversEnum), platform));
                                rs.Msg = "付款成功，但销帐失败，已通知管理员核查";
                            }
                        }
                        else
                        {
                            rs.State = 1;
                            rs.Msg = "数据访问异常，请检查更新APP，谢谢！";
                        }
                    }

                }
                else
                {
                    rs.Msg = "非法访问";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// MD5加密算法
        /// </summary>
        public string MakeMD5(string Date)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Date, "MD5").ToLower();
        }
    }
}
