using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using cn.jpush.api;
using cn.jpush.api.common;
using cn.jpush.api.push.mode;
using cn.jpush.api.push.notification;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json;

namespace JiGuangLinXin.App.Services
{


    public static class Tuisong
    {
        #region 作废的逻辑
        //    //public static string Key = "58949702eb35feca992cbbdf";
        //    //public static string Secret = "daeb50a576e27f4712e37322";
        //    /// <summary>
        //    /// 推送至单个会员
        //    /// </summary>
        //    /// <param name="userid">单个会员的ID</param>
        //    /// <param name="info">推送的内容(消息实体对象)</param>
        //    public static void User(string userid, MessageInfo info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_alias(userid);
        //            pushPayload.message = Message.content(JsonConvert.SerializeObject(info));
        //            client.SendPush(pushPayload);
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    /// <summary>
        //    /// 推送至单个会员
        //    /// </summary>
        //    /// <param name="userid">会员ID</param>
        //    /// <param name="info">推送的内容(序列化好的字符串)</param>
        //    public static void User(string userid, string info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_alias(userid.ToString());
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    /// <summary>
        //    /// 推送给多个人
        //    /// </summary>
        //    /// <param name="userid">人员ID</param>
        //    /// <param name="info"></param>
        //    public static void Users(string[] userid, string info)
        //    {

        //        PushMessage((int)PushPlatformEnum.Alias, JsonConvert.SerializeObject(info), userid);

        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_alias(userid);
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }



