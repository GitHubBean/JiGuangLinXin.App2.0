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
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 小区接口
    /// </summary>
    public class VillageController : ApiController
    {
        private VillageCore vCore = new VillageCore();

        /// <summary>
        /// 根据城市名字，获得选中城市下面的热门小区（默认10条）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage VillageList([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            string cName = obj.cityName;
            string vName = obj.vName;
            int isHot = obj.isHot;
            if (string.IsNullOrEmpty(cName))
            {
                rs.State = 1;
                rs.Msg = "请先选择城市";
            }
            else
            {
                cName = cName.Replace("市", "");

                //分页查询
                int pn;
                int rows;

                if (!int.TryParse(HttpContext.Current.Request.Form["pn"], out pn))
                {
                    pn = 0;
                }
                else
                {
                    pn = pn - 1;
                }
                if (!int.TryParse(HttpContext.Current.Request.Form["rows"], out rows))
                {
                    rows = 10;
                }

                //var list = vCore.GetHotList(cName, vName).Select(o => new
                //{
                //    vId = o.V_Id,
                //    vName = o.V_BuildingName,
                //    vCity = o.V_CityName,
                //    coordX = o.V_CoordX,
                //    coordY = o.V_CoordY
                //});
                dynamic list;
                if (isHot == 0 && !string.IsNullOrEmpty(vName))  //模糊查询 小区
                {
                    var sel =
                      vCore.LoadEntities(o => o.V_CityName.Contains(cName) && o.V_BuildingName.Contains(vName))
                          .OrderByDescending(o => o.V_Hot)
                          .ThenByDescending(o => o.V_Number).Skip(pn * rows)
                          .Take(rows).ToList()
                          .Select((o, i) => new
                          {
                              vId = o.V_Id,
                              vName = o.V_BuildingName,
                              vCity = o.V_CityName,
                              coordX = o.V_CoordX,
                              coordY = o.V_CoordY,
                              number = o.V_Number > 500 ? o.V_Number : 0,
                              star = 0 //i > 8 ? 3.5 : (i > 4 ? 4 : (i > 1 ? 4.5 : 5))
                          });

                    if (sel.Any())
                    {
                        list = sel;
                    }
                    else  //如果没有搜索出来结果，就返回默认
                    {
                        //todo:如果小区没有检索出来，暂时屏蔽不使用默认小区
                        //list = new List<object>()
                        //{
                        //    new
                        //    {
                        //        vId ="",
                        //        vName = vName,
                        //        vCity = cName,
                        //        coordX = 0,
                        //        coordY = 0,
                        //        number = 0,
                        //    }
                        //};
                        list = null;
                        rs.State = 1;
                        rs.Msg = "没有更多数据";
                    }
                }
                else  //查询热门
                {


                    list =
                      vCore.LoadEntities(o => o.V_CityName.Contains(cName))
                          .OrderByDescending(o => o.V_Hot)
                          .ThenByDescending(o => o.V_Number).Skip(pn * rows)
                          .Take(rows).ToList()
                          .Select((o, i) => new
                          {
                              vId = o.V_Id,
                              vName = o.V_BuildingName,
                              vCity = o.V_CityName,
                              coordX = o.V_CoordX,
                              coordY = o.V_CoordY,
                              number = o.V_Number > 500 ? o.V_Number : 0,
                              star =0  // i > 8 ? 3.5 : (i > 4 ? 4 : (i > 1 ? 4.5 : 5))
                          });
                }
                rs.Data = list;
                //rs.Data = JsonSerialize.Instance.ObjectToJson(list);                
            }

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
