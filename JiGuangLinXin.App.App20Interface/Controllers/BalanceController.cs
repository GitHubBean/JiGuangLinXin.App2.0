using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using Com.Alipay.Mobile;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.EncryptHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class BalanceController : BaseController
    {
        /// <summary>
        /// 用户帐号余额充值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [OperateLog(Desc = "账户余额充值", Flag = (int)ModuleEnum.充值)]
        public HttpResponseMessage Recharge([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            string order = obj.order;  //客户端加密
            string enMoney = obj.money;


            if (!string.IsNullOrEmpty(order)) //查询支付宝订单是否存在
            {
                order = DESProvider.DecryptString(order);

                SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                sParaTemp.Add("partner", Com.Alipay.Config.Partner);
                sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
                sParaTemp.Add("service", "single_trade_query");
                //sParaTemp.Add("trade_no", order);//使用支付宝交易号查询
                sParaTemp.Add("out_trade_no", order); //使用商户订单号查询

                string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sHtmlText);
                if (xmlDoc.DocumentElement["is_success"].InnerText == "T")
                {
                    decimal money = 0;
                    string sub = xmlDoc.DocumentElement["response"]["trade"]["subject"].InnerText;
                    string moneys = xmlDoc.DocumentElement["response"]["trade"]["total_fee"].InnerText;
                    //todo:支付宝查订单号，约定为 用户的手机号码加密码
                    if (decimal.TryParse(moneys, out money) && sub == "Lx." + phone) //支付宝账单合法
                    {
                        AlipayOrderCore aCore = new AlipayOrderCore();
                        //money = 100000;//测试金额
                        if (money != Convert.ToDecimal(DESProvider.DecryptString(enMoney)))  //账单的金额是否和客户端请求的金额一致
                        {
                            rs.State = 1;
                            rs.Msg = "无效的账单";
                        }
                        else //账单有效
                        {
                            //支付宝订单
                            Core_AlipayOrder aliOrder = new Core_AlipayOrder()
                            {
                                A_Id = Guid.NewGuid(),
                                A_Time = DateTime.Now,
                                A_Money = money,
                                A_OrderNo = order,
                                A_Phone = phone,
                                A_Remark = "余额充值",
                                A_Status = 0,
                                A_UId = uid
                            };

                            if (aCore.Pay(aliOrder))  //缴费成功
                            {
                                rs.State = 0;
                                rs.Msg = "ok";

                                #region 九宫格活动抽奖

                                //var pCore = new PrizeDetailCore();

                                //pCore.AddOne(uid, phone, 5); //添加中奖记录 

                                #endregion
                            }
                        }

                    }
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 服务端生产 支付宝 订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BuildAlipayOrder([FromBody] JObject value)
        {
            LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Alipay, LogType.Weekly);

            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            string moneyStr = obj.moneyStr;
            string nickname = obj.nickname;
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            decimal money = Convert.ToDecimal(DESProvider.DecryptString(moneyStr));
            Guid orderId = Guid.NewGuid();


            #region 生产订单

            //第一步开始
            //支付类型
            string payment_type = "1";
            //必填，不能修改
            //服务器异步通知页面路径
            //string notify_url = "http://" + HttpContext.Request.Url.Host.ToString() + "/ZwOnLine/notifyUrl";
            string notify_url = ConfigurationManager.AppSettings["notifyUrl"];
            //需http://格式的完整路径，不能加?id=123这类自定义参数
            //页面跳转同步通知页面路径
            //需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/
            //卖家支付宝帐户
            string seller_id = Config.Seller_email;
            //必填
            //商户订单号
            string out_trade_no = orderId.ToString("N").ToUpper();
            //商户网站订单系统中唯一订单号，必填
            //订单名称
            string subject = ConfigurationManager.AppSettings["alipaySubject"];//"极光邻信APP余额充值";

            //必填
            //付款金额
            //string total_fee = "0.1";
            string total_fee = money.ToString("N");
            //必填
            //订单描述
            string body = string.Format("用户：{0}于（{1}）在线支付宝支付，余额充值{2}元。", nickname, DateTime.Now.ToString(), total_fee);
            //防钓鱼时间戳
            //string anti_phishing_key = "";
            //若要使用请调用类文件submit中的query_timestamp函数
            //超时时间需要动态计算，并且减去2，如果超时时间小于2是否让继续支付。 


            ////////////////////////////////////////////////////////////////////////////////////////////////
            string signStr = "partner=\"" + Config.Partner + "\"&seller_id=\"" + seller_id + "\"&out_trade_no=\"" +
                             out_trade_no + "\"&subject=\"" + subject + "\"&body=\"" + body + "\"&total_fee=\"" +
                             total_fee + "\"&notify_url=\"" + notify_url +
                             "\"&service=\"mobile.securitypay.pay\"&payment_type=\"" + payment_type +
                             "\"&_input_charset=\"utf-8\"&it_b_pay=\"30m\"&return_url=\"m.alipay.com\"";

            string returnStr = signStr + "&sign=\"" +
                               HttpUtility.UrlEncode(
                                   RSAFromPkcs8.sign(signStr, Config.Private_key, Config.Input_charset),
                                   System.Text.Encoding.UTF8) + "\"&sign_type=\"RSA\"";

            //添加支付宝订单
            Core_AlipayOrder order = new Core_AlipayOrder()
            {
                A_Id = Guid.NewGuid(),
                A_OrderNo = out_trade_no,
                A_UId = uid,
                A_Phone = phone,
                A_Money = Convert.ToDecimal(total_fee),
                A_Status = 0,  //0未支付 1已支付
                A_Time = DateTime.Now
            };
            AlipayOrderCore core = new AlipayOrderCore();

            log.Write("----生成订单:参数----" + returnStr, LogLevel.Information);
            #endregion

            if (core.AddEntity(order) != null)
            {
                rs.Data = new
                {
                    returnStr,
                    orderNo = DESProvider.EncryptString(out_trade_no)
                };
            }

            return WebApiJsonResult.ToJson(rs);
        }
    }
}
