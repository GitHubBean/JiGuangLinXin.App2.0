using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Helpers;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.Auth;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.Rpg;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    public class AccountController : ApiController
    {
        private BusinessCore uCore = new BusinessCore();
        private AdminCore adCore = new AdminCore();
        /// <summary>
        /// 商家登录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Login([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            dynamic param = value;

            string uname = param.uname;
            string pwd = param.pwd;  //客户端加密
            int rememberMe = param.rememberMe;


            var admin = uCore.LoadEntity(o => o.B_LoginPhone == uname);

            if (admin == null)
            {
                rs.Msg = "商家不存在";
            }
            else if (admin.B_Status == (int)UserStatusEnum.冻结)
            {
                rs.Msg = "帐号已被冻结";
            }
            else
            {
                string enPwd = Md5Extensions.MD5Encrypt(pwd + admin.B_PwdCode);
                if (admin.B_LoginPwd == enPwd) //密码正确
                {
                    FormTicketHelper.SetCookie(admin.B_LoginPhone, string.Format("{0},{1}", admin.B_Role, admin.B_Id), rememberMe == 1, "bcenter_admin_");

                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        staticImgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"],
                        bid = admin.B_Id,
                        loginphone = admin.B_LoginPhone,
                        role = admin.B_Role,
                        logo =   admin.B_Logo,
                        nickname = admin.B_NickName,
                        buildingCount = admin.B_VillageCount,
                        sex = admin.B_Sex
                    };
                }
            }
            return rs;
        }


        /// <summary>
        /// 商家系统管理员登录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel AdminLogin([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            dynamic param = value;

            string uname = param.uname;
            string pwd = param.pwd;  //客户端加密
            int rememberMe = param.rememberMe;


            var admin = adCore.LoadEntity(o => o.A_Account == uname && o.A_Role == (int)ManagerRoleEnum.商家管理员);

            if (admin == null)
            {
                rs.Msg = "帐号不存在";
            }
            else if (admin.A_Status == (int)UserStatusEnum.冻结)
            {
                rs.Msg = "帐号已被冻结";
            }
            else
            {
                string enPwd = Md5Extensions.MD5Encrypt(pwd + admin.A_EncryptCode);
                if (admin.A_Pwd == enPwd) //密码正确
                {
                    FormTicketHelper.SetCookie(admin.A_Account, string.Format("{0},{1}", admin.A_Role, admin.A_Id), rememberMe == 1, "bcenter_sysadmin_");

                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        staticImgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"],
                        bid = admin.A_Id,
                        loginphone = admin.A_Account,
                        role = admin.A_Role,
                        logo = admin.A_HeadImg,
                        nickname = admin.A_Name,
                        sex = admin.A_Sex
                    };
                }
            }
            return rs;
        }



        /// <summary>
        /// 重置密码发送短信
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ResetPwdSendSMS([FromBody]JObject value)
        {
            OperateLogCore logCore = new OperateLogCore();
            dynamic oc = value;
            ResultMessageViewModel rs = new ResultMessageViewModel();
            string phone = oc.phone;

            //号码合法
            if (!phone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            else
            {

                var resetUser = uCore.LoadEntity(o => o.B_LoginPhone == phone && o.B_Status != (int)UserStatusEnum.冻结);
                if (resetUser != null)
                {
                    int sendCount = 0;//发送次数
                    int.TryParse(CacheHelper.GetCacheString("ResetPwdCount" + phone), out sendCount);
                    if (sendCount > 6) //超过发送次数，防止机器人频繁触发
                    {
                        rs.Msg = "验证码发送已超过5次，明日再试";
                    }
                    else
                    {
                        ////todo:发验证码测试
                        //rs.State = 0;
                        //rs.Msg = "ok,忘记密码验证码测试：123456";

                        //rs.Data = new { uid = resetUser.U_Id, phone = phone, code = DESProvider.EncryptString("123456"), time = DateTime.Now.ToString(), sendCount = sendCount };

                        //return WebApiJsonResult.ToJson(rs);



                        string code = new CreateRandomStr().GetRandomString(6);
                        sms sms = new sms();
                        sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                            phone, string.Format(ConfigurationManager.AppSettings["SmsRegTmp"], code)); //发送短消息


                        sendCount = sendCount + 1;
                        CacheHelper.SetCache("ResetPwdCount" + phone, sendCount, DateTime.Now.AddDays(1));  //发送记录
                        CacheHelper.SetCache("ResetPwdCode" + phone, code, DateTime.Now.AddDays(1));  //缓存发送的验证码

                        logCore.AddEntity(new Sys_OperateLog() { L_Desc = string.Format("商家忘记登录密码，发送短信验证码。验证码：{0},当天发送次数{1}", code, sendCount), L_DriverType = 0, L_Flag = (int)ModuleEnum.发短信, L_Phone = phone, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/Account/ResetPwdSendSMS", L_Time = DateTime.Now });  //记录操作日志

                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new { phone = phone, code = DESProvider.EncryptString(code), time = DateTime.Now.ToString(), sendCount = sendCount };
                    }
                }
                else
                {
                    rs.Msg = "帐号已被冻结或不存在！";
                }
            }
            return rs;
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public ResultMessageViewModel ResetPwd([FromBody] JObject ob)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = ob;

            string enPwd = obj.newPassword;
            string enCode = obj.code;
            string phone = obj.phone;


            if (!enCode.Equals(CacheHelper.GetCache("ResetPwdCode" + phone)))  //验证码
            {
                rs.State = 2;
                rs.Msg = "验证码错误";
            }
            else  //合法
            {
                var user = uCore.LoadEntity(o => o.B_LoginPhone == phone && o.B_Status != (int)UserStatusEnum.冻结);
                user.B_LoginPwd = Md5Extensions.MD5Encrypt(enPwd + user.B_PwdCode); //密码

                if (uCore.UpdateEntity(user))  //用户重置密码
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
                else
                {
                    rs.Msg = "重置密码失败";
                }
            }
            return rs;
        }
    }
}
