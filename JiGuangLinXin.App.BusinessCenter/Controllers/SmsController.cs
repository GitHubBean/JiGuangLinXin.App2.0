using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    /// <summary>
    /// 短信发送
    /// </summary>
    public class SmsController : BaseController
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Post([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;
            string smsContent = obj.smsContent;
            string sendPhone = obj.sendPhone;


            if (!sendPhone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            else
            {
                sms sms = new sms();
                SubmitResult sr = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"], sendPhone, smsContent);

                Sys_OperateLog log = new Sys_OperateLog()
                {
                    L_Desc = "商家后台管理系统发送短信#内容：" + smsContent,
                    L_DriverType = (int)DriversEnum.BusinessCenter,
                    L_Flag = (int)ModuleEnum.发短信,
                    L_Phone = sendPhone,
                    L_Status = 0,
                    L_Time = DateTime.Now,
                    L_UId = busId,
                    L_Url = "/Sms/"
                };

                OperateLogCore logCore = new OperateLogCore();
                if (logCore.AddEntity(log)!=null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }


            return rs;
        }

    }
}
