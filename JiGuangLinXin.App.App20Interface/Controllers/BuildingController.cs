using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.LambdaExtension;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 楼盘管理、新家推荐
    /// </summary>
    public class BuildingController : ApiController
    {
        private BuildingCore bCore = new BuildingCore();
        private AttachmentCore attCore = new AttachmentCore();
        string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

        /// <summary>
        /// 新家推荐列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage List([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;
            string cityName = obj.cityName;
            int orderBy = obj.orderBy;

            int pn = obj.pn;
            pn = pn - 1;
            int rows = obj.rows;

            //Expression<Func<Core_Building, Boolean>> exp = o => o.B_Time == 0;

            var list = bCore.LoadEntities(o => o.B_Status == 0 && (o.B_TargetCity.Contains(cityName) || o.B_TargetCity.Contains("全国")));

            if (orderBy == 1)  //热销
            {
                list = list.OrderByDescending(o => o.B_IsHot).ThenByDescending(o => o.B_Time); ;
            }
            else if (orderBy == 2)  //特价
            {
                list = list.OrderByDescending(o => o.B_IsThrift).ThenByDescending(o => o.B_Time);

            }
            else if (orderBy == 3)  //新盘
            {
                list = list.OrderByDescending(o => o.B_IsNew).ThenByDescending(o => o.B_Time);
            }
            else
            {
                list = list.OrderByDescending(o => o.B_Time);
            }

            list = list.Skip(pn * rows).Take(rows);

            var db = list.ToList().Select(o => new
            {
                bid = o.B_Id,
                coverImg = StaticHttpUrl + o.B_CovereImg,
                title = o.B_Name,
                price = o.B_Price,
                areas = o.B_Area,
                rooms = o.B_Rooms,
                address = o.B_Address,
                tags = o.B_Tags.Split(',').Take(5).Select(t => new
                {
                    str = t.ToString()
                })
            });
            if (db.Any())
            {
                rs.Data = db;
                rs.State = 0;
                rs.Msg = "ok";
            }
            else
            {
                rs.State = 1;
                rs.Msg = "没有更多数据!";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 楼盘展示
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Show([FromBody]JObject value)
        {
            var rs = new ResultViewModel();
            dynamic obj = value;
            Guid bId = obj.bId;

            var info = bCore.LoadEntity(o => o.B_Id == bId && o.B_Status == 0);

            if (info != null)
            {
                rs.State = 0;
                rs.Msg = "ok";

                dynamic content = JsonSerialize.Instance.JsonToObject<dynamic>(info.B_Content);  //基础信息内容

                BuidingCubeCore bcCore = new BuidingCubeCore();

                //全景看房
                var cubeList = bcCore.LoadEntities(o => o.BC_Status == 0 && o.BC_BuildingId == info.B_Id).OrderBy(o => o.BC_Rank).ToList().Select(o => new
                {
                    cubeId = o.BC_Id,
                    title = o.BC_Title,
                    coverImg = StaticHttpUrl + o.BC_CoverImg,
                    imgList = attCore.LoadEntities(a => a.A_PId == o.BC_Id).OrderBy(b => b.A_Rank).ToList().Select(c => new
                    {
                        imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                    })
                });
                //加载楼盘所有附件图片

                var attList = attCore.LoadEntities(o => o.A_PId == info.B_Id).ToList();


                //景观漫游
                var landscapeImg =
                    attList.Where(o => o.A_Folder == "landscape")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });

                //区位展示
                var locationImg =
                    attList.Where(o => o.A_Folder == "location")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });
                //建筑规划
                var planningImg =
                    attList.Where(o => o.A_Folder == "planning")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });


                //物业配套
                var propertyImg =
                    attList.Where(o => o.A_Folder == "property")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });


                rs.Data = new
                {
                    bid = info.B_Id,
                    title = info.B_Name,
                    coverImg = StaticHttpUrl + info.B_CovereImg,
                    videoImg = StaticHttpUrl + info.B_VideoImg,
                    videoUrl = StaticHttpUrl + info.B_Video,
                    desc = info.B_Desc,

                    adTitle = info.B_AdTitle,
                    adUrl = info.B_AdUrl,
                    adCartoon = string.IsNullOrEmpty(info.B_AdCartoonImg) ? "" : StaticHttpUrl + info.B_AdCartoonImg,
                    adBgImg = string.IsNullOrEmpty(info.B_AdBgImg) ? "" : StaticHttpUrl + info.B_AdBgImg,

                    hongbao = info.B_HongbaoRemain,

                    landscape = content.landscape,
                    landscapeImg,

                    location = content.location,
                    locationImg,

                    property = content.property,
                    propertyImg,

                    planning = content.planning,
                    planningImg,

                    cubeList,

                    linkPhone = info.B_BusPhone,

                };
            }
            else
            {
                rs.Msg = "数据不存在";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 活动申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Apply([FromBody]JObject value)
        {
            dynamic obj = value;
            Guid bId = obj.bId;  //楼盘ID
            Guid uId = obj.uId;  //用户ID

            //提交申请
            var rs = bCore.Apply(bId, uId);

            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 楼盘活动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Activity([FromBody]JObject value)
        {
            dynamic obj = value;
            Guid bId = obj.bId;  //楼盘ID
            Guid uId = obj.uId;  //用户ID

            var rs = bCore.Activity(bId, uId);
            rs.Data.coverImg = StaticHttpUrl + rs.Data.coverImg;
            if (rs.State == 0 && rs.Data.hbFlag == 1)  //本次领到了红包
            {

                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "楼盘红包",
                    title = "您收到一个楼盘红包",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您收到楼盘【" + rs.Data.buildingName + "】的红包，" + rs.Data.hbMoney + "元",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uId.ToString("N").ToLower());

                #endregion
            }

            return WebApiJsonResult.ToJson(rs);
        }


    }
}
