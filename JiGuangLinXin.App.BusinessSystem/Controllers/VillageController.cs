using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.BusinessSystem.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 用户小区
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
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            string city = obj.city;
            string district = obj.district;
            string buildingName = obj.buildingName;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_Village, Boolean>> exp = t => t.V_State == 0;
            if (!string.IsNullOrEmpty(city))  //城市
            {
                exp = exp.And(o => o.V_CityName.Contains(city));
            }
            if (!string.IsNullOrEmpty(district))  //地区
            {
                exp = exp.And(o => o.V_DistrictName.Contains(district));
            }
            if (!string.IsNullOrEmpty(buildingName))  //小区名
            {
                exp = exp.And(o => o.V_BuildingName.Contains(buildingName));
            }

            rs.Data =
                villCore.LoadEntities(exp)
                    .OrderByDescending(o => o.V_Hot)
                    .ThenByDescending(o => o.V_Number)
                    .Skip(pn * rows)
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
