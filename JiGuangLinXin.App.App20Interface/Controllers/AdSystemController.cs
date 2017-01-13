using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// 平台所有广告位置
    /// </summary>
    public class AdSystemController : ApiController
    {
        private AdSystemCore adCore = new AdSystemCore();
        string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            int placeId = obj.placeId;
            Guid buildingId = obj.buildingId;

            //如果查询本小区的定向广告没有，就查询全平台广告
            Sys_AdSystem ad1 =
                adCore.LoadEntity(
                    o =>
                        o.A_Status == 0 && o.A_Place == placeId && o.A_Building.Contains(buildingId.ToString()) &&
                        o.A_ETime > DateTime.Now &&
                        o.A_STime < DateTime.Now) ??
                adCore.LoadEntity(
                    o => o.A_Status == 0 && o.A_Place == placeId && o.A_Target == 0 && o.A_ETime > DateTime.Now &&
                         o.A_STime < DateTime.Now);

            if (ad1 != null)  //广告存在
            { 
                int flag = -1;  //-1不跳转 0跳转url 1跳转到商家服务 2跳转到精品汇 
                if (!string.IsNullOrEmpty(ad1.A_Linkurl))
                {
                    flag = 0;
                }
                else if (ad1.A_ProjectId != Guid.Empty && ad1.A_ProjectId != null)
                {
                    flag = 2;
                }
                else if (ad1.A_BusId != Guid.Empty)
                {
                    flag = 1;
                }

                rs.Data = new
                {
                    aid = ad1.A_Id,
                    target = ad1.A_Target,
                    title = ad1.A_Title,
                    imgUrl = StaticHttpUrl + ad1.A_ImgUrl,
                    desc = ad1.A_Desc,
                    httpUrl = ad1.A_Linkurl,
                    tags = ad1.A_Tags,
                    proId = ad1.A_ProjectId,
                    busId = ad1.A_BusId,
                    flag

                };
            }
            else  //采用系统默认广告图  精品汇频道页=2,社区服务频道页  =3,消息频道页 = 4
            {

                rs.Data = new
                {
                    aid = 0,
                    target =0,
                    title = "",
                    imgUrl = StaticHttpUrl + "ad/"+placeId+".jpg",
                    desc = "",
                    httpUrl ="",
                    tags = "",
                    proId = "",
                    busId = "",
                    flag = -1

                };
            }
            return WebApiJsonResult.ToJson(rs);
        }

    }
}
