using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Spatial;
using System.IO;
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
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.Rpg;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;
using AcceptVerbsAttribute = System.Web.Http.AcceptVerbsAttribute;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 用户登录、注册
    /// </summary>
    public class UserController : ApiController
    {
        private UserCore uCore = new UserCore();
        private VillageCore villCore = new VillageCore();
        private OperateLogCore logCore = new OperateLogCore();

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <returns></returns>
        public HttpResponseMessage SendSMS([FromBody]JObject value)
        {
            dynamic oc = value;
            string phone = oc.phone;
            ResultViewModel rs = new ResultViewModel();
            //return WebApiJsonResult.ToJson(rs);

            //ResultViewModel tt = new ResultViewModel();
            //tt.Msg = Request.Headers.GetValues("platform").FirstOrDefault();
            //return WebApiJsonResult.ToJson(tt);

            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            //if (Request.Headers.Any(o=>o.Key == "platform"))
            //{
            //    dr = Request.Headers.GetValues("platform").FirstOrDefault();
            //}
            int pla = 0;
            //号码合法，并且号码未被注册
            if (!phone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            else if (!int.TryParse(dr, out pla))
            {
                rs.Msg = "拒绝访问!!";
            }
            //todo:暂时屏蔽已注册的帐号多次发送验证码
            else
            {
                var us = uCore.LoadEntity(o => o.U_LoginPhone == phone);
                if (us != null && us.U_BuildingId != Guid.Empty)
                {
                    rs.Msg = "此帐号已经存在";
                    return WebApiJsonResult.ToJson(rs);
                }
                //没有选小区的帐号，都删除
                if (us != null && us.U_BuildingId == Guid.Empty)
                {
                    HuanXin.RemoveUser(us.U_ChatID);
                    uCore.DeleteEntity(us);
                }

                int sendCount = 0;//发送次数
                int.TryParse(CacheHelper.GetCacheString("code" + phone), out sendCount);
                if (sendCount > 5) //超过发送次数，防止机器人频繁触发
                {
                    rs.Msg = "验证码发送已超过5次，明日再试";
                }
                else
                {

                    //rs.State = 0;
                    //rs.Msg = "ok,测试返回验证码：123456";
                    ////todo:发验证码测试
                    //rs.Data = new { phone = phone, code = DESProvider.EncryptString("123456"), time = DateTime.Now.ToString(), sendCount = sendCount };

                    //sendCount = sendCount + 1;
                    //CacheHelper.SetCache("code" + phone, sendCount, DateTime.Now.AddDays(1));  //发送记录
                    //return WebApiJsonResult.ToJson(rs);



                    string code = new CreateRandomStr().GetRandomString(6);
                    sms sms = new sms();
                    sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                        phone, string.Format(ConfigurationManager.AppSettings["SmsRegTmp"], code)); //发送短消息


                    sendCount = sendCount + 1;
                    CacheHelper.SetCache("code" + phone, sendCount, DateTime.Now.AddDays(1));  //发送记录
                    CacheHelper.SetCache("reg_code" + phone, DESProvider.EncryptString(code));  //记录验证码
                    logCore.AddEntity(new Sys_OperateLog() { L_Desc = string.Format("注册发送短信验证码。验证码：{0},当天发送次数{1}", code, sendCount), L_DriverType = pla, L_Flag = (int)ModuleEnum.发短信, L_Phone = phone, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/User/SendSMS", L_Time = DateTime.Now });  //记录操作日志

                    rs.State = 0;
                    rs.Msg = "ok";
                    //todo:此处日后，可以服务端生成加密的标识，注册入库，服务端再次验证是否合法DESProvider.EncryptString(code) 
                    //rs.Data = new { phone = phone, code = "", time = DateTime.Now.ToString(), sendCount = sendCount };
                    if (pla == (int)DriversEnum.Android)
                    {
                        rs.Data = new { phone = phone, code = "", time = DateTime.Now.ToString(), sendCount = sendCount };//code= DESProvider.EncryptString(code)
                    }
                    else
                    {
                        rs.Data = new { phone = phone, code = DESProvider.EncryptString(code), time = DateTime.Now.ToString(), sendCount = sendCount };//code= DESProvider.EncryptString(code)
                    }


                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 找回密码，发送短信
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ResetPwdSendSMS([FromBody]JObject value)
        {
            dynamic oc = value;
            string phone = oc.phone;
            ResultViewModel rs = new ResultViewModel();
            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            int pla = 0;
            //号码合法，并且号码存在
            if (!phone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            //todo:暂时屏蔽已注册的帐号多次发送验证码
            //else if (uCore.LoadEntity(o => o.U_LoginPhone == phone) != null)
            //{
            //    rs.Msg = "此帐号已经存在";
            //} 
            else if (!int.TryParse(dr, out pla))
            {
                rs.Msg = "拒绝访问!!";
            }
            else
            {
                var resetUser = uCore.LoadEntity(o => o.U_LoginPhone == phone && o.U_Status != (int)UserStatusEnum.冻结);
                if (resetUser != null)
                {
                    int sendCount = 0;//发送次数
                    int.TryParse(CacheHelper.GetCacheString("resetPwdCode" + phone), out sendCount);
                    if (sendCount > 5) //超过发送次数，防止机器人频繁触发
                    {
                        rs.Msg = "验证码发送已超过5次，明日再试";
                    }
                    else
                    {
                        ////rs.State = 0;
                        ////rs.Msg = "ok,忘记密码验证码测试：123456";
                        //////todo:发验证码测试
                        ////rs.Data = new { uid = resetUser.U_Id, phone = phone, code = DESProvider.EncryptString("123456"), time = DateTime.Now.ToString(), sendCount = sendCount };

                        ////return WebApiJsonResult.ToJson(rs);



                        string code = new CreateRandomStr().GetRandomString(6);
                        sms sms = new sms();
                        var smsrs = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                            phone, string.Format(ConfigurationManager.AppSettings["SmsRegTmp"], code)); //发送短消息


                        sendCount = sendCount + 1;
                        CacheHelper.SetCache("resetPwdCode" + phone, sendCount, DateTime.Now.AddDays(1));  //发送记录
                        CacheHelper.SetCache("reset_code" + phone, DESProvider.EncryptString(code));
                        logCore.AddEntity(new Sys_OperateLog() { L_Desc = string.Format("忘记密码发送短信验证码。验证码：{0},当天发送次数{1}", code, sendCount), L_DriverType = pla, L_Flag = (int)ModuleEnum.发短信, L_Phone = phone, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/User/ResetPwd", L_Time = DateTime.Now });  //记录操作日志

                        rs.State = 0;
                        rs.Msg = "ok";
                        //todo:此处日后，可以服务端生成加密的标识，注册入库，服务端再次验证是否合法

                        if (pla == (int)DriversEnum.Android)
                        {
                            rs.Data = new { phone = phone, code = "", time = DateTime.Now.ToString(), sendCount = sendCount, uid = resetUser.U_Id };//code= DESProvider.EncryptString(code)
                        }
                        else
                        {
                            rs.Data = new { phone = phone, code = DESProvider.EncryptString(code), time = DateTime.Now.ToString(), sendCount = sendCount, uid = resetUser.U_Id };//code= DESProvider.EncryptString(code)
                        }

                    }
                }
                else
                {
                    rs.Msg = "手机帐号已被冻结或不存在！";
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public HttpResponseMessage ResetPwd([FromBody] JObject ob)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = ob;
            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            int pla = 0;
            if (!int.TryParse(dr, out pla))  //header 简单验证
            {
                rs.Msg = "拒绝访问";
            }
            else
            {
                string enPwd = obj.newPassword;
                string enCode = obj.code;
                string token = obj.callback;

                if (!string.IsNullOrEmpty(token))  //token不对
                {
                    dynamic tk = JsonSerialize.Instance.JsonToObject(token);

                    string codeTime = tk.time;
                    if (Convert.ToDateTime(codeTime).AddMinutes(30) < DateTime.Now)  //验证码已经超时了
                    {
                        rs.State = 2;
                        rs.Msg = "验证码超时";
                    }
                    else if (enCode.Equals(CacheHelper.GetCacheString("reset_code" + tk.phone)))  //合法
                    {
                        Guid uid = tk.uid;//重置会员的ID
                        string newPwd = DESProvider.DecryptString(enPwd);
                        if (uCore.ResetPwd(uid, newPwd))  //用户重置密码
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                        }
                        else
                        {
                            rs.Msg = "重置密码失败";
                        }
                    }
                    else
                    {
                        rs.Msg = "非法操作";
                    }
                }
                else
                {
                    rs.Msg = string.Format("token验证的口令失败,token={0} ", token);
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 新用户注册
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Reg([FromBody] JObject ob)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = ob;
            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            int pla = 0;
            if (!int.TryParse(dr, out pla))
            {
                rs.Msg = "拒绝访问";
            }
            else
            {

                string code = obj.code;
                string phone = obj.phone;
                string token = obj.callback;
                string nickname = obj.nickname;
                string coodX = obj.coordX;
                string coodY = obj.coordY;
                if (!string.IsNullOrEmpty(token))
                {
                    dynamic tk = JsonSerialize.Instance.JsonToObject(token);

                    string codeTime = tk.time;
                    if (Convert.ToDateTime(codeTime).AddMinutes(30) < DateTime.Now)  //验证码已经超时了
                    {
                        rs.State = 2;
                        rs.Msg = "验证码超时";
                    }
                    else if (code.Equals(CacheHelper.GetCacheString("reg_code" + tk.phone)) && tk.phone == phone)  //合法
                    {
                        string pwdCode = new CreateRandomStr().GetRandomString(4);//登录密码干扰码
                        string p = obj.pwd;  //客户端加密过
                        string pwd = Md5Extensions.MD5Encrypt(DESProvider.DecryptString(p) + pwdCode);

                        string area = "重庆";  //注册地
                        string areaCode = "22001";//区域码
                        string aName = obj.areaName;
                        if (!string.IsNullOrEmpty(aName))  //根据注册地定位到对应市、区域码
                        {
                            aName = aName.Replace("市", "");
                            CityCore cCore = new CityCore();
                            var city = cCore.LoadEntity(o => o.C_Name == aName + "市");
                            if (city != null)
                            {
                                area = city.C_Name.Replace("市", "");
                                areaCode = city.C_LevelCode;
                            }
                        }
                        DbGeography location = null;
                        try
                        {
                            location = DbGeography.FromText(string.Format("POINT({0} {1})", coodX, coodY), 4326);
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        Core_User user = new Core_User()
                        {
                            U_Age = 0,
                            U_City = area,
                            U_AreaCode = areaCode,
                            U_AuditingManager = 0,
                            U_AuditingState = 0,
                            U_Birthday = "",
                            U_BuildingId = Guid.Empty,
                            U_BuildingName = "",
                            U_ChatID = "",
                            U_CoordX = Convert.ToDouble(coodY),
                            U_CoordY = Convert.ToDouble(coodX),
                            U_Id = Guid.NewGuid(),
                            U_LoginPhone = phone,
                            U_LoginPwd = pwd,
                            U_Logo = "",
                            U_NickName = nickname,
                            U_PwdCode = pwdCode,
                            U_RegisterDate = DateTime.Now,
                            U_RegisterSource = pla,
                            U_Sex = 2,
                            U_Signatures = "",
                            U_Status = 0,
                            U_LastLoginTime = DateTime.Now,

                            U_Location = location
                        };

                        if (uCore.Reg(user, area))  //用户注册
                        {
                            rs.State = 0;
                            rs.Msg = "ok";


                            TokenViewModel vm = new TokenViewModel() { Phone = user.U_LoginPhone, Platform = pla, Time = DateTime.Now, Uid = user.U_Id.ToString(), AreaCode = user.U_AreaCode };

                            string refToken = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(vm));  //登录口令

                            // rs.Data = JsonSerialize.Instance.ObjectToJson(new { token = refToken});

                            rs.Data =
                            new
                            {
                                logo = ConfigurationManager.AppSettings["ImgSiteUrl"] + user.U_Logo,
                                huanxin = "lx_" + user.U_Id,
                                nickName = user.U_NickName,
                                trueName = user.U_TrueName,
                                sex = user.U_Sex,
                                age = user.U_Age,
                                birthday = user.U_Birthday,
                                signatures = user.U_Signatures,
                                buildingId = user.U_BuildingId,
                                buildingName = user.U_BuildingName,
                                state = user.U_Status,
                                auditingState = user.U_AuditingState,
                                areaCode = user.U_AreaCode,
                                areaName = user.U_City,
                                regTime = user.U_RegisterDate,
                                phone = user.U_LoginPhone,
                                uid = user.U_Id,
                                token = refToken
                            };


                        }
                        else
                        {
                            rs.Msg = "注册失败";
                        }
                        //uCore.AddEntity(new Core_User() {U_Age});
                    }
                    else
                    {
                        rs.Msg = "验证码错误";
                    }
                }
                else
                {
                    rs.Msg = string.Format("token验证的口令失败,code={0},phone={1},token={2}", code, phone, token);
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 用户入驻，注意：不是认证
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public HttpResponseMessage Entering()
        {
            VillageCore vCore = new VillageCore();
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());


            Guid buildingId;
            Guid.TryParse(HttpContext.Current.Request.Form["buildingId"], out buildingId);

            string trueName = HttpContext.Current.Request.Form["trueName"];
            string buildingName = HttpContext.Current.Request.Form["buildingName"];


            var us = uCore.LoadEntity(o => o.U_Id == uid && o.U_BuildingId == Guid.Empty && o.U_AuditingState == (int)AuditingEnum.未认证);


            if (us != null)
            {
                us.U_BuildingId = buildingId;
                us.U_BuildingName = buildingName;
                us.U_TrueName = trueName;

                if (uCore.UpdateEntity(us))
                {
                    rs.State = 0;
                    rs.Msg = "ok";


                    //群聊天ID
                    string villChatId = "";
                    string villLogo = "";//小区LOGO
                    if (buildingId != Guid.Empty)
                    {
                        var vill = vCore.LoadEntity(o => o.V_Id == buildingId);
                        if (vill != null)
                        {
                            if (string.IsNullOrEmpty(vill.V_ChatID))  //小区还没有环信群
                            {
                                //创建群
                                string chatid = HuanXin.CreateQun("group_" + vill.V_Id);
                                vill.V_ChatID = chatid;
                                if (vCore.UpdateEntity(vill))
                                {
                                    villChatId = chatid;
                                }
                            }
                            else
                            {
                                villChatId = vill.V_ChatID;
                            }
                            villLogo = vill.V_Img;
                        }

                        // 需求变更，没有实名认证的用户也可以加入到环信群
                        HuanXin.AccountQunJoin(villChatId, us.U_ChatID);
                        //villChatId = vCore.LoadEntity(p => p.V_Id == buildingId).V_ChatID;
                    }

                    rs.Data =
                    new
                    {
                        logo = "",
                        huanxin = "",
                        nickName = us.U_NickName,
                        trueName = us.U_TrueName,
                        sex = us.U_Sex,
                        signatures = us.U_Signatures,
                        buildingId = us.U_BuildingId,
                        buildingName = us.U_BuildingName,
                        state = us.U_Status,
                        auditingState = us.U_AuditingState,
                        auditingManager = us.U_AuditingManager,
                        areaCode = us.U_AreaCode,
                        areaName = us.U_City,
                        regTime = us.U_RegisterDate,
                        phone = us.U_LoginPhone,
                        buildingChatId = villChatId,
                        buildingLogo = villLogo,
                        uid = us.U_Id
                    };

                    #region 新用户注册，若没有通过用户认证成功，推送:课堂、认证流程

                    //JPushMsgModel jm1 = new JPushMsgModel()
                    //{
                    //    code = (int)MessageCenterModuleEnum.邻妹妹,
                    //    proFlag = (int)PushMessageEnum.社区活动跳转,
                    //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //    tags = "邻信玩法",
                    //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //    tips = "邻妹妹30秒玩转了邻信，都来试试看还能更快点吗？",
                    //    title = "邻信课堂开课啦",
                    //    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/ways/index.html",
                    //    proName = "才30秒，能慢点吗？"
                    //};
                    //Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), us.U_Id.ToString("N").ToLower());



                    //JPushMsgModel jm2 = new JPushMsgModel()
                    //{
                    //    code = (int)MessageCenterModuleEnum.邻妹妹,
                    //    proFlag = (int)PushMessageEnum.社区活动跳转,
                    //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //    tags = "用户认证",
                    //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //    tips = "第一款基于自家小区的邻居实时通讯的交友平台",
                    //    title = "邻信提醒您，您还未进行社区认证",
                    //    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/authflow/index.html",
                    //    proName = "社区认证真实交友新玩法"
                    //};
                    //Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm2.title, jm2.title, JsonSerialize.Instance.ObjectToJson(jm2), us.U_Id.ToString("N").ToLower());


                    #endregion
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Login([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            //return WebApiJsonResult.ToJson(rs);
            int platform = int.Parse(Request.Headers.GetValues("platform").FirstOrDefault());
            dynamic obj = value;

            string phone = obj.phone;
            double coodX = obj.coordX;
            double coodY = obj.coordY;
            string cityName = obj.cityName;

            UserCore uCore = new UserCore();
            var user = uCore.LoadEntity(o => o.U_LoginPhone == phone);
            if (user == null || user.U_BuildingId == Guid.Empty)
            {
                rs.Msg = "用户不存在,请注册！";
            }
            else if (user.U_Status == (int)UserStatusEnum.冻结)
            {
                rs.Msg = "帐号已被冻结";
            }
            else
            {
                string enpwd = obj.pwd;
                string pwd = DESProvider.DecryptString(enpwd);

                string enPwd = Md5Extensions.MD5Encrypt(pwd + user.U_PwdCode);
                if (user.U_LoginPwd == enPwd) //密码正确
                {
                    user.U_LastLoginTime = DateTime.Now;

                    if (Math.Abs(coodX) > 0 && Math.Abs(coodY) > 0)  //定位成功
                    {

                        try
                        {
                            user.U_Location = DbGeography.FromText(string.Format("POINT({0} {1})", coodX, coodY), 4326);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    user.U_CoordY = coodX;
                    user.U_CoordX = coodY;


                    TokenViewModel vm = new TokenViewModel()
                    {
                        Phone = user.U_LoginPhone,
                        Platform = platform,
                        Time = DateTime.Now,
                        Uid = user.U_Id.ToString(),
                        AreaCode = user.U_AreaCode
                    };

                    string token = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(vm));

                    //添加操作日志
                    Sys_OperateLog log = new Sys_OperateLog()
                    {
                        L_Desc = string.Format("登录成功#登录城市：{2}，具体位置：{0},{1}", coodX, coodY, cityName),
                        L_DriverType = user.U_RegisterSource,
                        L_Flag = (int)ModuleEnum.登录,
                        L_Phone = user.U_LoginPhone,
                        L_Status = 0,
                        L_Time = DateTime.Now,
                        L_UId = user.U_Id,
                        L_Url = "/User/Login"
                    };
                    new OperateLogCore().AddEntity(log);




                    string villChatId = ""; //小区环信ID
                    string villLogo = ""; //小区LOGO
                    var vill = villCore.LoadEntity(o => o.V_Id == user.U_BuildingId);
                    if (vill != null)
                    {
                        if (string.IsNullOrEmpty(vill.V_ChatID)) //小区还没有环信群
                        {
                            //创建群
                            string chatid = HuanXin.CreateQun("group_" + vill.V_Id);
                            vill.V_ChatID = chatid;
                            if (villCore.UpdateEntity(vill))
                            {
                                villChatId = chatid;
                            }
                        }
                        else
                        {
                            villChatId = vill.V_ChatID;
                        }
                        villLogo = vill.V_Img;
                    }



                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        logo = ConfigurationManager.AppSettings["ImgSiteUrl"] + user.U_Logo,
                        huanxin = user.U_ChatID,
                        nickName = user.U_NickName,
                        trueName = user.U_TrueName,
                        sex = user.U_Sex,
                        age = user.U_Age,
                        birthday = user.U_Birthday,
                        signatures = user.U_Signatures,
                        buildingId = user.U_BuildingId,
                        buildingName = user.U_BuildingName,
                        state = user.U_Status,
                        auditingState = user.U_AuditingState,
                        auditingManager = user.U_AuditingManager,
                        areaCode = user.U_AreaCode,
                        areaName = user.U_City,
                        regTime = user.U_RegisterDate,
                        phone = user.U_LoginPhone,
                        uid = user.U_Id,
                        buildingChatId = villChatId,
                        buildingLogo = ConfigurationManager.AppSettings["ImgSiteUrl"] + villLogo,
                        blockGroupMsg = user.U_BlockGroupMsg,
                        token = token
                    };
                    //return WebApiJsonResult.ToJson(new ResultViewModel() { State = 0, Msg = "ok", Data = token });

                    if (user.U_AuditingState == (int)AuditingEnum.未认证 || user.U_AuditingState == (int)AuditingEnum.认证失败)
                    {

                        #region 新用户注册，若没有通过用户认证成功，推送:课堂、认证流程

                        //JPushMsgModel jm1 = new JPushMsgModel()
                        //{
                        //    code = (int)MessageCenterModuleEnum.邻妹妹,
                        //    proFlag = (int)PushMessageEnum.社区活动跳转,
                        //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tags = "邻信玩法",
                        //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tips = "邻妹妹30秒玩转了邻信，都来试试看还能更快点吗？",
                        //    title = "邻信课堂开课啦",
                        //    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/ways/index.html",
                        //    proName = "才30秒，能慢点吗？"
                        //};
                        //Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), user.U_Id.ToString("N").ToLower());



                        //JPushMsgModel jm2 = new JPushMsgModel()
                        //{
                        //    code = (int)MessageCenterModuleEnum.邻妹妹,
                        //    proFlag = (int)PushMessageEnum.社区活动跳转,
                        //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tags = "用户认证",
                        //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tips = "第一款基于自家小区的邻居实时通讯的交友平台",
                        //    title = "邻信提醒您，您还未进行社区认证",
                        //    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/authflow/index.html",
                        //    proName = "社区认证真实交友新玩法"
                        //};
                        //Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm2.title, jm2.title, JsonSerialize.Instance.ObjectToJson(jm2), user.U_Id.ToString("N").ToLower());


                        #endregion
                    }


                    #region 九宫格 大抽奖返还奖励

                    //new PrizeDetailCore().BackAward(user.U_LoginPhone);

                    #endregion
                }
                else
                {
                    rs.Msg = "密码错误";
                }
            }



            return WebApiJsonResult.ToJson(rs);
        }
    }
}
