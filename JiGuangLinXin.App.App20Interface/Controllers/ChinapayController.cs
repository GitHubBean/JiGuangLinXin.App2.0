using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 银联便民服务接口
    /// </summary>
    public class ChinapayController : BaseController
    {

        private ChinapayDeptCore deptCore = new ChinapayDeptCore();
        private ChinapayOrderCore coCore = new ChinapayOrderCore();
        private CityCore cityCore = new CityCore();
        private BalanceCore bCore = new BalanceCore();
        private Encoding Encode = Encoding.ASCII;
        private string branchId = ConfigurationManager.AppSettings["branchId"];
        private string interfaceUrl = ConfigurationManager.AppSettings["interfaceUrl"];
        /// <summary>
        /// 测试一把
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetIndex()
        {
            return WebApiJsonResult.ToJson(new { name = "abc" });
        }

        /// <summary>
        /// 客户端选择缴费城市以及缴费项目，获取该城市该项目下的所有缴费机构
        /// </summary>
        /// <param name="cityName">客户端选择缴费城市以及缴费项目，获取该城市该项目下的所有缴费机构</param>
        /// <param name="proId">缴费项目(1水费、2电费、3燃气费)</param>
        /// <returns></returns>
        public HttpResponseMessage BranchQuery([FromBody]JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string cityName = obj.cityName;
            int proId = obj.proId;

            string proName = Enum.GetName(typeof(PaymentTypeEnum), proId);
            var list = deptCore.LoadEntities(o => o.D_CityName == cityName && o.D_ProjectName == proName).ToList().Select(s => new { id = s.D_MerSysId, name = s.D_UnitName });
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list;
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂未开通,敬请期待";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 查询便民订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [OperateLog(Desc = "查询便民订单", Flag = (int)ModuleEnum.银联便民)]
        public HttpResponseMessage BillQuery([FromBody]JObject value)
        {
            ResultViewModel rs = new ResultViewModel(1, "账单查询失败", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            dynamic obj = value;
            string queryNo = obj.queryNo;
            string merSysId = obj.merSysId;

            /**正式开整**/
            var dept = deptCore.LoadEntity(o => o.D_MerSysId == merSysId);

            if (!string.IsNullOrEmpty(queryNo) && dept != null)
            {
                string billType = dept.D_BillType;
                string merBillStat = "00";//未缴费
                string billDate = "";

                if (dept.D_BillDate == 1) //要求输入账期
                {
                    billDate = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                }
                string password = "";
                string individualArea = dept.D_IndividualAreaQuery;
                string writeOffType = "01";//销账类型主要区别是充值交易还是销账交易 00:充值,01:销账


                //生成报文（查询订单）
                string strxml = string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><workflows xmlns=\"http://convenience.chinapay.com/schema/outerinterface\"><queryRequest><branchId>{0}</branchId><merSysId>{1}</merSysId><queryNo>{2}</queryNo><billType>{3}</billType><merBillStat>{4}</merBillStat><billDate>{5}</billDate><password>{6}</password><individualArea>{7}</individualArea><writeOffType>{8}</writeOffType><resv1></resv1><resv2></resv2><resv3></resv3><resv4></resv4></queryRequest></workflows>", branchId, merSysId, queryNo, billType, merBillStat, billDate, password, individualArea, writeOffType);
                byte[] b_data = Encode.GetBytes(strxml);
                MACVerify_ANSIX99 macverify = new MACVerify_ANSIX99();
                //报文MAC校验码
                string pass = macverify.GetMAC_String16(b_data, true);
                //  SendMsg(pass.ToLower() + strxml, "http://140.206.112.250/SCWeb1/outerInterface", true);
                string resultXml = RequestChinapayInterface(pass.ToLower() + strxml, interfaceUrl);
                if (!string.IsNullOrEmpty(resultXml))  //返回成功
                {
                    Core_ChinapayOrder order = XmlToOrderQuery(resultXml);

                    order.D_Id = Guid.NewGuid();
                    order.A_MerSysId = merSysId;
                    order.A_billDate = billDate;
                    order.A_billType = billType;
                    order.A_password = password;
                    order.A_individualArea = individualArea;
                    order.A_merBillStat = merBillStat;
                    order.A_queryNo = queryNo;
                    order.A_writeOffType = writeOffType;
                    order.D_DeptId = dept.D_Id;
                    order.D_DeptName = dept.D_UnitName;
                    order.D_ProjectName = dept.D_ProjectName;
                    order.D_UserId = uid;
                    order.D_UserPhone = phone;
                    order.D_Time = DateTime.Now;


                    if (dept.D_IndividualAreaOrderFlag == 0)  //订单 扩展字段是否 用查询的返回原样请求：0否 1是
                    {
                        order.B_individualArea = order.A_individualArea;
                    }

                    string enOrder = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(order));
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        billAmt = DESProvider.EncryptString(order.B_billAmt),
                        billOrderNo = order.B_billNo,
                        //billDate = order.B_billDate,
                        billDate = DateTime.Now.ToShortDateString(),
                        cpToken = enOrder
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 确认下单缴费
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Payment([FromBody]JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            dynamic obj = value;
            string cpToken = obj.cpToken;
            string enPaypwd = obj.payPwd;  //必须输入支付密码

            if (!string.IsNullOrEmpty(cpToken))
            {
                string enOrder = Md5Extensions.MD5Decrypt(cpToken);
                Core_ChinapayOrder order = JsonSerialize.Instance.JsonToObject<Core_ChinapayOrder>(enOrder);
                if (order != null)
                {

                    Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
                    string phone = Request.Headers.GetValues("phone").FirstOrDefault();
                    var ban = bCore.LoadEntity(o => o.B_AccountId == uid);
                    if (ban == null)
                    {
                        rs.Msg = "用户帐号不存在";

                        return WebApiJsonResult.ToJson(rs);
                    }

                    if (string.IsNullOrEmpty(ban.B_PayPwd) || string.IsNullOrEmpty(ban.B_EncryptCode))
                    {
                        rs.State = 2;
                        rs.Msg = "您还未设置支付密码,请立即前去设置！";

                        return WebApiJsonResult.ToJson(rs);
                    }
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + ban.B_EncryptCode);// 加密支付密码
                    if (!ban.B_PayPwd.Equals(payPwd))
                    {
                        rs.Msg = "支付密码错误！";

                        return WebApiJsonResult.ToJson(rs);
                    }

                    if (ban.B_Balance < Convert.ToDecimal(order.B_billAmt))
                    {
                        rs.Msg = "余额不足";
                        return WebApiJsonResult.ToJson(rs);
                    }

                    order.B_reqSeqId = GenerateCheckCode(8); //请求方交易流水号
                    order.B_reqTransDate = DateTime.Now.ToString("yyyyMMdd");//请求方交易日期(yyyyMMdd)
                    order.B_userId = branchId;
                    order.B_writeOffType = "01";


                    string strxml = string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><workflows xmlns=\"http://convenience.chinapay.com/schema/outerinterface\"><orderRequest><reqSeqId>{0}</reqSeqId><reqTransDate>{1}</reqTransDate><branchId>{2}</branchId><userId>{3}</userId><billList><bill><merSysId>{4}</merSysId><writeOffType>{5}</writeOffType><billNo>{6}</billNo><billAmt>{7}</billAmt><password>{8}</password><billDate>{9}</billDate><billType>{10}</billType><recordTimes>{11}</recordTimes><individualArea>{12}</individualArea></bill></billList><resv1></resv1><resv2></resv2><resv3></resv3><resv4></resv4></orderRequest></workflows>", order.B_reqSeqId, order.B_reqTransDate, branchId, order.B_userId, order.A_MerSysId, order.B_writeOffType, order.B_billNo, order.B_billAmt, order.A_password, order.B_billDate, order.A_billType, order.B_recordTimes, order.B_individualArea);
                    byte[] b_data = Encode.GetBytes(strxml);
                    MACVerify_ANSIX99 macverify = new MACVerify_ANSIX99();
                    //报文MAC校验码
                    string pass = macverify.GetMAC_String16(b_data, true);

                    string resultXml = RequestChinapayInterface(pass.ToLower() + strxml, interfaceUrl);

                    if (!string.IsNullOrEmpty(resultXml))  //1.成功下单并返回
                    {
                        XmlToOrderSendOrder(resultXml, ref order);

                        // enOrder = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(order));  //加密
                        //rs.State = 0;
                        //rs.Msg = "ok";
                        //rs.Data = new { cpToken = enOrder };


                        //2.通知服务端销帐；调用银联接口，支付订单
                        var orRs = coCore.ChinapayOrder(order);
                        if (orRs.State == 0)
                        {

                            //todo:这里是测试数据，正式环境请取消注释
                            // phone = "15825942359";// GetValueByHeader("phone"); 
                            // uid = "AF854528-D063-47DC-A5E6-DDBC5725389D";//GetValueByHeader("uid");



                            #region 支付成功，销帐

                            order.C_CuryId = "156";
                            string TransAmt = "000000000000";
                            if (order.C_TransAmt.Length != 12)
                                TransAmt = TransAmt.Substring(0, 12 - order.C_TransAmt.Length) + order.C_TransAmt;
                            else
                                TransAmt = order.C_TransAmt;

                            order.C_TransAmt = TransAmt;
                            order.C_RespCode = "00";
                            order.C_BillInfo = "";
                            order.C_BankDate = order.C_resv1.Substring(6, 8);
                            order.C_BankSeq = order.C_resv1.Substring(0, 6);
                            order.C_Resv = "";
                            string MD5Key = ConfigurationManager.AppSettings["md5Key"];
                            string ChkValue = order.C_ordDate + order.C_ordId + order.C_CuryId + TransAmt + order.C_RespCode + order.C_billNo + order.C_BillInfo + order.C_BankDate + order.C_BankSeq + order.C_Resv + MD5Key;
                            string key = MD5.Encrypt(ChkValue, 32);
                            System.Net.WebClient web = new System.Net.WebClient();
                            string PostUrl = string.Format("http://payment-test.chinapay.com/pay/SCWriteOff.jsp?TransDate={0}&OrdId={1}&CuryId={2}&TransAmt={3}&RespCode={4}&BillNo={5}&BillInfo={6}&BankDate={7}&BankSeq={8}&Resv={9}&ChkValue={10}", order.C_ordDate, order.C_ordId, order.C_CuryId, TransAmt, order.C_RespCode, order.C_billNo, order.C_BillInfo, order.C_BankDate, order.C_BankSeq, order.C_Resv, key);
                            string result = web.DownloadString(PostUrl);

                            if ("ok".Equals(StringExt.removeHtml(result), StringComparison.OrdinalIgnoreCase))  //银联销帐成功
                            {
                                //todo:银联销帐成功后，可以再次请求银联查询订单接口，更新本地服务器的银联订单状态；目前是硬编码 默认 订单的状态

                                order.D_billDate = DateTime.Now.ToString();
                                order.D_ordStat = "000";  //000：支付成功；002：支付失败；111：未支付221：待退款；222：已退款；991：退款失败
                                order.D_scBillStat = "1";//便民平台账单状态0：未支付1：销账成功2：销账失败3：销账未知





                                rs.Msg = "已缴费成功";
                                rs.Data = new ConvenientViewModel
                                {
                                    AccountNo = order.C_ordId,
                                    OrderDate = order.D_Time.ToString("yyyy-MM-dd HH:mm"),
                                    OrderNo = order.A_queryNo,
                                    PayWay = "余额支付",
                                    UnitName = order.D_DeptName
                                };
                                rs.State = 0;
                            }
                            else  //银联销帐失败，提醒管理员手动销帐
                            {

                                order.D_billDate = DateTime.Now.ToString();
                                order.D_ordStat = "221";  //000：支付成功；002：支付失败；111：未支付221：待退款；222：已退款；991：退款失败
                                order.D_scBillStat = "3";//便民平台账单状态0：未支付1：销账成功2：销账失败3：销账未知



                                Core_Feedback fb = new Core_Feedback()
                                {
                                    F_Content =
                                        string.Format("银联接口便民，支付成功，银联销帐失败。#流水号订单号：{0},日期：{1},支付金额:{2}，应缴金额{3}#",
                                            order.C_ordId, order.C_billDate, order.B_billAmt, order.B_billAmt),
                                    F_Flag = (int)FeedbackEnum.系统,
                                    F_Phone = phone,
                                    F_Status = 0,
                                    F_Time = DateTime.Now,
                                    F_Title = "银联便民缴费，支付成功但销帐失败",
                                    F_UId = uid
                                };
                                new FeedbackCore().AddEntity(fb);



                                rs.State = 1;
                                rs.Data = result;
                                rs.Msg = "用户支付成功，但银联销帐失败";
                            }
                            #endregion

                            coCore.UpdateEntity(order);  //更新银联的便民订单
                        }
                        else  //下单失败
                        {
                            return WebApiJsonResult.ToJson(orRs);
                        }

                    }
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }


        #region 便民缴费 在线支付  作废的方法

        /// <summary>
        /// 会员确定下单
        /// </summary>
        /// <param name="value">银联约定内容</param>
        /// <returns></returns>
        [OperateLog(Desc = "确认便民下单", Flag = (int)ModuleEnum.银联便民)]
        public HttpResponseMessage BillSendOrder([FromBody] JObject value)
        {
            dynamic obj = value;
            string cpToken = obj.cpToken;
            ResultViewModel rs = new ResultViewModel(1, "便民下单失败", null);
            if (!string.IsNullOrEmpty(cpToken))
            {
                string enOrder = Md5Extensions.MD5Decrypt(cpToken);
                Core_ChinapayOrder order = JsonSerialize.Instance.JsonToObject<Core_ChinapayOrder>(enOrder);
                if (order != null)
                {
                    order.B_reqSeqId = GenerateCheckCode(8); //请求方交易流水号
                    order.B_reqTransDate = DateTime.Now.ToString("yyyyMMdd");//请求方交易日期(yyyyMMdd)
                    order.B_userId = branchId;
                    order.B_writeOffType = "01";


                    string strxml = string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><workflows xmlns=\"http://convenience.chinapay.com/schema/outerinterface\"><orderRequest><reqSeqId>{0}</reqSeqId><reqTransDate>{1}</reqTransDate><branchId>{2}</branchId><userId>{3}</userId><billList><bill><merSysId>{4}</merSysId><writeOffType>{5}</writeOffType><billNo>{6}</billNo><billAmt>{7}</billAmt><password>{8}</password><billDate>{9}</billDate><billType>{10}</billType><recordTimes>{11}</recordTimes><individualArea>{12}</individualArea></bill></billList><resv1></resv1><resv2></resv2><resv3></resv3><resv4></resv4></orderRequest></workflows>", order.B_reqSeqId, order.B_reqTransDate, branchId, order.B_userId, order.A_MerSysId, order.B_writeOffType, order.B_billNo, order.B_billAmt, order.A_password, order.B_billDate, order.A_billType, order.B_recordTimes, order.B_individualArea);
                    byte[] b_data = Encode.GetBytes(strxml);
                    MACVerify_ANSIX99 macverify = new MACVerify_ANSIX99();
                    //报文MAC校验码
                    string pass = macverify.GetMAC_String16(b_data, true);

                    string resultXml = RequestChinapayInterface(pass.ToLower() + strxml, interfaceUrl);

                    if (!string.IsNullOrEmpty(resultXml))  //返回成功
                    {
                        XmlToOrderSendOrder(resultXml, ref order);

                        enOrder = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(order));  //加密
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new { cpToken = enOrder };
                    }
                }

            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 客户端 会员支付成功，通知服务端销帐
        /// </summary>
        /// <param name="value">个中参数</param>
        /// <returns></returns>
        [OperateLog(Desc = "便民销帐", Flag = (int)ModuleEnum.银联便民)]
        public HttpResponseMessage BillWriteOff([FromBody]JObject value)
        {
            ResultViewModel rs = new ResultViewModel(1, "销帐失败", null);
            dynamic obj = value;
            string order = obj.alipayOrder;
            //todo:这里是测试数据，正式环境请取消注释
            string phone = "15825942359";// GetValueByHeader("phone"); 
            string uid = "AF854528-D063-47DC-A5E6-DDBC5725389D";//GetValueByHeader("uid");


            if (!string.IsNullOrEmpty(order))  //查询支付宝订单是否存在
            {

                SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                sParaTemp.Add("partner", Com.Alipay.Config.Partner);
                sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
                sParaTemp.Add("service", "single_trade_query");
                //sParaTemp.Add("trade_no", order);//使用支付宝交易号查询
                sParaTemp.Add("out_trade_no", order);//使用商户订单号查询

                string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sHtmlText);
                if (xmlDoc.DocumentElement["is_success"].InnerText == "T")
                {
                    decimal money = 0;
                    string sub = xmlDoc.DocumentElement["response"]["trade"]["subject"].InnerText;
                    string moneys = xmlDoc.DocumentElement["response"]["trade"]["total_fee"].InnerText;
                    //todo:支付宝查订单 校验订单号 (sub == ("Lx." + phone)) &&
                    if (decimal.TryParse(moneys, out money))//支付宝账单金额
                    {
                        AlipayOrderCore aCore = new AlipayOrderCore();
                        //获得便民接口的订单数据
                        string enstr = obj.cpToken;
                        string enOrder = Md5Extensions.MD5Decrypt(enstr);
                        Core_ChinapayOrder od = JsonSerialize.Instance.JsonToObject<Core_ChinapayOrder>(enOrder);
                        //money = 100000;//测试金额
                        if (money < Convert.ToDecimal(od.B_billAmt))
                        {
                            rs.State = 1;
                            rs.Msg = "支付的金额小于便民缴费的销帐金额";
                            return WebApiJsonResult.ToJson(rs);
                        }

                        Core_AlipayOrder aliOrder = new Core_AlipayOrder()
                        {
                            A_Id = Guid.NewGuid(),
                            A_Time = DateTime.Now,
                            A_Money = money,
                            A_OrderNo = order,
                            A_Phone = phone,
                            A_Remark = "便民缴费",
                            A_Status = 1,  //未销帐，正式销帐后，改状态值
                            A_UId = Guid.Parse(uid)

                        };
                        int rsInt = aCore.ChinapayAlipayOrder(aliOrder); //添加订单结果

                        if (rsInt == 2)
                        {
                            rs.State = 1;
                            rs.Msg = "账单已销帐";
                        }
                        else //订单入库成功,请求银联的下单销帐接口
                        {
                            od.C_CuryId = "156";
                            string TransAmt = "000000000000";
                            if (od.C_TransAmt.Length != 12)
                                TransAmt = TransAmt.Substring(0, 12 - od.C_TransAmt.Length) + od.C_TransAmt;
                            else
                                TransAmt = od.C_TransAmt;

                            od.C_TransAmt = TransAmt;
                            od.C_RespCode = "00";
                            od.C_BillInfo = "";
                            od.C_BankDate = od.C_resv1.Substring(6, 8);
                            od.C_BankSeq = od.C_resv1.Substring(0, 6);
                            od.C_Resv = "";
                            string MD5Key = ConfigurationManager.AppSettings["md5Key"];
                            string ChkValue = od.C_ordDate + od.C_ordId + od.C_CuryId + TransAmt + od.C_RespCode + od.C_billNo + od.C_BillInfo + od.C_BankDate + od.C_BankSeq + od.C_Resv + MD5Key;
                            string key = MD5.Encrypt(ChkValue, 32);
                            System.Net.WebClient web = new System.Net.WebClient();
                            string PostUrl = string.Format("http://payment-test.chinapay.com/pay/SCWriteOff.jsp?TransDate={0}&OrdId={1}&CuryId={2}&TransAmt={3}&RespCode={4}&BillNo={5}&BillInfo={6}&BankDate={7}&BankSeq={8}&Resv={9}&ChkValue={10}", od.C_ordDate, od.C_ordId, od.C_CuryId, TransAmt, od.C_RespCode, od.C_billNo, od.C_BillInfo, od.C_BankDate, od.C_BankSeq, od.C_Resv, key);
                            string result = web.DownloadString(PostUrl);

                            if ("ok".Equals(StringExt.removeHtml(result), StringComparison.OrdinalIgnoreCase))  //销帐成功
                            {
                                //银联销帐成功：1变更缴费的支付宝订单的记录状态为销帐 2添加银联销帐记录 3如果支付宝支付的金额大于销帐的金额，多余的钱存入余额
                                od.D_billDate = DateTime.Now.ToString();
                                od.D_ordStat = "000";  //000：支付成功；002：支付失败；111：未支付221：待退款；222：已退款；991：退款失败
                                od.D_scBillStat = "1";//便民平台账单状态0：未支付1：销账成功2：销账失败3：销账未知

                                var billOff = new ChinapayOrderCore().OrderWriteoff(order, od);
                                if (billOff == 0)
                                {
                                    rs.State = 0;
                                    //todo:具体销帐成功，返回的内容，以业务（界面）再定夺  
                                    rs.Msg = "销帐成功";
                                }
                                else
                                {
                                    rs.State = 1;
                                    rs.Msg = "销帐成功,但更新服务数据异常";
                                }

                                ////销帐成功，1更改支付宝订单的状态为已销帐
                                //var offOrder = aCore.LoadEntity(o => o.A_OrderNo == order);
                                //offOrder.A_Status = 0;//已经销帐
                                //aCore.UpdateEntity(offOrder);
                                ////销帐成功，2添加银联缴费订单
                                //od.D_Id = Guid.NewGuid();
                                //od.D_PayBillId = offOrder.A_Id.ToString(); //设置销帐的支付宝帐号id
                                //ChinapayOrderCore cpOrderCore = new ChinapayOrderCore();
                                //cpOrderCore.AddEntity(od);

                            }
                            else  //销帐失败，提醒管理员手动销帐
                            {

                                Core_Feedback fb = new Core_Feedback() { F_Content = string.Format("银联接口便民，费用支付成功，银联销帐失败。#订单号：{0},日期：{1},支付金额:{2}，应缴金额{3}#", od.C_ordId, od.C_billDate, money, od.B_billAmt), F_Flag = (int)FeedbackEnum.系统, F_Phone = phone, F_Status = 0, F_Time = DateTime.Now, F_Title = "银联便民缴费，支付成功但销帐失败", F_UId = Guid.Parse(uid) };
                                new FeedbackCore().AddEntity(fb);


                                rs.State = 1;
                                rs.Data = result;
                                rs.Msg = "会员支付成功，但银联销帐失败";
                            }
                        }
                    }
                    else
                    {
                        rs.State = 1;
                        rs.Msg = "未能支付成功";
                    }

                }
                else
                {
                    rs.State = 1;
                    rs.Msg = "未能查询到对应支付宝账单";
                    // info = new ReturnInfo(2, "未能查询到对应支付宝账单");
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        #endregion
        /// <summary>
        /// 请求银联的便民接口
        /// </summary>
        /// <param name="message">请求报文</param>
        /// <param name="url">银联接口地址</param>
        /// <returns>响应报文</returns>
        private string RequestChinapayInterface(string message, string url)
        {
            string rs = "";
            try
            {
                string formUrl = url;
                string formData = JiGuangLinXin.App.Services.Base64.EncodeBase64(Encoding.UTF8, message);//提交的参数
                //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
                byte[] postData = Encoding.UTF8.GetBytes(formData);
                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(formUrl) as HttpWebRequest;
                Encoding myEncoding = Encoding.UTF8;
                request.Method = "POST";
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded;charset=GBK";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
                request.ContentLength = postData.Length;
                // 提交请求数据 
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();
                HttpWebResponse response;
                Stream responseStream;
                StreamReader reader;
                string srcString;
                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                string result = srcString;   //返回值赋值
                reader.Close();
                if (!string.IsNullOrEmpty(result))
                {
                    rs = JiGuangLinXin.App.Services.Base64.DecodeBase64("UTF-8", result);
                }
            }
            catch (Exception ex)
            {
                LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + "chinapay", LogType.Daily);
                log.Write(ex.StackTrace, LogLevel.Error);
            }
            return rs;
        }
        /// <summary>
        /// 查询请求返回xml报文，解析xml数据
        /// </summary>
        /// <param name="strxml">查询返回报文</param>
        /// <returns>chinapay 订单实体</returns>
        private Core_ChinapayOrder XmlToOrderQuery(string strxml)
        {
            Core_ChinapayOrder obj = new Core_ChinapayOrder();
            XmlDocument xmldoc = new XmlDocument();
            string NewXml = strxml.Substring(16, strxml.Length - 16).Trim().Replace("xmlns=\"http://convenience.chinapay.com/schema/outerinterface\"", "");
            xmldoc.LoadXml(NewXml);
            XmlNode xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                if (xxNode2.Name == "responseCode")
                {
                    obj.B_responseCode = xxNode2.InnerText;

                }
                if (xxNode2.Name == "responseDesc")
                {
                    obj.B_responseDesc = xxNode2.InnerText;
                }
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/billNo");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_billNo = xxNode2.InnerText;
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/billAmt");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_billAmt = xxNode2.InnerText;
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/billDate");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_billDate = xxNode2.InnerText;
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/merBillStat");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_merBillStat = xxNode2.InnerText;
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/recordTimes");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_recordTimes = xxNode2.InnerText;
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/queryResponse[1]/billList/bill[1]/individualArea");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                obj.B_individualArea = xxNode2.InnerText;
            }
            return obj;
        }
        /// <summary>
        /// 会员确定下单
        /// </summary>
        /// <param name="strxml">银联接口，申请下单的请求报文</param>
        /// <returns></returns>
        private void XmlToOrderSendOrder(string strxml, ref Core_ChinapayOrder obj)
        {
            //Core_ChinapayOrder obj = new Core_ChinapayOrder();
            XmlDocument xmldoc = new XmlDocument();
            string NewXml = strxml.Substring(16, strxml.Length - 16).Trim().Replace("xmlns=\"http://convenience.chinapay.com/schema/outerinterface\"", "");
            xmldoc.LoadXml(NewXml);
            XmlNode xxNode = xmldoc.SelectSingleNode("/workflows/orderResponse[1]");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                if (xxNode2.Name == "responseCode")
                {
                    obj.C_responseCode = xxNode2.InnerText;
                }
                if (xxNode2.Name == "responseDesc")
                {
                    obj.C_responseDesc = xxNode2.InnerText;
                }
                if (xxNode2.Name == "reqSeqId")
                {
                    obj.C_reqSeqId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "reqTransDate")
                {
                    obj.C_reqTransDate = xxNode2.InnerText;
                }
                if (xxNode2.Name == "branchId")
                {
                    obj.C_branchId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "resSeqId")
                {
                    obj.C_resSeqId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "resTransDate")
                {
                    obj.C_resTransDate = xxNode2.InnerText;
                }
                if (xxNode2.Name == "ordId")
                {
                    obj.C_ordId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "ordDate")
                {
                    obj.C_ordDate = xxNode2.InnerText;
                }
                if (xxNode2.Name == "ordStat")
                {
                    obj.C_ordStat = xxNode2.InnerText;
                }
                if (xxNode2.Name == "userId")
                {
                    obj.C_userId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "resv1")
                {
                    obj.C_resv1 = xxNode2.InnerText;
                }
            }
            xxNode = xmldoc.SelectSingleNode("/workflows/orderResponse[1]/billList[1]/bill");
            foreach (XmlNode xxNode2 in xxNode.ChildNodes)
            {
                if (xxNode2.Name == "merSysId")
                {
                    obj.C_merSysId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "merId")
                {
                    obj.C_merId = xxNode2.InnerText;
                }
                if (xxNode2.Name == "billNo")
                {
                    obj.C_billNo = xxNode2.InnerText;
                }
                if (xxNode2.Name == "billAmt")
                {
                    obj.C_TransAmt = xxNode2.InnerText;
                }
                if (xxNode2.Name == "billDate")
                {
                    obj.C_billDate = xxNode2.InnerText;
                }
                if (xxNode2.Name == "merAccount")
                {
                    obj.C_merAccount = xxNode2.InnerText;
                }
                if (xxNode2.Name == "scBillStat")
                {
                    obj.C_scBillStat = xxNode2.InnerText;
                }
            }
        }



        public static int rep = 0;
        private static string GenerateCheckCode(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }




    }
}
