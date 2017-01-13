using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 邻里圈商家广告位
    /// </summary>
    public class ShowcaseController : BaseAdminController
    {
        private ShowcaseCore scCore = new ShowcaseCore();
        /// <summary>
        /// 添加广告位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            string gid = obj.S_GoodsId;
            string bid = obj.S_BuildingId;

            Core_Showcase info = new Core_Showcase();
            info.S_BusId = obj.S_BusId;
            info.S_BusName = obj.S_BusName;
            info.S_BusLogo = obj.S_BusLogo;
            info.S_BusRole = obj.S_BusRole;
            info.S_Phone = obj.S_Phone;
            info.S_Title = obj.S_Title;
            info.S_Img = obj.S_Img;
            info.S_Flag = obj.S_Flag;
            info.S_Video = obj.S_Video;
            info.S_STime = obj.S_STime;
            info.S_ETime = obj.S_ETime;
            info.S_Desc = obj.S_Desc;
            info.S_Address = obj.S_Address;
            info.S_GoodsName = obj.S_GoodsName;
            info.S_GoodsId = string.IsNullOrEmpty(gid) ? Guid.Empty : Guid.Parse(gid);
            info.S_BuildingId = string.IsNullOrEmpty(bid) ? Guid.Empty : Guid.Parse(bid);
            info.S_BuildingName = obj.S_BuildingName;
            info.S_TargetUrl = obj.S_TargetUrl;
            info.S_Target = obj.S_Target;

            info.S_Hongbao = 0;
            info.S_Likes = 0;
            info.S_Comments = 0;
            info.S_Rank = 0;
            info.S_Clicks = 0;
            info.S_Top = 0;
            info.S_Status = 0;
            info.S_Date = DateTime.Now;
            info.S_Id = Guid.NewGuid();

           
            int hbCount = obj.hbCount;
            decimal hbMoney = obj.hbMoney;
            if (hbCount > 0 && hbMoney > 0)
            {
                info.S_Hongbao = 1;  //含有红包
            }
           
            IEnumerable<dynamic> buildings = obj.buildings;  //如果发布到定向小区
            if (buildings != null)
            {
                info.S_Target = (int)AdTargetEnum.小区定向广告;
            }

            rs = scCore.AddOne(info, hbCount, hbMoney, buildings);
            if (rs.State == 0)
            {
                rs.Data = new
                {
                    sid = info.S_Id
                };
            }
            return rs;
        }
        /// <summary>
        /// 编辑广告位,只能更改基本信息，不能修改红包、发布的范围
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid scId = obj.S_Id;

            var sc = scCore.LoadEntity(o => o.S_Id == scId);
            if (sc != null)
            {
                sc.S_Title = obj.S_Title;
                sc.S_Img = obj.S_Img;
                sc.S_Flag = obj.S_Flag;
                sc.S_Video = obj.S_Video;
                sc.S_STime = obj.S_STime;
                sc.S_ETime = obj.S_ETime;
                sc.S_Desc = obj.S_Desc;
                sc.S_Address = obj.S_Address;
                sc.S_GoodsName = obj.S_GoodsName;
                sc.S_GoodsId = string.IsNullOrEmpty(obj.S_GoodsId) ? Guid.Empty : Guid.Parse(obj.S_GoodsId);
                sc.S_BuildingId = string.IsNullOrEmpty(obj.S_BuildingId) ? Guid.Empty : Guid.Parse(obj.S_BuildingId);
                sc.S_BuildingName = obj.S_BuildingName;
                sc.S_TargetUrl = obj.S_TargetUrl;


                if (scCore.UpdateEntity(sc))
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
        /// 下架、删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value; 
            Guid scId = obj.scId;
            int state = obj.state; //1下架 2删除


            var info = scCore.LoadEntity(o => o.S_Status == 0 && o.S_Id == scId);
            if (info != null && state > 0)
            {
                info.S_Status = state; //标识下架

                if (scCore.UpdateEntity(info)) //下架
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

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_Showcase, Boolean>> exp = t => t.S_Status==0;  //筛选条件
            if (state > 0)
            {
                exp = exp.And(o => o.S_Status == state);
            }

            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.S_STime > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.S_ETime < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.S_Title.Contains(title));
            }

            var list = scCore.LoadEntities(exp).OrderByDescending(o => o.S_Date).Skip(pn * rows).Take(rows).ToList().Select(o => new
            {
                oid = o.S_Id,
                img = StaticHttpUrl + o.S_Img,
                time = o.S_Date,

                title = o.S_Title,
                desc = o.S_Desc,
                flag = Enum.GetName(typeof(ShowcaseFlagEnum), o.S_Flag),
                busName = o.S_BusName,
                busPhone = o.S_Phone,
                stime = o.S_STime,
                etime = o.S_ETime,
                remark = o.S_Remark,

                target = o.S_Target,
                clicks = o.S_Clicks,
                commoents = o.S_Comments
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


        /// <summary>
        /// 推广位详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid scId = obj.scId;

            var sc = scCore.LoadEntity(o => o.S_Id == scId);
            if (sc != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    sc.S_Id,
                    sc.S_Title,
                    sc.S_Img,
                    sc.S_Flag,
                    sc.S_Video,
                    sc.S_STime,
                    sc.S_ETime,
                    sc.S_Desc,
                    sc.S_Address,
                    sc.S_GoodsId,
                    sc.S_GoodsName,
                    sc.S_BuildingId,
                    sc.S_BuildingName,
                    sc.S_Target,
                    sc.S_TargetUrl
                };
            }
            return rs;
        }


    }
}
