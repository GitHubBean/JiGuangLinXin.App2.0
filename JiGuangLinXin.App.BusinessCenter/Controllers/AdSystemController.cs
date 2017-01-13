using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.BusinessCenter.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{

    /// <summary>
    /// 频道页的广告发布
    /// </summary>
    public class AdSystemController : BaseAdminController
    {
        private AdSystemCore adCore = new AdSystemCore();
        /// <summary>
        /// 发布广告
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Sys_AdSystem ad = new Sys_AdSystem();
            string pid = obj.A_ProjectId;
            ad.A_Place = obj.A_Place;
            ad.A_Target = obj.A_Target;
            ad.A_Type = obj.A_Type;
            ad.A_Building = obj.A_Building;
            ad.A_STime = obj.A_STime;
            ad.A_ETime = obj.A_ETime;
            ad.A_Title = obj.A_Title;
            ad.A_ImgUrl = obj.A_ImgUrl;
            ad.A_Desc = obj.A_Desc;
            ad.A_Linkurl = obj.A_Linkurl;
            ad.A_BusId = obj.A_BusId;
            ad.A_BusName = obj.A_BusName;
            ad.A_BusRole = obj.A_BusRole;
            ad.A_Phone = obj.A_Phone;
            ad.A_ProjectName = obj.A_ProjectName;
            ad.A_ProjectId = string.IsNullOrEmpty(pid) ? Guid.Empty : Guid.Parse(pid);
            ad.A_Remark = obj.remark;

            ad.A_Status = 0;
            ad.A_Time = DateTime.Now;
            ad.A_Clicks = 0;
            if (adCore.AddEntity(ad) != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            return rs;
        }

        /// <summary>
        /// 编辑频道页面广告内容
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int adId = obj.adId;
            Sys_AdSystem info = adCore.LoadEntity(o => o.A_Id == adId);
            if (info != null)
            {

                info.A_Place = obj.A_Place;
                info.A_Target = obj.A_Target;
                info.A_Type = obj.A_Type;
                info.A_Building = obj.A_Building;
                info.A_STime = obj.A_STime;
                info.A_ETime = obj.A_ETime;
                info.A_Title = obj.A_Title;
                info.A_ImgUrl = obj.A_ImgUrl;
                info.A_Desc = obj.A_Desc;
                info.A_Linkurl = obj.A_Linkurl;
                info.A_BusId = obj.A_BusId;
                info.A_BusName = obj.A_BusName;
                info.A_BusRole = obj.A_BusRole;
                info.A_Phone = obj.A_Phone;
                info.A_ProjectName = obj.A_ProjectName;
                info.A_ProjectId = obj.A_ProjectId;

                if (adCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }
        /// <summary>
        /// 查看频道广告的内容
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int adId = obj.adId;
            Sys_AdSystem info = adCore.LoadEntity(o => o.A_Id == adId);
            if (info != null)
            {
                rs.Data = new
                {
                    info.A_Id,
                    info.A_Place,
                    info.A_Target,
                    info.A_Type,
                    info.A_Building,
                    info.A_STime,
                    info.A_ETime,
                    info.A_Title,
                    info.A_ImgUrl,
                    info.A_Desc,
                    info.A_Linkurl,
                    info.A_BusId,
                    info.A_BusName,
                    info.A_BusRole,
                    info.A_Phone,
                    info.A_ProjectId,
                    info.A_ProjectName
                };
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }


        /// <summary>
        ///  下架、删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;


            int adId = obj.adId;
            int state = obj.state; //1下架 2删除


            var info = adCore.LoadEntity(o => o.A_Status == 0 && o.A_Id == adId);
            if (info != null && state > 0)
            {
                info.A_Status = state; //标识下架

                if (adCore.UpdateEntity(info)) //下架
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }


        /// <summary>
        /// 推广位列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel List([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int state = obj.state;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            string title = obj.title;
            int flag = obj.flag;
            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Sys_AdSystem, Boolean>> exp = t => true;  //筛选条件
            if (state > 0)
            {
                exp = exp.And(o => o.A_Status == state);
            }

            if (flag > 0)
            {
                exp = exp.And(o => o.A_Place == flag);
            }

            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.A_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.A_ETime < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.A_Title.Contains(title));
            }

            var list = adCore.LoadEntities(exp).OrderByDescending(o => o.A_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                oid = o.A_Id,
                img = StaticHttpUrl + o.A_ImgUrl,
                time = o.A_Time,

                title = o.A_Title,
                desc = o.A_Desc,

                busName = o.A_BusName,
                busPhone = o.A_Phone,
                stime = o.A_STime,
                etime = o.A_ETime,

                target = o.A_Target,
                clicks = o.A_Clicks,

            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list;
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }


    }
}
