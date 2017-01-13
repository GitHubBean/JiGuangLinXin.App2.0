using System;
using System.Collections.Generic;
using System.Configuration;
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
using JiGuangLinXin.App.Provide.Rpg;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 商家入驻申请
    /// </summary>
    public class EnteringApplyController : ApiController
    {

        //静态文件的物理地址
        private string staticFilePath = ConfigurationManager.AppSettings["StaticFilePath"];
        //图片URL
        private string staticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

        private OperateLogCore logCore = new OperateLogCore();
        private BusinessCore busCore = new BusinessCore();
        private BusinessEnteringApplyCore baCore = new BusinessEnteringApplyCore();
        /// <summary>
        /// 第一步：录入基本信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Step1([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string code = obj.code;
            string trueName = obj.trueName;
            string cardId = obj.cardId;
            string password = obj.password;
            string phone = obj.phone;

            int pla = 3;

            string serviceCode = CacheHelper.GetCacheString("applyCode" + phone);
            if (!code.Equals(serviceCode)) //正确
            {
                rs.Msg = "验证码不正确";
            }
            else
            {
                string pwdCode = new CreateRandomStr().GetRandomString(4);//登录密码干扰码

                string pwd = Md5Extensions.MD5Encrypt(password + pwdCode);
                Core_Business bus = new Core_Business()
                {
                    B_Id = Guid.NewGuid(),
                    B_LoginPhone = phone,
                    B_LoginPwd = pwd,
                    B_PwdCode = pwdCode,
                    B_Role = (int)MemberRoleEnum.商家,
                    B_Sex = 2,
                    B_Age = 0,
                    B_RegisterSource = pla,
                    B_AuditingState = (int)AuditingEnum.未认证,
                    B_Status = 0,
                    B_Level = 0,
                    B_VillageCount = 0,
                    B_RegisterDate = DateTime.Now,
                    B_Category = 0,
                    B_IsAuthentic = 0,
                    B_IsFamous = 0,
                    B_IsFree = 0,
                    B_IsHot = 0,
                    B_IsReturns = 0,
                    B_CardId = cardId,
                    B_TrueName = trueName
                };

                if (busCore.AddEntity(bus) != null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        businessId = bus.B_Id
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        [System.Web.Http.HttpOptions]
        public string Options()
        {

            return null; 

        }
        /// <summary>
        /// 第二部，提交审核信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Step2([FromBody] JObject value)
        {

            ResultViewModel rs = new ResultViewModel(1, "申请失败", null);
            dynamic obj = value;

            Guid busId = obj.busId;
            var busInfo = busCore.LoadEntity(o => o.B_Id == busId);
            if (busInfo.B_AuditingState == (int)AuditingEnum.未认证)
            {
                Core_BusinessEnteringApply baInfo = baCore.LoadEntity(o => o.BA_BusId == busId && o.BA_State == (int)AuditingEnum.认证中);
                if (baInfo == null)
                {
                    string img1 = obj.img1;
                    string img2 = obj.img2;
                    string img3 = obj.img3;
                    string img4 = obj.img4;

                    if (!File.Exists(staticFilePath + img1))
                    {
                        rs.Msg = "请上传商家LOGO";
                    }
                    else if (!File.Exists(staticFilePath + img2))
                    {
                        rs.Msg = "请上传商家法定人手持身份证照片";
                    }
                    else if (!File.Exists(staticFilePath + img3))
                    {
                        rs.Msg = "请上传商家工商营业执照";
                    }
                    else if (!File.Exists(staticFilePath + img4))
                    {
                        rs.Msg = "请上传商家行业代码证";
                    }
                    else
                    {

                        baInfo = new Core_BusinessEnteringApply()
                        {
                            BA_Address = obj.address,
                            BA_AreaCode = "",
                            BA_BuildingName = obj.buildingName,
                            BA_BusId = busId,
                            BA_BusLicenseImg = img3,
                            BA_CardImg = img2,
                            BA_Category = 0,
                            BA_City = obj.city,
                            BA_Logo = img1,
                            BA_NickName = obj.nickname,
                            BA_OrganizationCodeImg = img4,
                            BA_State = (int)AuditingEnum.认证中,
                            BA_Time = DateTime.Now,
                            BA_Phone = busInfo.B_LoginPhone,
                            BA_CardId = busInfo.B_CardId,
                            BA_TrueName = busInfo.B_TrueName,
                            
                            
                        };
                        if (baCore.AddEntity(baInfo) != null) //提交申请
                        {
                            busInfo.B_AuditingState = (int)AuditingEnum.认证中;
                            busInfo.B_NickName = baInfo.BA_NickName;
                            busInfo.B_Logo = baInfo.BA_Logo;
                            busInfo.B_Address = baInfo.BA_City + " " + baInfo.BA_Address;

                            if (busCore.UpdateEntity(busInfo))
                            {
                                rs.State = 0;
                                rs.Msg = "ok";
                            }
                        }
                    }
                }
                else
                {
                    rs.Msg = "您已经提交入驻申请，请耐心等待！";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }




        /// <summary>
        /// 检测验证码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage CheckSmsCode([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(1, "验证码不正确", null);
            dynamic obj = value;
            string phone = obj.phone;
            string code = obj.code;
            string serviceCode = CacheHelper.GetCacheString("applyCode" + phone);

            if (code.Equals(serviceCode))  //正确
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage SendSms([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            string phone = obj.phone;
            int pla = 3;
            //号码合法，并且号码未被注册
            if (!phone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            //todo:暂时屏蔽已注册的帐号多次发送验证码
            else if (1 != 1)
            {
                rs.Msg = "此帐号已经存在";
            }
            else
            {
                int sendCount = 0; //发送次数
                int.TryParse(CacheHelper.GetCacheString("applyCount" + phone), out sendCount);
                if (sendCount > 5) //超过发送次数，防止机器人频繁触发
                {
                    rs.Msg = "验证码发送已超过5次，明日再试";
                }
                else
                {


                    sendCount = sendCount + 1;
                    CacheHelper.SetCache("applyCount" + phone, sendCount, DateTime.Now.AddDays(1));  //发送次数
                    CacheHelper.SetCache("applyCode" + phone, "123456", DateTime.Now.AddDays(1));  //缓存发送的验证码


                    rs.State = 0;
                    rs.Msg = "ok,测试返回验证码：123456";
                    //todo:发验证码测试
                    rs.Data = new { phone = phone, code = "123456", time = DateTime.Now.ToString(), sendCount = sendCount };

                    return WebApiJsonResult.ToJson(rs);


                    string code = new CreateRandomStr().GetRandomString(6);
                    sms sms = new sms();
                    sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                        phone, string.Format(ConfigurationManager.AppSettings["SmsRegTmp"], code)); //发送短消息


                    sendCount = sendCount + 1;
                    CacheHelper.SetCache("applyCount" + phone, sendCount, DateTime.Now.AddDays(1));  //发送次数
                    CacheHelper.SetCache("applyCode" + phone, code, DateTime.Now.AddDays(1));  //缓存发送的验证码

                    logCore.AddEntity(new Sys_OperateLog() { L_Desc = string.Format("商家入驻申请，发送短信验证码。验证码：{0},当天发送次数{1}", code, sendCount), L_DriverType = pla, L_Flag = (int)ModuleEnum.发短信, L_Phone = phone, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/User/ResetPwd", L_Time = DateTime.Now });  //记录操作日志

                    rs.State = 0;
                    rs.Msg = "ok";

                    //todo:此处日后，可以服务端生成加密的标识，注册入库，服务端再次验证是否合法
                    rs.Data =
                        new
                        {
                            phone = phone,
                            code,//= DESProvider.EncryptString(code),
                            time = DateTime.Now.ToString(),
                            sendCount = sendCount
                        };
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        public string Get([FromBody]string value)
        {
            return value;
        }



    }
}
