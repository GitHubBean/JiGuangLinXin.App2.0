using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using JiGuangLinXin.App.AdminCenter.Extension.Filters;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Provide.Auth;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.Rpg;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 后台系统基类
    /// </summary>
    [Authorize]
    [LogErrorException]
    public class BaseController : Controller
    {

        #region 属性
        /// <summary>
        /// 访问URL 带有域名
        /// </summary>
        public string HostUrl { get; set; }

        /// <summary>
        /// 访问的URL， 不带域名
        /// </summary>
        public string RawUrl { get; set; }
        /// <summary>
        /// 上一次访问URL(来源地址)
        /// </summary>
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// 请求的IP
        /// </summary>
        public string IP { get; set; }
        #endregion
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Init(requestContext);
        }


        /// <summary>
        /// 获得当前登录到后台的管理员
        /// </summary>
        /// <returns></returns>
        public static Sys_Admin GetUser()
        {
            Sys_Admin obj = new Sys_Admin() { A_Id = Guid.Empty, A_Account = "匿名" };
            var data = FormTicketHelper.GetUserData();
            if (!string.IsNullOrEmpty(data))
            {
                string objStr = CookieHelper.GetCookieValue("manager");  //缓存取到当前登录的管理员
                if (string.IsNullOrEmpty(objStr))  //不存在
                {
                    string loginId = data.Split(',')[1];
                    Guid id;
                    if (Guid.TryParse(loginId, out id))
                    {
                        obj = new AdminCore().LoadEntity(o => o.A_Id == id);
                        CookieHelper.SetCookie("manager", JsonSerialize.Instance.ObjectToJson(obj), DateTime.Now.AddSeconds(30));
                    }
                }
                else
                {

                    obj = JsonSerialize.Instance.JsonToObject<Sys_Admin>(objStr);
                }
            }
            return obj;
        }


        /// <summary>
        /// 初始化一些元数据
        /// </summary>
        /// <param name="requestContext"></param>
        private void Init(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.Url != null) this.HostUrl = requestContext.HttpContext.Request.Url.ToString();
            this.ReferrerUrl = requestContext.HttpContext.Request.UrlReferrer != null ? requestContext.HttpContext.Request.UrlReferrer.ToString() : string.Empty;
            this.IP = requestContext.HttpContext.Request.UserHostAddress;
            this.RawUrl = requestContext.HttpContext.Request.RawUrl;

            ViewBag.RawUrl = RawUrl;//string.Format("/{0}/{1}", requestContext.RouteData.Values["controller"], requestContext.RouteData.Values["action"]); 

            var obj = GetUser();
            ViewBag.UName = obj.A_Account; //登录的用户名字
            ViewBag.Role = obj.A_Role;
            ViewBag.UId = obj.A_Id;

            ViewBag.StaticSiteUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

            ViewBag.SiteName = "邻信App2.0后台管理系统";
        }


        /// <summary>
        /// 获取所有 省 列表
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetProvinceList()
        {

            ProvinceCore pCore = new ProvinceCore();
            //如果是新增，加载省份列表
            var provinceList = pCore.LoadEntities().OrderBy(o => o.P_Pinyin).ToList();
            List<SelectListItem> pList = provinceList.Select(o => new SelectListItem
            {
                Text = o.P_Name,
                Value = o.P_LevelCode
            }).ToList();
            if (pList.Count > 0)
            {
                pList.Insert(0, new SelectListItem()
                {
                    Text = "==请选择==",
                    Value = "0"
                });
            }
            return pList;
        }

        /// <summary>
        /// 获取所有城市列表
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetCityList(string areaCode)
        {

            CityCore cCore = new CityCore();
            var provinceList = cCore.LoadEntities(o => o.C_LevelCode.StartsWith(areaCode)).OrderBy(o => o.C_Pinyin).ToList();
            List<SelectListItem> pList = provinceList.Select(o => new SelectListItem
            {
                Text = o.C_Name,
                Value = o.C_LevelCode
            }).ToList();
            if (pList.Count > 0)
            {
                pList.Insert(0, new SelectListItem()
                {
                    Text = "==请选择==",
                    Value = "0"
                });

            }
            return pList;
        }



        /// <summary>
        /// 获取所有 区县 列表
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetDistrictList(string areaCode)
        {

            DistrictCore dCore = new DistrictCore();
            var provinceList = dCore.LoadEntities(o => o.D_LevelCode.StartsWith(areaCode)).OrderBy(o => o.D_Pinyin).ToList();
            List<SelectListItem> pList = provinceList.Select(o => new SelectListItem
            {
                Text = o.D_Name,
                Value = o.D_LevelCode
            }).ToList();
            if (pList.Count > 0)
            {
                pList.Insert(0, new SelectListItem()
                {
                    Text = "==请选择==",
                    Value = "0"
                });
            }
            return pList;
        }



    }
}
