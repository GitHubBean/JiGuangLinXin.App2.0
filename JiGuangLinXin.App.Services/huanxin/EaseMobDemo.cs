using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.Services
{
    /// <summary>
    /// 环信服务器端会员访问接口Demo
    /// Author：Mr.Hu
    /// QQ:346163801
    /// Email:346163801@qq.com
    /// 如有任何问题，可QQ或邮箱联系
    /// </summary>
    public class EaseMobDemo
    {
        string reqUrlFormat = "https://a1.easemob.com/{0}/{1}/";
        public string clientID { get; set; }
        public string clientSecret { get; set; }
        public string appName { get; set; }
        public string orgName { get; set; }
        public string token { get; set; }
        public string easeMobUrl { get { return string.Format(reqUrlFormat, orgName, appName); } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="easeAppClientID">client_id</param>
        /// <param name="easeAppClientSecret">client_secret</param>
        /// <param name="easeAppName">应用标识之应用名称</param>
        /// <param name="easeAppOrgName">应用标识之登录账号</param>
        public EaseMobDemo(string easeAppClientID, string easeAppClientSecret, string easeAppName, string easeAppOrgName)
        {
            this.clientID = easeAppClientID;
            this.clientSecret = easeAppClientSecret;
            this.appName = easeAppName;
            this.orgName = easeAppOrgName;
            this.token = QueryToken();
        }

        /// <summary>
        /// 使用app的client_id 和 client_secret登陆并获取授权token
        /// </summary>
        /// <returns></returns>
        string QueryToken()
        {
            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret)) { return string.Empty; }
            string cacheKey = clientID + clientSecret;
            if (System.Web.HttpRuntime.Cache.Get(cacheKey) != null &&
                System.Web.HttpRuntime.Cache.Get(cacheKey).ToString().Length > 0)
            {
                return System.Web.HttpRuntime.Cache.Get(cacheKey).ToString();
            }

            string postUrl = easeMobUrl + "token";
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"grant_type\": \"client_credentials\",\"client_id\": \"{0}\",\"client_secret\": \"{1}\"", clientID, clientSecret);
            _build.Append("}");

            string postResultStr = ReqUrl(postUrl, "POST", _build.ToString(), string.Empty);
            string token = string.Empty;
            int expireSeconds = 0;
            try
            {
                JObject jo = JObject.Parse(postResultStr);
                token = jo.GetValue("access_token").ToString();
                int.TryParse(jo.GetValue("expires_in").ToString(), out expireSeconds);
                //设置缓存
                if (!string.IsNullOrEmpty(token) && token.Length > 0 && expireSeconds > 0)
                {
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, token, null, DateTime.Now.AddSeconds(expireSeconds), System.TimeSpan.Zero);
                }
            }
            catch { return postResultStr; }
            return token;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="password">密码</param>
        /// <param name="nickname">密码</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string userName, string password, string nickname = "")
        {
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"username\": \"{0}\",\"password\": \"{1}\",\"nickname\": \"{2}\"", userName, password, nickname);
            _build.Append("}");

            return AccountCreate(_build.ToString());
        }
        /// <summary>
        /// 创建群
        /// </summary>
        /// <param name="groupname">群组名称</param>
        /// <param name="desc">群组描述</param>
        /// <param name="publik">是否是公开群</param>
        /// <param name="maxusers">群组成员最大数（包括群主）</param>
        /// <param name="approval">加入公开群是否需要批准</param>
        /// <returns></returns>
        public string QunCreate(string groupname, string desc, string publik, string maxusers, string approval)
        {
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"groupname\": \"{0}\",\"desc\": \"{1}\",\"public\": {2},\"maxusers\": {3},\"approval\": {4},\"owner\": \"LMM\"", groupname, desc, publik, maxusers, approval);
            _build.Append("}");

            return AccountQun(_build.ToString());
        }
        /// <summary>
        /// 环信推送消息
        /// </summary>
        /// <param name="target_type">1用户 2群组</param>
        /// <param name="content">内容（json序列化）</param>
        /// <param name="ids">对象集合</param>
        /// <returns></returns>
        public string SendMessage(int target_type, string content, params string[] ids)
        {
            string tar = "";
            if (target_type == 2)
            {
                tar = "chatgroups";
            }
            else if (target_type == 1)
            {
                tar = "users";
            }

            //JPushMsgModel obj = JsonSerialize.Instance.JsonToObject<JPushMsgModel>(content);

            //StringBuilder extStr = new StringBuilder();
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "code", obj.code);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "title", obj.title);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "tips", obj.tips);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "uid", obj.uid);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "logo", obj.logo);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "nickname", obj.nickname);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "proFlag", obj.proFlag);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "proId", obj.proId);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "proName", obj.proName);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "proTime", obj.proTime);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "time", obj.time);
            //extStr.AppendFormat("\"{0}\":\"{1}\"", "tags", obj.tags); 


            StringBuilder _msg = new StringBuilder();
            _msg.Append("{");
            _msg.AppendFormat("\"target_type\": \"{0}\",\"target\": {1},\"msg\": {2}", tar, "[" + string.Join(",", ids.Select(o=>"\""+o+"\"")) + "]", "{\"type\" : \"txt\",\"msg\" : \"" + DESProvider.EncryptString(content) + "\"}");
            _msg.Append("}");

            return ReqUrl(easeMobUrl + "messages", "POST", _msg.ToString(), token);
        }



        /// <summary>
        /// 加入群
        /// </summary>
        /// <param name="qunId">群ID</param>
        /// <param name="userName">用户帐号</param>
        /// <returns></returns>
        public string AccountQunJoin(string qunId, string userName) { return ReqUrl(easeMobUrl + "chatgroups/" + qunId + "/users/" + userName, "POST", string.Empty, token); }


        /// <summary>
        /// 退出群
        /// </summary>
        /// <param name="qunId">群ID</param>
        /// <param name="userName">用户帐号</param>
        /// <returns></returns>
        public string ExitQun(string qunId, string userName) { return ReqUrl(easeMobUrl + "chatgroups/" + qunId + "/users/" + userName, "DELETE", string.Empty, token); }
        /// <summary>
        /// 查找所有群
        /// </summary>
        public string SelectQun() { return ReqUrl(easeMobUrl + "chatgroups", "GET", "", token); }


        public string QueryChatMessage() { return ReqUrl(easeMobUrl + "chatmessages", "GET", "", token); }

        public string AccountQun(string postData) { return ReqUrl(easeMobUrl + "chatgroups", "POST", postData, token); }

        /// <summary>
        /// 创建用户(可以批量创建)
        /// </summary>
        /// <param name="postData">创建账号JSON数组--可以一个，也可以多个</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string postData) { return ReqUrl(easeMobUrl + "users", "POST", postData, token); }

        /// <summary>
        /// 获取指定用户详情
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>会员JSON</returns>
        public string AccountGet(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "GET", string.Empty, token); }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>重置结果JSON(如：{ "action" : "set user password",  "timestamp" : 1404802674401,  "duration" : 90})</returns>
        public string AccountResetPwd(string userName, string newPassword) { return ReqUrl(easeMobUrl + "users/" + userName + "/password", "PUT", "{\"newpassword\" : \"" + newPassword + "\"}", token); }

        /// <summary>
        /// 重置用户昵称
        /// </summary>
        /// <param name="userName">帐号</param>
        /// <param name="newNickname">新昵称</param>
        /// <returns></returns>
        public string AccountResetNickname(string userName, string newNickname) { return ReqUrl(easeMobUrl + "users/" + userName, "PUT", "{\"nickname\" : \"" + newNickname + "\"}", token); }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>成功返回会员JSON详细信息，失败直接返回：系统错误信息</returns>
        public string AccountDel(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "DELETE", string.Empty, token); }

        public string ReqUrl(string reqUrl, string method, string paramData, string token)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
                request.Method = method.ToUpperInvariant();

                if (!string.IsNullOrEmpty(token) && token.Length > 1) { request.Headers.Add("Authorization", "Bearer " + token); }
                if (request.Method.ToString() != "GET" && !string.IsNullOrEmpty(paramData) && paramData.Length > 0)
                {
                    request.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(paramData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex) { return ex.ToString(); }
        }
    }

    public class SampleUse
    {
        /// <summary>
        /// 使用范例
        /// HXComm.SampleUse mySample = new HXComm.SampleUse();
        /// mySample.Test(appClientID, appClientSecret, appName, orgName);
        /// </summary>
        /// <param name="appClientID"></param>
        /// <param name="appClientSecret"></param>
        /// <param name="appName"></param>
        /// <param name="orgName"></param>
        public void Test(string appClientID, string appClientSecret, string appName, string orgName)
        {
            EaseMobDemo myEaseMobDemo = new EaseMobDemo(appClientID, appClientSecret, appName, orgName);
            string userName = "a001", password = "a001", newPassword = "a000000";//此处我们要进行加密处理，如果在实际项目中，建议加密

            Console.WriteLine("{0}", myEaseMobDemo.AccountCreate(userName, password));
            Console.WriteLine("{0}", myEaseMobDemo.AccountGet(password));
            Console.WriteLine("{0}", myEaseMobDemo.AccountResetPwd(userName, newPassword));
            Console.WriteLine("{0}", myEaseMobDemo.AccountDel(password));
        }

        /// <summary>
        /// 批量导入账号(txt文件)
        /// 内容格式为(用制表符或空格隔开即可)：账号 密码
        /// </summary>
        /// <param name="txtFile">文件保存地址</param>
        /// <param name="appClientID"></param>
        /// <param name="appClientSecret"></param>
        /// <param name="appName"></param>
        /// <param name="orgName"></param>
        public void ImportUserToHX(string txtFile, string appClientID, string appClientSecret, string appName, string orgName)
        {
            using (StreamReader sr = new StreamReader(txtFile))
            {
                string line = sr.ReadLine();
                EaseMobDemo easeMob = new EaseMobDemo(appClientID, appClientSecret, appName, orgName);
                System.Text.StringBuilder _build = new StringBuilder();
                while (line != null)
                {
                    string[] arr = line.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length == 2)
                    {
                        string u = arr[0], p = arr[1];
                        string returnMsg = easeMob.AccountCreate(u, p);
                        _build.AppendFormat("{0}\r\n\r\n", returnMsg);
                        Console.WriteLine(returnMsg);
                    }
                    line = sr.ReadLine();
                }

                string logPath = AppDomain.CurrentDomain.BaseDirectory + "Log" + DateTime.Now.Ticks.ToString() + ".txt";
                using (StreamWriter sw = new StreamWriter(logPath, false, Encoding.UTF8))
                {
                    sw.Write(_build.ToString());
                    sw.Flush();
                }
            }
        }
    }
}
