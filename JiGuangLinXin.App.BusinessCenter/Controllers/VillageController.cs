using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    /// <summary>
    /// 业主小区
    /// </summary>
    public class VillageController : BaseController
    {
        private VillageCore villCore = new VillageCore();
        /// <summary>
        /// 根据城市地区，搜索小区
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel List([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0,"ok",null);
            dynamic obj = value;
            string city = obj.city;
            string district = obj.district;

            int pn = obj.pn;
            int rows = obj.rows;


            rs.Data=
                villCore.LoadEntities(
                    o => o.V_CityName == city && o.V_DistrictName == district || o.V_DistrictName.Contains(city + "周边"))
                    .OrderByDescending(o => o.V_Hot)
                    .ThenByDescending(o => o.V_Number)
                    .Skip(pn*rows)
                    .Take(rows)
                    .Select(o => new
                    {
                        vId = o.V_Id,
                        vName = o.V_BuildingName,
                        vAddress = o.V_BuildingAddress,
                        vNumber = o.V_Number
                    });
            return rs;
        }
    }
}
