using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    ///活动中心
    /// </summary>
    public class EventController : BaseController
    {
        private EventCore eventCore = new EventCore();
        //private EventViewCore evCore = new EventViewCore();
        /// <summary>
        /// 活动中心
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;
            string buildingId = obj.buildingId;  //社区uID
            IEnumerable<Core_Event> list;
            //if (string.IsNullOrEmpty(buildingId))  //查询官方活动
            //{
            //    list = eventCore.LoadEntities(o => o.E_Target == (int)MemberRoleEnum.平台 && o.E_Status == 0 && o.E_AuditingState == (int)AuditingEnum.认证成功);
            //}
            //else
            //{

            //    list = eventCore.GetBuildingEvents(Guid.Parse(buildingId));
            //}
            //得到所有平台的活动,E_Recom=0 标识要显示出来的  E_Recom=1 标识隐藏
            list = eventCore.LoadEntities(o => o.E_Target == (int)MemberRoleEnum.平台 && o.E_Status == 0 && o.E_AuditingState == (int)AuditingEnum.认证成功&&o.E_Recom==0).OrderByDescending(o=>o.E_Date).Skip(pn*rows).Take(rows);

            if (!string.IsNullOrEmpty(buildingId))  //查询社区活动
            {
                var buildingList = eventCore.GetBuildingEvents(Guid.Parse(buildingId));
                list = list.Union(buildingList);
            }
            
            list = list.OrderBy(o => o.E_Target).ThenBy(o => o.E_Rank).ThenByDescending(o=>o.E_Date);  //按照发布平台排序（平台活动在前，商家活动在后）、再按照排序值排序


            var rsList =
                       list.Skip(pn * rows)
                            .Take(rows).ToList().Select(o => new
                            {
                                eid = o.E_Id,
                                converImg = StaticHttpUrl + o.E_Img,
                                title = o.E_Title,
                                role = o.E_BusRole,
                                isTop = o.E_Top,
                                datetime = o.E_Date,
                                shareCallback = o.E_Remark,  //分享链接之后，回调地址

                                flag = o.E_Flag,
                                targetUrl = o.E_TargetUrl,
                                proId = o.E_GoodsId
                            });
            rs.Data = rsList;
            return WebApiJsonResult.ToJson(rs);
        }

    }
}
