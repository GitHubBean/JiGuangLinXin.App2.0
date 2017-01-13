using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiguangLinXin.App.SpecialEvent.Core;
using JiguangLinXin.App.SpecialEvent.Models;
using Webdiyer.WebControls.Mvc;

namespace JiguangLinXin.App.SpecialEvent.Areas.ControlCenter.Controllers
{
    public class VillageLinxinController : BaseController
    {

        private ProvinceCore pCore = new ProvinceCore();
        private CityCore cCore = new CityCore();
        private DistrictCore dCore = new DistrictCore();
        private VillageCore vlCore = new VillageCore();
        /// <summary>
        /// 邻信 官方 录入的 小区信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string province, string areaCity, string areaDistrict, int id = 1, int size = 10, string key = "")
        {
            TempData["ProvinceList"] = GetProvinceList();


            Expression<Func<Core_Village, Boolean>> expr = t => true;

            Expression<Func<Core_Village, Boolean>> expr1 = t => true;



            if (!string.IsNullOrWhiteSpace(key))
            {
                expr1 = t => t.V_BuildingName.Contains(key.Trim());
            }

            if (!string.IsNullOrEmpty(areaDistrict) && "0" != areaDistrict)
            {
                var dis = dCore.LoadEntity(o => o.D_LevelCode == areaDistrict);
                //expr = t => t.V_AreaCode.StartsWith(areaDistrict);
                expr = t => dis.D_Name.Contains(t.V_DistrictName);
            }
            else if (!string.IsNullOrEmpty(areaCity) && "0" != areaCity)
            {
                var city = cCore.LoadEntity(o => o.C_LevelCode == areaCity);
                //expr = t => t.V_AreaCode.StartsWith(areaCity);

                expr = t => city.C_Name.Contains(t.V_CityName);
            }
            else if (!string.IsNullOrEmpty(province) && "0" != province)
            {

                expr = t => t.V_AreaCode.StartsWith(province);
            }

            ViewBag.TotalCount = vlCore.LoadEntities().Count();

            var list = vlCore.LoadEntities(expr).Where(expr1).OrderByDescending(o => o.V_Time).ThenBy(o => o.V_CityName).ThenByDescending(o => o.V_Hot).ToPagedList(id, size);
            ViewBag.CurrentCount = vlCore.LoadEntities(expr).Where(expr1).Count();

            //ViewBag.TotalCount = vlCore.LoadEntities().Count();
            //var where = PredicateBuilder.True<Core_Village>();
            //if (!string.IsNullOrWhiteSpace(key))
            //{
            //    where.And(t => t.V_BuildingName.Contains(key.Trim()));
            //}


            //if (!string.IsNullOrEmpty(areaDistrict) && "0" != areaDistrict)
            //{
            //    where.And(t => t.V_AreaCode.StartsWith(areaDistrict));
            //}
            //else if (!string.IsNullOrEmpty(areaCity) && "0" != areaCity)
            //{
            //    where.And(t => t.V_AreaCode.StartsWith(areaCity));
            //}
            //else if (!string.IsNullOrEmpty(province) && "0" != province)
            //{

            //    where.And(t => t.V_AreaCode.StartsWith(province));
            //}
            //var list1 = vlCore.LoadEntities(where).OrderByDescending(o => o.V_Time).ThenBy(o => o.V_CityName).ThenByDescending(o => o.V_Hot).ToPagedList(id, size);
            return View(list);
        }

        /// <summary>
        /// 编辑小区
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(Guid? id)
        {
            Core_Village obj = null;

            if (id.HasValue)
            {
                obj = vlCore.LoadEntity(o => o.V_Id == id);
            }
            else
            {
                TempData["ProvinceList"] = GetProvinceList();

                obj = new Core_Village();
            }
            return View(obj);
        }


        /// <summary>
        /// 保存楼盘编辑信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Core_Village obj)
        {
            obj.V_Time = DateTime.Now;

            if (obj.V_Id == Guid.Empty) //新增
            {
                obj.V_Id = Guid.NewGuid();
                vlCore.AddEntity(obj);
            }
            else //修改
            {
                vlCore.UpdateEntity(obj);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 获得所有城市
        /// </summary>
        /// <returns></returns>
        public string GetAllProvinceList()
        {
            var cityList = GetProvinceList();
            return JsonSerialize.Instance.ObjectToJson(cityList);
        }
        [HttpPost]
        public string GetCityListByProvince(string areaCode)
        {
            var cityList = GetCityList(areaCode);
            return JsonSerialize.Instance.ObjectToJson(cityList);
        }
        [HttpPost]
        public string GetDistrictListByCity(string areaCode)
        {

            var disList = GetDistrictList(areaCode);
            return JsonSerialize.Instance.ObjectToJson(disList);
        }
    }
}
