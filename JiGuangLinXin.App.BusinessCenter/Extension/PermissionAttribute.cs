using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;


namespace JiGuangLinXin.App.App20Interface.Extension
{
    /// <summary>
    /// 口令验证
    /// </summary>
    public class PermissionAttributeFilter : ActionFilterAttribute
    {
        private UserCore uCore = new UserCore();
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                string requestToken = "";
                string uid = "";
                string phone = "";
                string platform = "";

                if (actionContext.Request.Headers.Any(o => o.Key == "platform"))
                {
                    platform = actionContext.Request.Headers.GetValues("platform").FirstOrDefault();
                }
                if (actionContext.Request.Headers.Any(o => o.Key == "phone"))
                {
                    phone = actionContext.Request.Headers.GetValues("phone").FirstOrDefault();
                }
                if (actionContext.Request.Headers.Any(o => o.Key == "uid"))
                {
                    uid = actionContext.Request.Headers.GetValues("uid").FirstOrDefault();
                }
                if (actionContext.Request.Headers.Any(o => o.Key == "token"))
                {
                    requestToken = actionContext.Request.Headers.GetValues("token").FirstOrDefault();
                }



                //测试
                if (string.IsNullOrEmpty(uid))
                {
                    actionContext.Request.Headers.Add("uid", "4C88057E-AC17-4DDA-94CF-8274A1382AE6");
                    actionContext.Request.Headers.Add("phone", "15825942359");
                    actionContext.Request.Headers.Add("platform", "8");
                    base.OnActionExecuting(actionContext);
                    return;
                }
                //测试 end



                //var serverKey = System.Web.Configuration.WebConfigurationManager.AppSettings["token"];
                TokenViewModel token = JsonSerialize.Instance.JsonToObject<TokenViewModel>(Md5Extensions.MD5Decrypt(requestToken));

                if (token.Uid != uid || token.Phone != phone || token.Platform.ToString() != platform)
                {
                    ResultViewModel rs = new ResultViewModel() { State = 1000, Msg = string.Format("口令错误,token uid:{0},客户端 uid:{1}; token phone:{2},客户端 phone {3};token platform:{4},客户端 platform:{5}", token.Uid, uid, token.Phone, phone, token.Platform, platform), Data = null };
                    actionContext.Response = new HttpResponseMessage { Content = new StringContent(JsonSerialize.Instance.ObjectToJson(rs), Encoding.GetEncoding("UTF-8"), "application/json") };
                }
                else if (token.Time.AddDays(30) < DateTime.Now)
                {
                    ResultViewModel rs = new ResultViewModel() { State = 1001, Msg = "口令过期", Data = null };
                    actionContext.Response = new HttpResponseMessage { Content = new StringContent(JsonSerialize.Instance.ObjectToJson(rs), Encoding.GetEncoding("UTF-8"), "application/json") };
                }
                else//校验通过
                {
                    //枚举出所有未认证仍然可以操作的controller action
                    string controller = actionContext.ControllerContext.RouteData.Values["controller"].ToString();
                    string action = actionContext.ControllerContext.RouteData.Values["action"].ToString(); ;

                    List<KeyPairPath> ft = new KeyPairPath().GetFileterPath();
                    if (ft.Find(o => o.Controller.ToLower() == controller.ToLower() && o.Action.ToLower() == action.ToLower()) == null) //必须要进行认证
                    {
                        Guid userId = Guid.Parse(uid);
                        var user = new UserCore().LoadEntity(o => o.U_Id == userId);
                        if (user.U_Status == (int)UserStatusEnum.冻结)
                        {
                            ResultViewModel rs = new ResultViewModel() { State = 1, Msg = "业主帐号已被冻结！", Data = null };
                            actionContext.Response = new HttpResponseMessage { Content = new StringContent(JsonSerialize.Instance.ObjectToJson(rs), Encoding.GetEncoding("UTF-8"), "application/json") };
                        }
                        else if (user.U_AuditingState != (int)AuditingEnum.认证成功)
                        {
                            ResultViewModel rs = new ResultViewModel() { State = 1, Msg = "您已入驻自己小区，请通过实名认证！", Data = null };
                            actionContext.Response = new HttpResponseMessage { Content = new StringContent(JsonSerialize.Instance.ObjectToJson(rs), Encoding.GetEncoding("UTF-8"), "application/json") };
                        }
                        else
                        {
                            base.OnActionExecuting(actionContext);  //可以执行
                        }
                    }
                    else
                    {
                        base.OnActionExecuting(actionContext);  //可以执行
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper log = new LogHelper(ConfigurationManager.AppSettings["LogPath"] + ErrorLogEnum.System, LogType.Daily);
                log.Write(ex.Message + "\n" + ex.StackTrace, LogLevel.Error);

                ResultViewModel rs = new ResultViewModel() { State = 9999, Msg = "网络异常，请检查网络"+ex.StackTrace, Data = null };
                actionContext.Response = new HttpResponseMessage { Content = new StringContent(JsonSerialize.Instance.ObjectToJson(rs), Encoding.GetEncoding("UTF-8"), "application/json") };

                //此处暂时以401返回，可调整为其它返回 
                //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized); //访问拒绝
            }
            //var obj = new ResultViewModel() { State = 9999, Msg = "网络异常", Data = null };
            // string str = JsonSerialize.Instance.ObjectToJson(obj);
            //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            // actionContext.Response = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            //actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);  
        }

    }

    /// <summary>
    /// 键值对
    /// </summary>
    public class KeyPairPath
    {
        public string Controller { get; set; }
        public string Action { get; set; }

        /// <summary>
        /// 获取过滤的 路径地址
        /// </summary>
        /// <returns></returns>
        public List<KeyPairPath> GetFileterPath()
        {
            List<KeyPairPath> paths = new List<KeyPairPath>();
            paths.Add(new KeyPairPath()
            {
                Controller = "auditing",
                Action = "buildingauditing"
            });   paths.Add(new KeyPairPath()
            {
                Controller = "auditing",
                Action = "uploadtest"
            });
            paths.Add(new KeyPairPath()
            {
                Controller = "token",
                Action = "refreshtoken"
            });
            paths.Add(new KeyPairPath()
            {
                Controller = "common",
                Action = "uploadtest"
            });
            paths.Add(new KeyPairPath()
            {
                Controller = "Chat",
                Action = "FriendList"
            });
            paths.Add(new KeyPairPath()
            {
                Controller = "Chat",
                Action = "QueryUserByChatId"
            });
            paths.Add(new KeyPairPath()
            {
                Controller = "Main",
                Action = "Index"
            });
            return paths;
        }
    }
}
