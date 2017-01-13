using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.Rpg;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers.Prize
{
    /// <summary>
    /// 小区管理员中心
    /// </summary>
    public class GroupManagerCenterController : ApiController
    {
        private VillageCore vcore = new VillageCore();
        private UserCore ucore = new UserCore();
        private EventCore ecore = new EventCore();

        string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];
        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "非法请求", null);
            dynamic obj = value;
            Guid aid = obj.aid; //业主ID

            var user = ucore.LoadEntity(o => o.U_Id == aid && o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_AuditingState == (int)AuditingEnum.认证成功 && o.U_Status == 0);
            if (user == null)
            {
                return WebApiJsonResult.ToJson(rs);
            }
            Guid bid = user.U_BuildingId;

            string bname = "";
            string blogo = "";
            int newUser = 0;
            int oftenUser = 0;
            decimal incomeByAll = 1000;
            //decimal incomeByMonth = 0;

            var binfo = vcore.LoadEntity(o => o.V_Id == bid && o.V_State == 0);
            if (binfo == null)
            {
                return WebApiJsonResult.ToJson(rs);
            }
            bname = binfo.V_BuildingName;
            blogo = StaticHttpUrl + binfo.V_Img;

            var users = ucore.LoadEntities(o => o.U_BuildingId == bid).ToList();
            newUser = users.Count(o => o.U_RegisterDate.Year == DateTime.Now.Year && o.U_RegisterDate.Month == DateTime.Now.Month);
            oftenUser = users.Count(o => o.U_LastLoginTime > DateTime.Now.AddMonths(-2));   //活跃用户，统计进两个月登录的用户

            //List<dynamic> eventRs = new List<dynamic>();

            //活动：星际闯关活动 、 邻信电影季--这个ID写死的，因为特么的不同活动就是一个不同的模块设计的表数据都不一样
            Guid id1 = Guid.Parse("7AB7F487-5D3A-4683-981B-DEFD2E81E77D");
            Guid id2 = Guid.Parse("8F1FD5C1-86D9-40BD-B4BC-484745BAB6A6");

            var joinCount1 = new PrizeDetailCore().JoinCountByBuildingId(user.U_BuildingId);
            var joinCount2 = new ScoreExchangeCore().LoadEntities(o => o.E_BuildingId == user.U_BuildingId && o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票).Count();

            var el =
                ecore.LoadEntities(
                    o =>
                        o.E_Id == id1 ||
                        o.E_Id == id2).ToList();
            List<dynamic> eventList = new List<dynamic>();
            foreach (var item in el)
            {
                eventList.Add(new
                {
                    eid = item.E_Id,
                    title = item.E_Title,
                    logo = StaticHttpUrl + item.E_Img,
                    stime = item.E_STime,
                    etime = item.E_ETime,
                    state = item.E_Status,
                    remark = "",
                    flag = item.E_Flag,
                    type = 0,
                    joinCount = item.E_Id == id1 ? joinCount1 : joinCount2
                });
            }
            rs.State = 0;
            rs.Msg = "ok";
            rs.Data = new
            {
                bname,
                blogo = string.IsNullOrEmpty(binfo.V_Img) ? "http://static.cqjglx.com/building/u.png" : blogo,
                newUser,
                oftenUser,
                income = new
                {
                    total = incomeByAll,
                    items = new List<dynamic>()
                    {
                        new { id = 1, name = "引流收入", value = 20 },
                        new { id = 2, name= "社区活跃度收入", value= 50 },
                        new { id = 3, name = "活动收入", value= 10 },
                    }
                },
                star = 5,
                eventList
            };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 活动图表统计
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage EventChart([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "非法请求", null);
            dynamic obj = value;
            Guid aid = obj.aid; //业主ID
            Guid eid = obj.eid; //活动ID

            var user = ucore.LoadEntity(o => o.U_Id == aid && o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_AuditingState == (int)AuditingEnum.认证成功 && o.U_Status == 0);
            if (user == null)
            {
                return WebApiJsonResult.ToJson(rs);
            }
            Guid bid = user.U_BuildingId;
            /*"7AB7F487-5D3A-4683-981B-DEFD2E81E77D"
             *"8F1FD5C1-86D9-40BD-B4BC-484745BAB6A6"*/

            dynamic rsdata = null;
            string title = "";
            if (eid == Guid.Parse("7AB7F487-5D3A-4683-981B-DEFD2E81E77D")) //星际闯关
            {
                //PrizeDetailCore pcore = new PrizeDetailCore();
                //var rsjson = CacheHelper.GetCacheString("star" + eid);
                //if (string.IsNullOrEmpty(rsjson))
                //{
                //    Random rdm = new Random(Guid.NewGuid().GetHashCode());
                //    List<dynamic> tmp = new List<dynamic>()
                //    {
                //        new {item = 1,count = rdm.Next(0,5)},
                //        new {item = 2,count = rdm.Next(0,6)},
                //        new {item = 4,count = rdm.Next(0,10)},
                //        new {item = 5,count = 0},
                //        new {item = 6,count = rdm.Next(0,8)},
                //    };
                //    rsjson = JsonSerialize.Instance.ObjectToJson(tmp);
                //    CacheHelper.SetCache("star" + eid, rsjson);
                //}

                //rsdata = JsonSerialize.Instance.JsonToObject("star" + eid);

                rsdata = new PrizeDetailCore().JoinCountDetailByBuildingId(user.U_BuildingId);

                title = "星际闯关活动";

            }

            if (eid == Guid.Parse("8F1FD5C1-86D9-40BD-B4BC-484745BAB6A6")) //电影季活动
            {
                rsdata =
                    new ScoreExchangeCore().LoadEntities(
                        o => o.E_BuildingId == bid && o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票)
                        .GroupBy(o => o.E_Flag).ToList()
                        .Select((o,i) => new
                        {
                            item = i,
                            title = Enum.GetName(typeof(FilmFlagEnum), o.Key),
                            count = o.Count()
                        });
                title = "电影季活动";

            }

            if (rsdata != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    title,
                    json = rsdata
                };
            }


            return WebApiJsonResult.ToJson(rs);
        }


    }
}
