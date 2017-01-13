using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Provide.Common;
using JiGuangLinXin.App.Services.weixin;
using JiguangLinXin.App.SpecialEvent.Core;
using JiguangLinXin.App.SpecialEvent.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;

namespace JiguangLinXin.App.SpecialEvent.Controllers
{
    /// <summary>
    /// 投票
    /// </summary>
    public class VoteController : ApiController
    {
        private VillageCore vCore = new VillageCore();
        private VillageVoteCore vvCore = new VillageVoteCore();
        private VillageApplyCore vaCore = new VillageApplyCore();
        private StatisticsCore sCore = new StatisticsCore();

        public HttpResponseMessage Test()
        {
            ResultViewModel rs = new ResultViewModel();

            var vill = vCore.LoadEntities().FirstOrDefault();

            //dynamic info = UserAdminAPI.GetInfo();
            rs.Data = vill;
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 发起投票
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Send([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            string ip = NetHelper.GetIP();

            var ipCounts =
                vvCore.LoadEntities(
                    o =>
                        o.C_Ip == ip && o.C_Time.Year == DateTime.Now.Year && o.C_Time.Month == DateTime.Now.Month &&
                        o.C_Time.Day == DateTime.Now.Day).Count() > 9;
            if (ipCounts)
            {
                rs.Msg = "今日投票次数已经用完";

            }
            else if (vvCore.Send(buildingId, ip))
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
             
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 投票结果排序
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Rank([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            int pn = obj.pn;
            int rows = obj.rows;

            pn = pn - 1;
            var list = vCore.LoadEntities().OrderByDescending(o => o.V_Votes);
            var pageList = list.Skip(pn * rows).Take(rows).ToList().Select(o => new
            {
                bid = o.V_Id,
                //rankNo = i,
                votes = o.V_Votes,
                title = o.V_BuildingName,
                cityName = o.V_CityName

            });
            if (list.Any())
            {
                rs.State = 0;
                rs.Data = new { totalCount = list.Count(), list = pageList };
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 查询自家小区投票情况
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Query([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            string bName = obj.buildingName;

            string cityName = obj.cityName;
            string disName = obj.disName;


            var vi = vCore.LoadEntities(o => o.V_BuildingName.Contains(bName.Trim()) && o.V_CityName == cityName && disName == o.V_DistrictName).OrderByDescending(o => o.V_Votes).Take(50);  //查询所有的小区

            List<dynamic> rsList = new List<dynamic>();
            if (vi.Any())
            {
                foreach (var item in vi)
                {

                    var rank = vCore.LoadEntities(o => o.V_Votes > item.V_Votes).Count() + 1;
                    rsList.Add(new
                    {
                        rank,
                        bid = item.V_Id,
                        //rankNo = i,
                        votes = item.V_Votes,
                        title = item.V_BuildingName,
                        cityName = item.V_CityName
                    });
                }
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = rsList;
            }
            else
            {
                rs.Msg = "小区不存在";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 判断小区是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage IsExist([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string buildingName = obj.buildingName;
            string cityName = obj.cityName;
            string disName = obj.disName;

            var vi =
                vCore.LoadEntity(
                    o => o.V_BuildingName == buildingName && o.V_CityName == cityName && disName == o.V_DistrictName);
            if (vi != null)
            {
                rs.Msg = "小区已存在";
            }
            else
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 申请创建
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ApplyAdd([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            string buildingName = obj.buildingName;
            string cityName = obj.cityName;
            string disName = obj.disName;

            var vi =
                vCore.LoadEntity(
                    o => o.V_BuildingName.Contains(buildingName.Trim()) && o.V_CityName == cityName && disName == o.V_DistrictName);
            if (vi != null)
            {
                rs.Msg = "小区已存在";
            }
            else
            {
                Core_VillageApply mod = new Core_VillageApply()
                {
                    A_ApplyName = buildingName,
                    A_CityName = cityName,
                    A_DistrictName = disName,
                    A_Id = Guid.NewGuid(),
                    A_State = 0,
                    A_Time = DateTime.Now
                };
                //申请成功
                if (vaCore.AddEntity(mod) != null)
                {
                    rs.Msg = "ok";
                    rs.State = 0;
                }
                else
                {
                    rs.Msg = "申请失败";
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 累计浏览量
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage View()
        {
            ResultViewModel rs = new ResultViewModel();

            var obj = sCore.LoadEntity();
            obj.S_Views += 1;
            var r = sCore.UpdateEntity(obj);
            if (r)
            {
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "累计浏览量异常";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 统计数量
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Statistics()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);


            var sta = sCore.LoadEntity();

            int voteCount = sta.S_Votes;
            int viewCount = sta.S_Views;
            int villageCout = sta.S_VillageCount;

            rs.Data = new
            {
                voteCount,
                viewCount,
                villageCout
            };

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
