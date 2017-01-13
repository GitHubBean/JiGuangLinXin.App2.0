using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 新盘推荐
    /// </summary>
    public class BuildingController : BaseAdminController
    {

        private BuildingCore bdCore = new BuildingCore();


        private ActivityApplyCore applyCore = new ActivityApplyCore();
        /// <summary>
        /// 添加新盘推荐
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Core_Building bd = new Core_Building();

            bd.B_Id = Guid.NewGuid();
            bd.B_AdminId = obj.adminId;
            bd.B_BusPhone = obj.busPhone;
            bd.B_BusName = obj.busName;
            bd.B_Name = obj.title;
            bd.B_CityName = obj.cityName;
            bd.B_DistrictName = obj.districtName;
            bd.B_Area = obj.area;
            bd.B_Rooms = obj.rooms;
            bd.B_Price = obj.price;
            bd.B_Tags = obj.tags;
            bd.B_CovereImg = obj.coverImg;
            bd.B_Logo = obj.logo;
            bd.B_VideoImg = obj.videoImg;
            bd.B_Video = obj.videoUrl;
            bd.B_IsHot = obj.isHot;
            bd.B_IsNew = obj.isNew;
            bd.B_IsThrift = obj.isThrift;
            bd.B_Top = obj.top;
            bd.B_Desc = obj.desc;
            bd.B_Address = obj.address;
            bd.B_TargetCity = obj.targetCity;
            bd.B_AdTitle = obj.adTitle;
            bd.B_AdUrl = obj.adUrl;
            bd.B_HongbaoCount = obj.hbCount;
            bd.B_HongbaoMoney = obj.hbMoney;
            bd.B_HongbaoRemain = obj.hbCount;
            bd.B_HongbaoTips = obj.hbTips;
            bd.B_BTime = obj.STime;
            bd.B_ETime = obj.ETime;
            bd.B_ActivityContent = obj.activityContent;
            bd.B_ActivityImg = obj.activityImg;
            bd.B_ActivityTitle = obj.activityTitle;
            bd.B_Flag = obj.flag;

            bd.B_CellsCount = 0;
            bd.B_Clicks = 0;
            bd.B_Status = 0;
            //bd.B_Flag = 0;
            bd.B_Recom = 0;
            bd.B_Time = DateTime.Now;

            //基础设置
            BuildingViewModel vm = new BuildingViewModel()
            {
                landscape = obj.landscape,
                location = obj.location,
                planning = obj.planning,
                property = obj.property
            };

            bd.B_Content = JsonSerialize.Instance.ObjectToJson(vm);

            IEnumerable<dynamic> images = obj.images;  //楼盘各种附件图片

            IEnumerable<dynamic> cubes = obj.cubes;  //楼盘 全景户型

            if (bdCore.Add(bd, images, cubes))  //发布成功了
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    buildingId = bd.B_Id
                };
            }

            return rs;
        }
        /// <summary>
        /// 发布的楼盘列表
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

            Expression<Func<Core_Building, Boolean>> exp = t => true;  //筛选条件


            if (state > -1)
            {
                exp = exp.And(o => o.B_Status == state);
            }
            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.B_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.B_Time < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.B_Name.Contains(title));
            }
            var list = bdCore.LoadEntities(exp).OrderByDescending(o => o.B_Top).ThenByDescending(o => o.B_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                o.B_Id,
                o.B_Name,
                o.B_Address,
                o.B_BTime,
                o.B_ETime,
                o.B_Status,
                o.B_Tags,
                o.B_Time,
                o.B_BusName,
                B_BusId = o.B_AdminId,
                o.B_BusPhone
                
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
        /// 楼盘活动申请历史记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ApplyHistory([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid bdId = obj.bdId;
            string title = obj.title;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_ActivityApply, Boolean>> exp = t => t.AA_ActivityId == bdId;  //筛选条件

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.AA_Remark.Contains(title));
            }

            var list =
                applyCore.LoadEntities(exp)
                    .OrderByDescending(o => o.AA_Time)
                    .Skip(pn * rows)
                    .Take(rows)
                    .ToList()
                    .Select(o => new
                    {
                        aId = o.AA_Id,
                        phone = o.AA_UPhone,
                        uid = o.AA_UId,
                        remark = o.AA_Remark  // 真实姓名#小区名
                    });
            if (list.Any())
            {
                rs.Data = list;
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.Msg = "暂无数据";
            }
            return rs;
        }

        /// <summary>
        ///  楼盘下架
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid bdId = obj.bdId;
            int state = obj.state;

            var info = bdCore.LoadEntity(o => o.B_Id == bdId);
            if (info != null)
            {
                info.B_Status = state; //标识1下架、2删除

                if (bdCore.UpdateEntity(info)) //下架
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



    }
}
