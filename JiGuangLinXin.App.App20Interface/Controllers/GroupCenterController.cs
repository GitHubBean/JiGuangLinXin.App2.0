using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class GroupCenterController : ApiController
    {
        private InteractiveCore iCore = new InteractiveCore();
        private BillboardCore nCore = new BillboardCore();
        private UserCore uCore = new UserCore();
        // private GroupbuyCore eventScopeCore = new GroupbuyCore();
        private BusinessServiceCore busVillCore = new BusinessServiceCore();
        private GroupAlbumPicCore picCore = new GroupAlbumPicCore();
        private HotlineCore hCOre = new HotlineCore();
        private VillageCore vCore = new VillageCore();

        string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            EventCore eCore = new EventCore();
            Guid buildingId = obj.buildingId;  //社区ID
            IEnumerable<string> uids = null;
            Request.Headers.TryGetValues("uid", out uids);
            string uid = "";
            if (uids != null)
            {
                uid = uids.FirstOrDefault();
            }

            //小区背景、小区头像
            var info = vCore.LoadEntity(o => o.V_Id == buildingId && o.V_State == 0);
            if (info == null)
            {
                rs.State = 1;
                rs.Msg = "社区不存在";
                return WebApiJsonResult.ToJson(rs);
            }

            //社区服务总数
            //int lifeCount = lifeCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == buildingId).Count();
            //int lifeCount = busVillCore.LoadEntities(p => p.BV_VillageId == buildingId).Count();
            int lifeCount = busVillCore.LoadEntities(o => o.buildingId == buildingId).Count();

            //社区活动总数
            //int eventCount = eventScopeCore.LoadEntities(p => p.S_BuildingId == buildingId).Count();
            //int eventCount = eventScopeCore.GetGroupBuyCountByBuildingId(buildingId);
            int eventCount = eCore.GetBuildingEvents(buildingId).Count() + eCore.LoadEntities(o => o.E_Status == 0 && o.E_Target == 0).Count();


            //精品汇总数
            //int mallCount = new MallGoodsCore().CountGoods(buildingId, Guid.Empty);

            //精品汇总数[特意改成了邻里团]
            // int mallCount = eventScopeCore.GetGroupBuyCountByBuildingId(buildingId);


            //mallCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == buildingId).Count();
            //社区相册
            //var albumlist =
            //    albumCore.LoadEntities(o => o.A_BuildingId == buildingId && o.A_State == (int)OperateStatusEnum.Default).OrderBy(o => o.A_Flag).ThenBy(o => o.A_Time)
            //        .Select(o => new
            //        {
            //            albumId = o.A_Id,
            //            coverImg = StaticHttpUrl + o.A_CoverImg
            //        });
            var albumlist =
                picCore.LoadEntities(p => p.P_BuildingId == buildingId && p.P_State == (int)OperateStatusEnum.Default)
                    .OrderByDescending(p => p.P_Time)
                    .Take(10)
                    .ToList()
                    .Select(o => new
                    {
                        albumId = o.P_Id,
                        coverImg = StaticHttpUrl + o.P_Folder + "/" + o.P_FileName
                    }).ToList<dynamic>();
            int albumCount = picCore.LoadEntities(o => o.P_BuildingId == buildingId && o.P_State == (int)OperateStatusEnum.Default).Count();
            for (int i = 1; i <= 5 - albumCount; i++)  //如果照片的数量不足5张，添加默认图片
            {
                albumlist.Add(new { albumId = 0, coverImg = string.Format("{0}default/pic_{1}.png", StaticHttpUrl, i) });
            }

            //我的邻居
            var neighbor =
                uCore.LoadEntities(o => o.U_Status != (int)UserStatusEnum.冻结 && o.U_BuildingId == buildingId);

            int neighborCount = neighbor.Count();  //邻居总数
            //最新的 10 位用户邻居 ;   有头像的在前面
            var neighborList = neighbor.OrderByDescending(o => o.U_Logo).ThenByDescending(o => o.U_RegisterDate).Take(10).ToList().Select(user => new
            {
                uid = user.U_Id,
                huanxinId = user.U_ChatID,
                logo = StaticHttpUrl + (string.IsNullOrEmpty(user.U_Logo) ? "default/u.png" : user.U_Logo),
                nikeName = user.U_NickName,
                cityName = user.U_City,
                buidingName = user.U_BuildingName,
                sex = user.U_Sex,
                age = user.U_Age
            });

            //社区互动
            var interactive =
                iCore.LoadEntities(o => o.I_VillageId == buildingId && o.I_Status == (int)OperateStatusEnum.Default);
            int interactiveCount = interactive.Count();
            var interactiveList = interactive.OrderByDescending(o => o.I_Date).Take(5).Select(o => new
            {
                aid = o.I_Id,
                uid = o.I_UserId,
                imgUrl = StaticHttpUrl + o.I_Img,
                title = o.I_Title
            }).ToList<dynamic>();

            string[] activeItems = new string[] { "邻里邀约", "邻里互动", "二手置换" };
            for (int i = 1; i <= 3 - interactiveCount; i++)  //如果活动的数量不足5张，添加默认活动
            {
                interactiveList.Add(new
                {
                    aid = 0,
                    uid = Guid.Empty,
                    imgUrl = string.Format("{0}default/interactive_{1}.png", StaticHttpUrl, i),
                    title = activeItems[i - 1]
                });
            }


            //社区资讯

            dynamic notice = null;
            var notAllList = nCore.LoadEntities(
                o =>
                    o.B_State == 0 && o.B_BuildingId == buildingId)
                .OrderByDescending(o => o.B_Date);
            var notList = notAllList.Take(8).ToList()
                .Select(o => new
                {
                    title = o.B_Date.ToString("[MM.dd]") + o.B_Title,
                    tags = o.B_Tags,
                    endTime = o.B_Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    proId = o.B_Id
                });  //公告

            if (!notList.Any())
            {
                notice = new List<dynamic>()
                {
                    new
                    {
                        title = "社区公告栏，仅管理员有权发布，赶快申请成为小区管理员吧！",
                        tags = "公告",
                        endTime = "",
                        proId = ""
                    }
                };

            }
            else
            {
                notice = notList;
            }

            //社区热线
            var hotline = hCOre.LoadEntities(o => o.H_BuildingId == buildingId).OrderBy(o => o.H_Flag).Select(o => new { flag = o.H_Flag, title = o.H_Title, state = 0, phone = o.H_Phone }).ToList();
            List<dynamic> temp = new List<dynamic>();

            for (int i = 1; i <= 3; i++)
            {
                if (hotline.All(o => o.flag != i))
                {
                    temp.Add(new
                    {
                        flag = i,
                        title = "",
                        phone = "仅管理员有权编辑此栏信息",
                        state = 1
                    });
                }
                else
                {
                    temp.AddRange(hotline.Where(o => o.flag == i));
                }
            }

            //物业服务 todo:这个鸟地方后期扩展，需要建表、添加动态数据，目前需求不明朗硬编码

            List<dynamic> serviceItem = new List<dynamic>()
            {
                new {id=1,title="清洁卫生",img=StaticHttpUrl+"default/sitem_1.png",phone="",url="",state=0},
                new {id=2,title="管道疏通",img=StaticHttpUrl+"default/sitem_2.png",phone="",url="",state=0},
                new {id=3,title="日常维修",img=StaticHttpUrl+"default/sitem_3.png",phone="",url="",state=0},
                new {id=4,title="门禁开锁",img=StaticHttpUrl+"default/sitem_4.png",phone="",url="",state=0}
            };

            string bgImgFileName = string.IsNullOrEmpty(info.V_Remark) ? "bgimg_1.png" : info.V_Remark;
            rs.Data = new
            {
                //shareTitle = string.Format("{2} {0}.{1}", info.V_CityName, info.V_DistrictName, info.V_BuildingName),
                shareTitle = info.V_BuildingName + "-社区中心",
                shareDesc = string.Format("{0} {1}", info.V_BuildingName, info.V_BuildingAddress),
                shareUrl = string.Format("{0}html/groupCenter/index.html?uid={1}&buildingId={2}&tip=1", ConfigurationManager.AppSettings["OutsideUrl"], uid, buildingId),

                bgImg = string.Format("{0}default/buildingBgImg/{1}", StaticHttpUrl, bgImgFileName),
                bgImgFileName,
                logo = StaticHttpUrl + info.V_Img,
                lifeCount,
                eventCount,
                album = new
                {
                    list = albumlist,
                    count = albumCount
                },
                neighbor = new
                {
                    list = neighborList,
                    count = neighborCount
                },
                interactive = new
                {
                    list = interactiveList,
                    count = interactiveCount
                },
                notice = new
                {
                    list = notice,
                    count = notAllList.Count()
                },
                hotline = temp,
                serviceItem,
                managercenter = new
                {
                    url = "http://120.76.101.244:8123/html/adminCenter/#/home/" + uid
                }
            };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 群聊 查看小区信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Chat([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            Guid buildingId = obj.buildingId;  //社区ID


            //我的邻居
            var neighbor =
                uCore.LoadEntities(o => o.U_Status != (int)UserStatusEnum.冻结 && o.U_BuildingId == buildingId);

            int neighborCount = neighbor.Count();  //邻居总数
            //最新的 10 位用户邻居 ;   有头像的在前面
            var neighborList = neighbor.OrderByDescending(o => o.U_Logo).ThenByDescending(o => o.U_RegisterDate).Take(10).Select(user => new
            {
                uid = user.U_Id,
                huanxinId = user.U_ChatID,
                logo = StaticHttpUrl + user.U_Logo,
                nikeName = user.U_NickName,
                cityName = user.U_City,
                buidingName = user.U_BuildingName,
                sex = user.U_Sex,
                age = user.U_Age
            });
            rs.Data = new
            {
                neighborCount,
                neighborList
            };

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
