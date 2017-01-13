using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Com.Alipay.Mobile;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Log;

namespace JiGuangLinXin.App.App20Interface.Controllers
{

    /// <summary>
    /// 支付成功，异步通知
    /// </summary>
    public class NotifyController : Controller
    {
        public ActionResult Alipay()
        {
            LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Alipay, LogType.Weekly);
            SortedDictionary<string, string> sPara = GetRequestPost();
            //log.Write("----开始----", LogLevel.Information);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----开始----");
            if (sPara.Count > 0)//判断是否有带返回参数
            {

                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);
                //log.Write("--" + "notify_id：" + Request.Form["notify_id"] + "--" + "sign：" + Request.Form["sign"], LogLevel.Information);
                sb.AppendLine("--" + "notify_id：" + Request.Form["notify_id"] + "--" + "sign：" + Request.Form["sign"]);

                //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表
                //商户订单号                string out_trade_no = Request.Form["out_trade_no"];
                string out_trade_no = Request.Form["out_trade_no"];

                //log.Write("--" + "商户订单号 out_trade_no：" + out_trade_no, LogLevel.Information);
                sb.AppendLine("--" + "商户订单号 out_trade_no：" + out_trade_no);

                //支付宝交易号                string trade_no = Request.Form["trade_no"];
                string trade_no = Request.Form["trade_no"];
                //log.Write("--" + "支付宝交易号trade_no：" + trade_no, LogLevel.Information);
                sb.AppendLine("--" + "支付宝交易号trade_no：" + trade_no);


                //交易状态
                string trade_status = Request.Form["trade_status"];
                //log.Write("--" + "交易状态trade_status：" + trade_status, LogLevel.Information);
                sb.AppendLine("--" + "交易状态trade_status：" + trade_status);

                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    string seller_email = Request.Form["seller_email"]; //商户支付宝帐号
                    string seller_id = Request.Form["seller_id"];  //商户号
                    decimal total_fee = Convert.ToDecimal(Request.Form["total_fee"]);  //订单金额
                    if (!seller_id.Equals(Config.Partner) || !seller_email.Equals(Config.Seller_email))
                    {
                        sb.AppendLine("验证失败（商户号、或者商户支付宝帐号错误）,;返回支付宝：fail"); log.Write(sb.ToString(), LogLevel.Error);
                        return Content("fail");
                    }

                    ResultMessageViewModel result;
                    BalanceCore bCore = new BalanceCore();


                    //请在这里加上商户的业务逻辑程序代码
                    if (trade_status == "TRADE_FINISHED")
                    {
                        //log.Write(out_trade_no + "订单状态正常（TRADE_FINISHED）", LogLevel.Information);
                        sb.AppendLine("--订单状态正常（TRADE_FINISHED）");

                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        //注意：
                        //该种交易状态只在两种情况下出现
                        //1、开通了普通即时到账，买家付款成功后。
                        //2、开通了高级即时到账，从该笔交易成功时间算起，过了签约时的可退款时限（如：三个月以内可退款、一年以内可退款等）后。
                        result = bCore.RechargeAlipay(out_trade_no, total_fee, trade_no);
                    }
                    else if (trade_status == "TRADE_SUCCESS")
                    {
                        //log.Write(out_trade_no + "订单状态正常（TRADE_SUCCESS）", LogLevel.Information);
                        sb.AppendLine("--订单状态正常（TRADE_SUCCESS）");


                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        //注意：
                        //该种交易状态只在一种情况下出现——开通了高级即时到账，买家付款成功后。
                        result = bCore.RechargeAlipay(out_trade_no, total_fee, trade_no);

                    }
                    else
                    {
                        //log.Write(out_trade_no + "状态错误（notifyUrl）", LogLevel.Information);
                        sb.AppendLine("--交易状态:" + trade_status + ",处理订单失败；返回支付宝：trade_status=" + trade_status);

                        log.Write(sb.ToString(), LogLevel.Error);
                        return Content("trade_status=" + trade_status);
                    }
                    if (result != null)
                    {
                        if (result.State == 0)
                        {
                            sb.AppendLine("--恭喜，销帐成功，交易状态:" + trade_status + ";返回支付宝：success");
                            log.Write(sb.ToString(), LogLevel.Information);
                            return Content("success");
                        }
                        else
                        {
                            sb.AppendLine("--" + "更新数据库订单出错，错误描述：" + result.Msg);
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    //log.Write(out_trade_no + "验证失败（notifyUrl）", LogLevel.Error);
                    sb.AppendLine("--验证失败（notifyUrl）"); 
                }
            }
            else
            {
                //log.Write("无返回参数（notifyUrl）", LogLevel.Warning);
                sb.AppendLine("无返回参数（notifyUrl）"); 
                return Content("无通知参数");
            }
            sb.AppendLine("--销帐失败（notifyUrl），返回支付宝：fail");
            log.Write(sb.ToString(), LogLevel.Error);
            return Content("fail");
        }



        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
    }
}