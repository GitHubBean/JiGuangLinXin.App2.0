using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ServicesModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using WebGrease.Css.Extensions;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class HomeController : Controller
    {
        private ChinapayDeptCore cpCore = new ChinapayDeptCore();
        private BusinessCore bCore = new BusinessCore();
        CookieContainer cookie = new CookieContainer();

        private string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public string MakeMD5(string Date)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Date, "MD5").ToLower();
        }

        public ActionResult Index(string id, string uid = "", int flag = 0)
        {

            return Content("adf");
            //string dsk = JsonSerialize.Instance.JsonToObject<List<Dictionary<string, string>>>(ConfigurationManager.AppSettings["EventIdList"])[1]["value"];
            //return Content(dsk + "000");

            string phone = "";//17723040574 13983379868,18166434341

            string orderid = phone + DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                         new Random(Guid.NewGuid().GetHashCode()).Next(0, 100); // Guid.NewGuid().ToString("N");//订单号 
            string sign0 = MakeMD5(Juhe.OpenID + Juhe.MobileKey + phone + 50 + orderid);

            string json0 = Juhe.Client(Juhe.MobileSite + "onlineorder?key=" + Juhe.MobileKey
                + "&phoneno=" + orderid
                + "&cardnum=" + 50
                + "&orderid=" + orderid
                + "&sign=" + sign0
                );


            return Content(json0 + "\n" + orderid);


            //string item = "35";
            string orderId = "";

            int[] plans = new int[] { 4,4,4 };
            string json = "";
            foreach (var item in plans)
            {
                string o = phone + DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                             new Random(Guid.NewGuid().GetHashCode()).Next(0, 100); // Guid.NewGuid().ToString("N");//订单号 
                string sign =
                         Juhe.MakeMD5(Juhe.OpenID + Juhe.TrafficKey + phone + item + o);
                orderId += o + ",";
                json += Juhe.Client(Juhe.TrafficSite + "?key=" + Juhe.TrafficKey
                                          + "&phone=" + phone
                                          + "&pid=" + item
                                          + "&orderid=" + o
                                          + "&sign=" + sign
                    ) + "\n";

            }
            return Content(json + "\n" + orderId);

            Juhe.Update("D:\\juheDept2016-06-15.txt");

            return Content(Md5Extensions.MD5Encrypt("lx@123888"));

            //return Content("OK");
            //using (LinXinApp20Entities db = new LinXinApp20Entities())
            //{
            //    string rsl = "";
            //    Guid gid = Guid.Parse("43E7713D-6022-4B2B-84A0-B844BC263D1B");
            //    var li = from tt in db.Core_Topic
            //             join uu in db.Core_User on tt.T_UserId equals uu.U_Id
            //             where tt.T_UserId == gid
            //             select new
            //                 {
            //                     t = tt.T_Title,
            //                     u = uu.U_NickName
            //                 };

            //    li.ForEach(o => rsl += string.Format("title:{0},nickname:{1}", o.t, o.u));

            //    return Content(rsl);
            //}


            string toId = "";
            if (!string.IsNullOrEmpty(uid))
            {
                toId = Guid.Parse(uid).ToString("N").ToLower();
            }
            else
            {
                return Content("接收人 uid 未提供");
            }

            //var idsss =
            //    new UserCore().LoadEntities(
            //        p =>
            //            p.U_ChatID == "lx_a4f86b43-f6ef-4867-a961-532dceb016b6" ||
            //            p.U_ChatID == "lx_4a9c46cd-72dd-4c0f-8cc8-25a1c083e051").Select(o => o.U_Id.ToString()).ToArray();

            JPushMsgModel jj = new JPushMsgModel()
            {
                //code = (int)MessageCenterModuleEnum.邻妹妹,
                code = Convert.ToInt32(id),
                proFlag = (int)PushMessageEnum.社区活动跳转,
                proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tags = DateTime.Now.Second + "招聘小区管理员" + DateTime.Now,
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tips = DateTime.Now.Second + "欢迎您入驻自家小区，邻信管理员招聘火热进行中，快来参加吧" + DateTime.Now,
                title = DateTime.Now.Second + "邻信管理员招聘火热进行中，快来参加吧" + DateTime.Now,
                logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                proName = DateTime.Now.Second + "社区小管家招聘" + DateTime.Now
            };
            Tuisong.PushMessage((int)PushPlatformEnum.Alias, jj.title, jj.title, JsonSerialize.Instance.ObjectToJson(jj), toId);


            return Content("ok");

            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"],
                ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"],
                ConfigurationManager.AppSettings["HxOrgName"]);
            //    string code = ease.QueryChatMessage();


            //string sstr =
            //    DESProvider.DecryptString(
            //        "U8XP8gVOm+q3zMtR4iKC4PRSpTSSQz023H7uXq0DTA4MJbiBpi0fB3ysG2jH9SeUiTn8P+MW5/p8DUaZTp18XCObSXnurAW6X8JJaXmiWyU0kj8KQuQyNaxUulnKVcZPuS/BHV/U2F0M4gSgSTscYKvigFSjZYuRlv32MtrJXbM9PakwNTkiQXIn8PB3tmUVh6B1FRRVWKjKJjp9Cp8tzQ71pcGSz3hJ9W37JKxjfj/LDkauyq7a1mzapIcxEoa2IlvK0mE7Eomvw6sgru2mvuss/rgyDwAjcgCjz+f4Q35dOzdw6dB8vKgsjb7mWL8J580/27Q3MKJvgbvNZxHYThOVWE1hbm7sWSSqTc5U8JGavY0VHVGbgV4259UkXy5l39rDbD4Dglx0gXLscCwQKXLJB0i5+rTUjKmrQzZgSjPourGsqyl9KMQL+fOnGIV8JCFeEg2oBU4oAu9iNQpjTq1doJAXFcyg3UDsReMK4KvsrxNvN42jz5pMXPNgqavzAMmKOeIh6/x3SgVha+BYDZqDIhlCWnc1");



            //JPushMsgModel vm = new JPushMsgModel()
            //{
            //    code = 1006,
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
            //    nickname = "【精品汇】水果王",
            //    proFlag = 4,
            //    proId = "48081354-D3B8-4019-8C3B-0AE9ECD2A728",
            //    proName = "海南香菠萝",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "精品汇",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "精品汇推送的默认不跳，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
            //    uid = "d675a17563c14f1e96598edecf08c137",
            //    title = "精品汇推送" + DateTime.Now.ToString(),

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm), toId);




            //JPushMsgModel vm1 = new JPushMsgModel()
            //{
            //    code = 1006,
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
            //    nickname = "【社区服务】水果王",
            //    proFlag = 3,
            //    proId = "",
            //    proName = "",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "社区服务",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "社区服务推送的默认不跳，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
            //    uid = "CACBA69B-214F-4B6F-B6AA-1EC2AB29C85D",
            //    title = "社区服务推送" + DateTime.Now.ToString(),

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm1), toId);




            //JPushMsgModel vm2 = new JPushMsgModel()
            //{
            //    code = 1006,
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
            //    nickname = "【好友申请-木易】",
            //    proFlag = 1,
            //    proId = "",
            //    proName = "",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "好友申请",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "社区服务推送的默认不跳，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now+"打瞌睡",
            //    uid = "6E5E8BA2-6A85-4388-9D76-5E7E5E7C2376",
            //    title = "社区服务推送" + DateTime.Now.ToString(),

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm2), toId);


            //JPushMsgModel vm2 = new JPushMsgModel()
            //{
            //    code = 1006,
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
            //    nickname = "【好友申请-木易】",
            //    proFlag = 1,
            //    proId = "",
            //    proName = "",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "好友申请",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "社区服务推送的默认不跳，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now + "打瞌睡",
            //    uid = "6E5E8BA2-6A85-4388-9D76-5E7E5E7C2376",
            //    title = "社区服务推送" + DateTime.Now.ToString(),

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm2), toId);




            //JPushMsgModel vm3 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.社区活动跳转,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "招聘小区管理员",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "欢迎您入驻自家小区，邻信管理员招聘火热进行中，快来参加吧",
            //    title = "邻信管理员招聘火热进行中，快来参加吧",
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
            //    proName = "社区小管家招聘"

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm3), toId);


            //JPushMsgModel vm4 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.默认,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "天气",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "双卡双待康师傅阿萨德发射点发撒撒旦法 双卡双待康师傅阿萨德发射点发撒撒旦法 双卡双待康师傅阿萨德发射点发撒撒旦法 双卡双待康师傅阿萨德发射点发撒撒旦法 双卡双待康师傅阿萨德发射点发撒撒旦法 双卡双待康师傅阿萨德发射点发撒撒旦法 ",
            //    title = "双卡双待康师傅阿萨德发射点发撒撒旦法 ",
            //    logo = "http://i.cqjglx.com//html/mgrinvite/index.html",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm4), toId);


            //JPushMsgModel vm5 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.审核通过,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "社区认证",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "tips社区认证通过tips社区认证通过tips社区认证通过tips社区认证通过tips社区认证通过",
            //    title = "社区认证通过",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm5), toId);


            //JPushMsgModel vm6 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.审核失败,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "社区认证",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "tips社区认证失败社区认证失败社区认证失败社区认证失败社区认证失败社区认证失败社区认证失败",
            //    title = "社区认证失败",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm6), toId);



            //JPushMsgModel vm5 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.禁言,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "禁言",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "tips禁言禁言禁言禁言禁言禁言禁言禁言禁言禁言禁言",
            //    title = "你被禁言",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm5), toId);



            //JPushMsgModel vm5 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.解禁,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "解禁",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "tips解禁解禁解禁解禁解禁解禁解禁解禁解禁解禁解禁解禁解禁解禁",
            //    title = "你被解禁",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm5), toId);


            //JPushMsgModel vm6 = new JPushMsgModel()
            //{
            //    code = (int)MessageCenterModuleEnum.邻妹妹,
            //    proFlag = (int)PushMessageEnum.管理员审核通过,
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "管理员审核通过",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "tips管理员审核通过管理员审核通过管理员审核通过管理员审核通过管理员审核通过管理员审核通过",
            //    title = "管理员审核通过",

            //};
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm6), toId);



            JPushMsgModel vm7 = new JPushMsgModel()
            {
                code = (int)MessageCenterModuleEnum.邻妹妹,
                proFlag = (int)PushMessageEnum.管理员审核通过,
                proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tags = "管理员审核通过",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tips = "tips管理员审核通过管理员审核通过管理员审核通过管理员审核通过管理员审核通过管理员审核通过",
                title = "管理员审核通过",

            };
            Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(vm7), toId);



            return Content("ok");

            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = DateTime.Parse(DateTime.Now.ToString());
            TimeSpan toNow = dtNow.Subtract(dtStart);
            string timeStamp = toNow.TotalMilliseconds.ToString();

            //string toId = Guid.Parse("0DA1BC46-477F-481F-822D-A48DA560F6A3").ToString("N").ToLower();


            //JPushMsgModel jm22 = new JPushMsgModel()
            //{
            //    code = 1006,
            //    title = "瓜娃子商家入驻小区",
            //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
            //    nickname = "刘一手火锅",
            //    proFlag = 3,
            //    proId = "",
            //    proName = "",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "用户话题",
            //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tips = "，强势登录我们小区" + DateTime.Now,
            //    uid = Guid.Parse("0DA1BC46-477F-481F-822D-A48DA560F6A3").ToString("N").ToLower()
            //};

            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm22), toId);


            // return Content("aaddda");


            if (!string.IsNullOrEmpty(id))
            {
                var arr = id.Split(',');
                foreach (var s in arr)
                {

                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 5,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "社区活动跳转，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm), toId);


                    JPushMsgModel jm1 = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 1,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "好友申请，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm1), toId);


                    JPushMsgModel jm2 = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 0,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "默认不跳，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm2), toId);




                    JPushMsgModel jm3 = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 2,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "用户跳转，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm3), toId);



                    JPushMsgModel jm4 = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 3,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "社区服务跳转，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm4), toId);




                    JPushMsgModel jm5 = new JPushMsgModel()
                    {
                        code = Convert.ToInt32(s),
                        logo = "http://i.cqjglx.com//html/mgrinvite/index.html",
                        nickname = "刘一手火锅",
                        proFlag = 4,
                        proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
                        proName = "热烈祝贺，极光邻信2.0Beta版本内测",
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "用户话题",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = "精品汇跳转，张.大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题大但评论了您的话题" + DateTime.Now,
                        uid = "d675a17563c14f1e96598edecf08c137",
                        title = "标题来了哦松松松松" + DateTime.Now.ToString(),

                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm5), toId);


                    if (flag == 0)
                    {
                        //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm), toId);

                    }
                    else if (flag == 1)
                    {

                        Tuisong.PushMessage((int)PushPlatformEnum.Tags, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm), toId);
                    }
                    else
                    {
                        Tuisong.PushMessage((int)PushPlatformEnum.All, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm), toId);
                    }

                    Thread.Sleep(200);

                }

                //JPushMsgModel jm0 = new JPushMsgModel()
                //{
                //    code = 1006,
                //    title = "社区认证失败",
                //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                //    nickname = "",
                //    proFlag = 11,
                //    proId = "",
                //    proName = "",
                //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tags = "社区认证",
                //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tips = " 失败原因：资料不详细" + DateTime.Now,
                //    uid = toId
                //};

                //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm0), jm0.uid);



                //JPushMsgModel jm1 = new JPushMsgModel()
                //{
                //    code = 1006,
                //    title = "瓜娃子在评论话题",
                //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                //    nickname = "张三",
                //    proFlag = 0,
                //    proId = "",
                //    proName = "",
                //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tags = "用户话题",
                //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tips = " 评论了您的话题" + DateTime.Now,
                //    uid = "d675a17563c14f1e96598edecf08c137"
                //};

                //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm1), toId);

                //Thread.Sleep(200);


                //JPushMsgModel jm2 = new JPushMsgModel()
                //{
                //    code = 1006,
                //    title = "瓜娃子商家入驻小区",
                //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                //    nickname = "刘一手火锅",
                //    proFlag = 3,
                //    proId = "",
                //    proName = "",
                //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tags = "用户话题",
                //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tips = "，强势登录我们小区" + DateTime.Now,
                //    uid = Guid.Parse("D47E2A3C-AC0B-4FC3-86D9-CB55D525325F").ToString("N").ToLower()
                //};

                //Tuisong.PushMessage((int)PushPlatformEnum.Tags, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm2), toId);

                //Thread.Sleep(200);


                //JPushMsgModel jm3 = new JPushMsgModel()
                //{
                //    code = 1006,
                //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                //    title = "瓜娃子商家发布新的精品汇",
                //    nickname = "俏江南",
                //    proFlag = 4,
                //    proId = Guid.Parse("B1F13019-10AF-4611-9B30-FB138B4BA8C3").ToString("N").ToLower(),
                //    proName = "俏江南时节江湖菜",
                //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tags = "用户话题",
                //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    tips = "，精心只为您" + DateTime.Now,
                //    uid = Guid.Parse("D47E2A3C-AC0B-4FC3-86D9-CB55D525325F").ToString("N").ToLower()
                //};

                //Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm3), toId);

                //Thread.Sleep(200);



            } return Content("ok");


            // return Content(Guid.Parse("07435884-BFFF-4E84-95E2-0918E46E1291").ToString("N").ToLower());
            //return Content(JsonSerialize.Instance.ObjectToJson(jm));

            //JPushMsgModel jm2 = new JPushMsgModel()
            //{
            //    code = 1005,
            //    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
            //    nickname = "张大但",
            //    proFlag = "1",
            //    proId = "130B2FA7-B14A-484D-84A4-CBD63AC6E596",
            //    proName = "热烈祝贺，极光邻信2.0Beta版本内测",
            //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    tags = "用户话题",
            //    time = timeStamp,
            //    tips = "张.大但评论了您的话题" + DateTime.Now,
            //    uid = "9048B6C7-56EF-4E89-8C99-4A71FA0E0611"
            //};

            //Tuisong.PushMessage((int)PushPlatformEnum.All, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm2), "42f58af5f5894be48b0b25bb53305bfc");



            //return Content(JsonSerialize.Instance.ObjectToJson(jm));
            sms sms = new sms();
            string msg = "您申请的验证码是：" + 1555 + "。";
            //  SubmitResult sr = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"], "15825942359", msg);
            //Juhe.Update("D:\\juheDept.txt");
            return View();
            Juhe.Update("D:\\juheDept.txt");

            Guid gu = Guid.Parse("30238388-218c-4c9f-a96d-293c077548b9");
            //var list = bCore.GetBusinessServiceByBuildingId(gu,1);


            MalShoppingCarCore msCore = new MalShoppingCarCore();

            ShoppingCarViewCore scCoew = new ShoppingCarViewCore();
            var list = scCoew.LoadEntities(o => o.S_UId == gu).OrderByDescending(o => o.S_Time).ToList();


            //按照商家分组查询
            var gp = list.GroupBy(o => new { o.S_BusId, o.B_NickName }).Select(o => new
            {
                busId = o.Key.S_BusId,
                busName = o.Key.B_NickName,
                busList = list.Where(p => p.S_BusId == o.Key.S_BusId).Select(s => new
                {
                    busId = s.S_BusId,
                    busName = s.B_NickName,
                    imgUrl = s.G_Img,
                    carId = s.S_Id,
                    goodsId = s.S_GoodsId,
                    goodsName = s.G_Name,
                    goodsCount = s.S_GoodsCount,
                    price = s.G_Price,
                    fee = s.G_ExtraFee,
                    state = s.G_Status,
                    clientFlag = 0
                }),
                extraFee = list.Max(c => c.G_ExtraFee)
            });




            var a = new MallGoodsCore().CountGoods(Guid.Empty, Guid.Empty);
            return Content(JsonSerialize.Instance.ObjectToJson(gp));
            //return Content(HuanXin.ExitQun("171121951853511124", "lx_403351e0-581d-4ea2-887c-1351bf5f28bb").ToString());


            //HuanXin.AccountResetNickname("lx18580465179", "hello jack");
            //HuanXin.CreateUser("lx_goodjob1","好的，goodjob");
            return View();
            //sms sms = new sms();
            //string msg = "您申请的验证码是：" + 1555 + "。";
            //SubmitResult sr = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"], "15825942359", msg);

            string js = JsonSerialize.Instance.ObjectToJson(new MessageInfo(205, "邻友  2.0   已入驻您的小区66666666"));
            var ss = new List<object> { new List<object> { new { hot = "a" }, new { hot = "b" } }, new List<object> { new { name = "a" }, new { name = "b" } } };
            return Content(JsonSerialize.Instance.ObjectToJson(ss));
            //Tuisong.Users(new string[] { "125", "132" }, js);
            //Tuisong.PushMessage((int)PushPlatformEnum.Alias, js, new string[] { "54912", "53659" });
            //Tuisong.Building("54912", new MessageInfo(205, "邻友  2.0   已入驻您的小区"));
            //            Tuisong.User(132, new MessageInfo(202, new MessageData("单号", "15825942355", 100, "fuck00000")));
            // Tuisong.User(132,"000000");
            //Tuisong.Building("54912", new MessageInfo(205, "邻友  2.0   已入驻您的小区"));


            //string rs =  HuanXin.CreateUser("abcd123456")?"ok":"error";
            //string rs = HuanXin.CreateQun("qun123456") ;
            //return Content(rs);


            //SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            //sParaTemp.Add("partner", Com.Alipay.Config.Partner);
            //sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
            //sParaTemp.Add("service", "single_trade_query");
            ////sParaTemp.Add("trade_no", order);//使用支付宝交易号查询
            //sParaTemp.Add("out_trade_no", "URI50E1CTDQ7GC9");//使用商户订单号查询

            //string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp);

            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(sHtmlText);
            //if (xmlDoc.DocumentElement["is_success"].InnerText == "T")
            //{
            //    string sub = xmlDoc.DocumentElement["response"]["trade"]["subject"].InnerText;
            //    string moneys = xmlDoc.DocumentElement["response"]["trade"]["total_fee"].InnerText;

            //    return Content(string.Format("主题：{0},金额：{1}",sub,moneys));
            //}
            //return Content("哦豁");

            //Juhe.Update("D:\\juheDept.txt");
            string str =
                "{\"reason\":\"查询成功\",\"result\":[{\"provinceId\":\"v2171\",\"cityId\":\"v2173\",\"payProjectId\":\"c2670\",\"payUnitId\":\"v83575\",\"payUnitName\":\"沈阳市水费代收\"},{\"provinceId\":\"v2171\",\"cityId\":\"v2173\",\"payProjectId\":\"c2670\",\"payUnitId\":[],\"payUnitName\":\"沈阳胜科水务（张士开发区）\"}],\"error_code\":0}";
            return Content(DESProvider.EncryptString("123456", "12345678") + "*****" + js);
            //return Content(DESProvider.Encrypt("123456") + "`````" + DESProvider.DesDecrypt("HUX+7VtHgb0="));
            return View();
        }

        public ActionResult Jpush(string uid = "", string buildingId = "")
        {


            if (!string.IsNullOrEmpty(uid))
            {

                JPushMsgModel jm22 = new JPushMsgModel()
                {
                    code = 1006,
                    title = "瓜娃子商家入驻小区",
                    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                    nickname = "刘一手火锅",
                    proFlag = 3,
                    proId = "",
                    proName = "",
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "用户话题",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = "，强势登录我们小区" + DateTime.Now,
                    uid = Guid.Parse("D47E2A3C- AC0B-4FC3-86D9-CB55D525325F").ToString("N").ToLower()
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm22), uid);

            }


            if (!string.IsNullOrEmpty(buildingId))
            {

                JPushMsgModel jm22 = new JPushMsgModel()
                {
                    code = 1006,
                    title = "瓜娃子商家入驻小区",
                    logo = "http://192.168.1.172:8122/album/01bc600b53d94a17890d37dc66123943.png",
                    nickname = "刘一手火锅",
                    proFlag = 3,
                    proId = "",
                    proName = "",
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "用户话题",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = "，强势登录我们小区" + DateTime.Now,
                    uid = Guid.Parse("D47E2A3C-AC0B-4FC3-86D9-CB55D525325F").ToString("N").ToLower()
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Tags, "alter " + DateTime.Now.ToString(), "title " + DateTime.Now.ToString(), JsonSerialize.Instance.ObjectToJson(jm22), buildingId);

            }


            return Content("0");
        }

        /// <summary>
        /// 银联便民
        /// </summary>
        /// <returns></returns>
        public ActionResult Chinapay()
        {
            string branchId = "00003978";//webconfig配置


            string key = "重庆";
            int projectId = 1; //交水费

            string no = "0101123456";

            var obj = cpCore.LoadEntity(o => o.D_CityName == key && o.D_ProjectID == projectId);

            if (obj != null) //城市/缴费机构 存在
            {
                if (obj.D_BillPwd != 0) //需要密码
                {

                }
                if (obj.D_BillDate != 0) //需要日期
                {

                }

                string merSysId = obj.D_MerSysId;
                string queryNo = no;//查询账号
                string billType = obj.D_BillType;//账单类型
                string merBillStat = "02"; //全部
                string billDate = "";//查询账期
                string password = "";//密码
                string individualArea = obj.D_IndividualAreaQuery;//特殊需求
                string writeOffType = "01";



            }
            return Content("lalala ");
        }

        public ActionResult Test()
        {
            BusinessCore uc = new BusinessCore();

            string a = ";";
            var list = uc.LoadEntities().Select(o => new { o.B_Id }).ToList();


            return Content(list.ToString());
        }
    }
}