        //    public static void Building(string building, MessageInfo info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_tag(building);
        //            pushPayload.message = Message.content(JsonConvert.SerializeObject(info));
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    public static void Building(string[] buildings, string info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_tag(buildings);
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    public static void City(string city, string info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_tag(city);
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    public static void City(string[] citys, string info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.s_tag(citys);
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }

        //    public static void All(string info)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();
        //            pushPayload.audience = Audience.all();
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    /// <summary>
        //    /// 推送给所有人
        //    /// </summary>
        //    /// <param name="info">消息实体</param>
        //    public static void All(MessageInfo info)
        //    {

        //        PushMessage((int)PushPlatformEnum.All, JsonConvert.SerializeObject(info));
        //        //try
        //        //{
        //        //    JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //        //    PushPayload pushPayload = new PushPayload();
        //        //    pushPayload.platform = Platform.all();
        //        //    pushPayload.audience = Audience.all();
        //        //    pushPayload.message = Message.content(JsonConvert.SerializeObject(info));
        //        //    client.SendPush(pushPayload);
        //        //}
        //        //catch
        //        //{
        //        //}
        //    }


        //    #endregion

        //    /// <summary>
        //    /// 推送消息
        //    /// </summary>
        //    /// <param name="platform">目标标识</param>
        //    /// <param name="info">推送的消息</param>
        //    /// <param name="values">标签(用户id,社区id,城市名)</param>
        //    public static void PushMessage(int platform, string info, params string[] values)
        //    {
        //        try
        //        {
        //            JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"], ConfigurationManager.AppSettings["JPushSecret"]);
        //            PushPayload pushPayload = new PushPayload();
        //            pushPayload.platform = Platform.all();

        //            switch (platform)
        //            {
        //                case (int)PushPlatformEnum.Alias:
        //                    pushPayload.audience = Audience.s_alias(values);
        //                    break;

        //                case (int)PushPlatformEnum.Tags:
        //                    pushPayload.audience = Audience.s_tag(values);
        //                    break;
        //                default:
        //                    pushPayload.audience = Audience.all();
        //                    break;
        //            }
        //            pushPayload.message = Message.content(info);
        //            client.SendPush(pushPayload);
        //        }
        //        catch (APIRequestException e)//推送异常
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine("异常消息：" + e.Message);
        //            sb.AppendLine("异常类型：" + e.GetType().FullName);
        //            sb.AppendLine("异常方法：" + (e.TargetSite == null ? null : e.TargetSite.Name));
        //            sb.AppendLine("异常源：" + e.Source);

        //            string errMsg = string.Format("自定义信息：极光推送消息返回异常\n{0}", sb);
        //            new LogHelper("D:\\linxinAppLog\\service\\", LogType.Daily).Write(errMsg, LogLevel.Error);
        //        }
        //    }
        #endregion


        /// <summary>
        /// 推送 消息
        /// </summary>
        /// <param name="platform">目标平台（1.用户 2群 3全部）</param>
        /// <param name="alert">必填	通知内容</param>
        /// <param name="title">可选	通知标题</param>
        /// <param name="content">必填 消息内容本身</param>
        /// <param name="ids">必填 用户id,社区id【去掉 - 同时 全小写】</param>
        public static void PushMessage(int platform, string alert, string title, string content, params string[] ids)
        {
            //return;
       //     string[] hxIds = ids; //环信推送的ID 
            try
            {

                JPushClient client = new JPushClient(ConfigurationManager.AppSettings["JPushKey"],
                    ConfigurationManager.AppSettings["JPushSecret"]);
                PushPayload pushPayload = new PushPayload();
                pushPayload.platform = Platform.all();
                pushPayload.options = new Options() { apns_production = true }; //false:IOS开发环境 true : 生产环境
                switch (platform)
                {
                    case (int)PushPlatformEnum.Alias:
                        pushPayload.audience = Audience.s_alias(ids);


                     //   hxIds = ids.Select(o => "lx_" + Guid.Parse(o)).ToArray();

                        break;

                    case (int)PushPlatformEnum.Tags:
                        pushPayload.audience = Audience.s_tag(ids);


                    //    hxIds = ids.Select(o => "group_" + Guid.Parse(o)).ToArray();

                        break;
                    default:
                        pushPayload.audience = Audience.all();
                        break;
                }


                var notification = new Notification().setAlert(alert);
                notification.AndroidNotification = new AndroidNotification().setTitle(title);
                notification.AndroidNotification.AddExtra("message", content);

                notification.IosNotification = new IosNotification();
                notification.IosNotification.AddExtra("message", content);
                notification.IosNotification.incrBadge(1);
                notification.IosNotification.setSound("happy");
                pushPayload.message = Message.content(content);

                //notification.IosNotification.AddExtra("extra_key", "extra_value");
                //pushPayload.message = Message.content(MSG_CONTENT).AddExtras("from", "JPush");
                pushPayload.notification = notification.Check();
                client.SendPush(pushPayload);



            }
            catch (APIRequestException e) //推送异常                                   n
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("异常IP：" + HttpContext.Current.Request.UserHostAddress );
                sb.AppendLine("异常消息：" + e.Message);
                sb.AppendLine("异常类型：" + e.GetType().FullName);
                sb.AppendLine("异常方法：" + (e.TargetSite == null ? null : e.TargetSite.Name));
                sb.AppendLine("异常源：" + e.Source);

                string errMsg = string.Format("自定义信息：极光推送消息返回异常\n{0}", sb);
                new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Service, LogType.Daily).Write(
                    errMsg, LogLevel.Error);
            }

            //try
            //{

            //    #region 环信推送

            //    EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"],
            //        ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"],
            //        ConfigurationManager.AppSettings["HxOrgName"]);
            //    string code = ease.SendMessage(platform, content, hxIds);

            //    #endregion
            //}
            //catch (Exception e) //推送异常                                   n
            //{
            //    StringBuilder sb = new StringBuilder();
            //    sb.AppendLine("异常消息：" + e.Message);
            //    sb.AppendLine("异常类型：" + e.GetType().FullName);
            //    sb.AppendLine("异常方法：" + (e.TargetSite == null ? null : e.TargetSite.Name));
            //    sb.AppendLine("异常源：" + e.Source);

            //    string errMsg = string.Format("自定义信息：环信推送消息返回异常\n{0}", sb);
            //    new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.Service, LogType.Daily).Write(
            //        errMsg, LogLevel.Error);
            //}

            #region 服务器处理推送消息

            try
            {
                //反序列化 推送的消息
                JPushMsgModel msgVm = JsonSerialize.Instance.JsonToObject<JPushMsgModel>(content);

                using (LinXinApp20Entities db = new LinXinApp20Entities())
                {
                    foreach (var id in ids)
                    { 
                        var jmsg = new Sys_JPushMessage()
                        {
                            M_Content = content,
                            M_Id = Guid.NewGuid(),
                            M_ReadList = "",
                            M_State = 0,
                            M_TargetId = "",
                            M_Time = DateTime.Now,
                            M_Title = title,
                            M_Type = platform,
                            M_Code = msgVm.code
                        };
                        Guid temp;
                        if (Guid.TryParse(id, out temp))
                        {
                            jmsg.M_TargetId = temp.ToString();
                        }

                        db.Sys_JPushMessage.Add(jmsg);
                    }
                    if (platform == 3)  //全体成员的消息
                    {
                        var jmsg = new Sys_JPushMessage()
                        {
                            M_Content = content,
                            M_Id = Guid.NewGuid(),
                            M_ReadList = "",
                            M_State = 0,
                            M_TargetId = "",
                            M_Time = DateTime.Now,
                            M_Title = title,
                            M_Type = platform,
                            M_Code = msgVm.code
                        };

                        db.Sys_JPushMessage.Add(jmsg);
                    }

                    db.SaveChanges(); //单元提交

                }
            }
            catch (Exception)
            {
            }
            #endregion
        }

