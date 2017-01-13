using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 社区群
    /// </summary>
    public class CommunityController : BaseController
    {
        private AuditingGroupManangerCore audCore = new AuditingGroupManangerCore();
        /// <summary>
        /// 群主申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public  HttpResponseMessage GroupManagerApply([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string trueName = obj.trueName;
            string linkPhone = obj.linkPhone;
            string tips = obj.tips;
            Guid buildingId = obj.buildingId;
            string buildingName =obj.buildingName;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();


            Core_AuditingGroupMananger aud = new Core_AuditingGroupMananger()
            {
                M_BuildingId = buildingId,
                M_BuildingName =  buildingName,
                M_CheckBack = "",
                M_CheckTime = null,
                M_Id = Guid.NewGuid(),
                M_Phone = linkPhone,
                M_QQ="",
                M_Remark = tips,
                M_Status = 0,
                M_Time = DateTime.Now,
                M_UId = uid,
                M_TrueName = trueName,
                M_UPhone = phone
            };
            if (audCore.AddEntity(aud)!=null)
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            return WebApiJsonResult.ToJson(rs);
        }
    }
}