        /// <summary>
        /// 推送给所有人
        /// </summary>
        /// <param name="alert">必填	通知内容</param>
        /// <param name="title">可选	通知标题</param>
        /// <param name="content">必填 消息内容本身</param>
        /// <returns></returns>
        public static PushPayload PushObjectToAll(string alert, string title, string content)
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            var audience = Audience.all();
            pushPayload.audience = audience;
            var notification = new Notification().setAlert(alert);
            notification.AndroidNotification = new AndroidNotification().setTitle(title);
            notification.IosNotification = new IosNotification();
            notification.IosNotification.incrBadge(1);
            notification.IosNotification.setSound("happy");
            //notification.IosNotification.AddExtra("extra_key", "extra_value");
            //pushPayload.message = Message.content(MSG_CONTENT).AddExtras("from", "JPush");
            pushPayload.message = Message.content(content);


            pushPayload.notification = notification.Check();
            return pushPayload;
        }
        /// <summary>
        /// 按用户的ID推送
        /// </summary>
        /// <param name="alert">必填	通知内容</param>
        /// <param name="title">可选	通知标题</param>
        /// <param name="content">必填 消息内容本身</param>
        /// <param name="ids">必填 用户id,社区id【去掉 - 同时 全小写】</param>
        /// <returns></returns>
        public static PushPayload PushObjectToUsers(string alert, string title, string content, params string[] ids)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            pushPayload.audience = Audience.s_alias(ids);

            var notification = new Notification().setAlert(alert);
            notification.AndroidNotification = new AndroidNotification().setTitle(title);
            notification.IosNotification = new IosNotification();
            notification.IosNotification.incrBadge(1);
            notification.IosNotification.setSound("happy");
            pushPayload.message = Message.content(content);


            pushPayload.notification = notification.Check();
            return pushPayload;

        }
        /// <summary>
        /// 按用户所在小区ID推送
        /// </summary>
        /// <param name="alert">必填	通知内容</param>
        /// <param name="title">可选	通知标题</param>
        /// <param name="content">必填 消息内容本身</param>
        /// <param name="ids">必填 用户id,社区id【去掉 - 同时 全小写】</param>
        /// <returns></returns>
        public static PushPayload PushObjectToGroups(string alert, string title, string content, params string[] ids)
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();


            pushPayload.audience = Audience.s_tag(ids);

            var notification = new Notification().setAlert(alert);
            notification.AndroidNotification = new AndroidNotification().setTitle(title);
            notification.IosNotification = new IosNotification();
            notification.IosNotification.incrBadge(1);
            notification.IosNotification.setSound("happy");
            pushPayload.message = Message.content(content);

            pushPayload.notification = notification.Check();
            return pushPayload;
        }
    }


    /// <summary>
    /// 推送消息的目标标识
    /// </summary>
    public enum PushPlatformEnum
    {
        /// <summary>
        /// 按别名推送，用户
        /// </summary>
        Alias = 1,
        /// <summary>
        /// 按标签推送，群
        /// </summary>
        Tags = 2,
        /// <summary>
        /// 所有
        /// </summary>
        All = 3,
        /// <summary>
        /// 群管理员
        /// </summary>
        GroupMgr = 4
    }
}