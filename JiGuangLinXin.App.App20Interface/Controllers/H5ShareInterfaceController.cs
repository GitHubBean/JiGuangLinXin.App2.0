using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 前端h5 页面分享，数据接口
    /// </summary>
    public class H5ShareInterfaceController : ApiController
    {

        private string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];

        /// <summary>
        /// 分享邻里团
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel GroupbuyDetail([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid gId = obj.gId;

            GroupbuyCore buyCore = new GroupbuyCore();

            var info = buyCore.LoadEntity(o => o.GB_Id == gId);
            rs.Data = new
            {
                gId,
                title = info.GB_Titlte,
                price = info.GB_Price.ToString("N"),
                priceOld = info.GB_PriceOld.ToString("N"),
                img = StaticHttpUrl + info.GB_CoverImg
            };

            return rs;
        }

        #region 社区活动
        /// <summary>
        /// 社区活动详情（投票、互动）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel EventDetail([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid proId = obj.proId;
            Guid uid = obj.uid;


            EventCore eventCore = new EventCore();
            AttachmentCore attCore = new AttachmentCore();
            EventVoteItemCore voteCore = new EventVoteItemCore();
            EventJoinHistoryCore historyCore = new EventJoinHistoryCore();

            var info = eventCore.LoadEntity(o => o.E_Id == proId);

            var imgList = attCore.LoadEntities(o => o.A_PId == info.E_Id).OrderBy(o => o.A_Rank).Select(o => new
            {
                imgUrl = StaticHttpUrl + o.A_Folder + "/" + o.A_FileName
            });

            List<Core_EventVoteItem> voteList = null;
            List<Core_EventJoinHistory> historyList = null;

            if (info.E_Flag == (int)EventFlagEnum.投票)
            {
                voteList =
                    voteCore.LoadEntities(o => o.I_EventId == info.E_Id && o.I_State == 0)
                        .OrderBy(o => o.I_Rank)
                        .ToList();

                historyList = historyCore.LoadEntities(o => o.H_EventId == proId).ToList();
            }

            rs.Data = new
            {
                proId,
                title = info.E_Title,
                time = info.E_Date.ToString(),
                address = info.E_Address,
                busName = info.E_BusName,
                linkphone = info.E_LinkPhone,
                desc = info.E_Desc,
                flag = info.E_Flag,

                imgList,

                voteItemList = voteList == null ? null : voteList.Select(o => new
                {
                    coverImg = StaticHttpUrl + o.I_Img,
                    title = o.I_Title,
                    vid = o.I_Id,
                    flag = historyList.Any(c => c.H_UserId == uid)

                })
            };

            return rs;
        }


        /// <summary>
        /// 社区活动
        /// </summary>
        public ResultMessageViewModel EventVote([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            EventJoinHistoryCore eventhisCore = new EventJoinHistoryCore();
            EventVoteItemCore eventitemCore = new EventVoteItemCore();
            UserCore uCore = new UserCore();
            EventCore eventCore = new EventCore();

            dynamic obj = value;
            Guid voteId = obj.proId;
            Guid uid = obj.uid;

            var user = uCore.LoadEntity(o => o.U_Id == uid);
            if (user == null)
            {
                rs.Msg = "用户不存在";
                return rs;
            }
            var vi = eventitemCore.LoadEntity(o => o.I_Id == voteId);
            //var history = eventhisCore.LoadEntities(p => p.H_VoteId == voteId && p.H_UserId == uid);

            var history = eventhisCore.LoadEntities(p => p.H_EventId == vi.I_EventId && p.H_UserId == uid);

            var etInfo = eventCore.LoadEntity(o => o.E_Id == vi.I_EventId);

            if (history.Any(o => o.H_VoteId == voteId))//有数据
            {
                rs.Msg = "谢谢您的参与，请勿重复投票";
            }
            else if (etInfo.E_IsSelectedOne == 1 && history.Any())
            {
                rs.Msg = "您已经参与了此活动投票，本活动每位用户只允许投票一次，谢谢";
            }
            else
            {
                Core_EventJoinHistory his = new Core_EventJoinHistory()
                {
                    H_EventId = vi.I_EventId,
                    H_EventTtitle = vi.I_EventTtitle,
                    H_Id = Guid.NewGuid(),
                    H_Remark = "",
                    H_Time = DateTime.Now,
                    H_UserId = uid,
                    H_UserName = user.U_NickName,
                    H_VoteId = voteId,
                    H_VoteTitle = vi.I_Title,
                    H_Flag = (int)EventHistoryFlagEnum.商家活动
                };

                if (eventhisCore.AddEntity(his) != null) //添加投票记录
                {
                    var evts = eventitemCore.LoadEntities(o => o.I_EventId == vi.I_EventId && o.I_State == 0).ToList();
                    var evt = evts.First(o => o.I_Id == voteId);
                    evt.I_Count += 1; //累计票数
                    var voteHis = eventhisCore.LoadEntities(o => o.H_EventId == vi.I_EventId).ToList();
                    if (eventitemCore.UpdateEntity(evt)) //投票成功
                    {
                        //todo:投票成功需要推送消息不呢？
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = evts.OrderBy(o => o.I_Rank)
                            .Select(o => new
                            {
                                voteId = o.I_Id,
                                title = o.I_Title,
                                count = o.I_Count,
                                flag = voteHis.Any(c => c.H_UserId == uid && c.H_VoteId == o.I_Id) ? 1 : 0
                            });
                    }
                }
            }

            return rs;
        }
        /// <summary>
        /// 商家活动申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel EventApply([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            EventApplyCore applyCore = new EventApplyCore();
            EventCore eventCore = new EventCore();
            UserCore uCore = new UserCore();

            dynamic obj = value;
            Guid proId = obj.proId;
            Guid uid = obj.uid;
            string uname = obj.uname;
            string uphone = obj.uphone;
            string proTitle = obj.proTitle;
            int flag = obj.flag;//2商家活动 3楼盘活动

            var history = applyCore.LoadEntities(p => p.A_EventId == proId && p.A_UId == uid);
            if (history.Any())//有数据
            {
                rs.Msg = "谢谢您的参与，请勿重复申请";
            }
            else
            {
                //   var user = uCore.LoadEntity(o => o.U_Id == uid);
                //  var et = eventCore.LoadEntity(o => o.E_Id == proId);


                Core_EventApply his = new Core_EventApply()
                {
                    A_EventId = proId,
                    A_EventTtitle = proTitle,
                    A_Status = flag,
                    A_Time = DateTime.Now,
                    A_UId = uid,
                    A_UPhone = uphone,
                    A_Remark = uname
                };

                if (applyCore.AddEntity(his) != null)  //添加投票记录
                {

                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return rs;
        }



        /// <summary>
        /// 查看新家推荐的活动详情（有机会得红包）。另外活动里面的报名参见 API Building/Apply
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="buildingId">楼盘ID</param>
        /// <returns></returns>
        public ResultMessageViewModel BuildingActivity([FromBody] JObject value)
        {

            dynamic obj = value;

            string uIdStr = obj.userId;//用户ID
            Guid uId;

            Guid bId = obj.buildingId;//楼盘ID
            BuildingCore bCore = new BuildingCore();
            //Guid uId = Guid.Parse(userId);  //楼盘ID
            //Guid bId = Guid.Parse(buildingId);  //用户ID

            //  return Content(string.Format("用户ID：{0}, 楼盘ID:{1},报名",uId,bId));
            Guid.TryParse(uIdStr, out uId);

            var rs = bCore.Activity(bId, uId);
            //rs.Data.coverImg = StaticHttpUrl + rs.Data.coverImg;
            if (rs.State != 9999)
            {
                if (rs.State == 1)  //本次领到了红包
                {
                    var binfo = bCore.LoadEntity(o => o.B_Id == bId);
                    #region 消息推送
                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = (int)PushMessageEnum.默认,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "楼盘红包",
                        title = "您收到一个楼盘红包",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = " 您收到楼盘【" + binfo.B_Name + "】的红包，" + binfo.B_HongbaoMoney.ToString("N") + "元",
                    };

                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uId.ToString("N").ToLower());

                    #endregion
                }
                rs.Data = new
                {
                    baseImgUrl = StaticHttpUrl,
                    rs.Data
                };
            }
            return rs;
        }

        #endregion
        
        #region 商家（社区服务）中心

        public HttpResponseMessage ServiceCenter([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid bid = obj.serviceId;

            BusinessCore busCore = new BusinessCore();
            AttachmentCore attCore = new AttachmentCore();
              MallGoodsCore gCore = new MallGoodsCore();
              VoucherCardCore vcCore = new VoucherCardCore();
            var bus = busCore.LoadEntity(o => o.B_Id == bid && o.B_Status == 0);
            //附件
            var atts = attCore.LoadEntities(o => o.A_PId == bid).Select(p => new
            {
                imgUrl = StaticHttpUrl + p.A_Folder + "/" + p.A_FileName
            });
            string servicePhone = bus.B_Phone.Split(',')[0];
            if (string.IsNullOrEmpty(servicePhone))
            {
                servicePhone = bus.B_Phone.Split(',')[1];
            }


            var goods = gCore.LoadEntities(o => o.G_BusId == bid && o.G_Status == 0 && o.G_AuditingState == (int)AuditingEnum.认证成功).OrderByDescending(o => o.G_Time).Select(o => new
            {
                gId = o.G_Id,
                logo = StaticHttpUrl + o.G_Img,
                title = o.G_Name,
                tags = o.G_Tags,
                price = o.G_Price,
                sales = o.G_Sales,
                clicks = o.G_Clicks,
                expFee = o.G_ExtraFee
            });
            var cards = vcCore.LoadEntities(o => o.C_BusId == bid && o.C_State == 0 && o.C_ETime>DateTime.Now).OrderByDescending(o => o.C_Time).Select(o => new
            {
                cId = o.C_Id,
                title = o.C_Title,
                money = o.C_Money,
                sTime = o.C_STime,
                eTime = o.C_ETime,
                busName = o.C_BusNickname,
                busId = o.C_BusId,
                flag = "0"
            });


            rs.Data = new
            {
                bid = bid,
                logo = StaticHttpUrl + bus.B_Logo,
                nickName = bus.B_NickName,
                address = bus.B_Address,
                isHot = bus.B_IsHot,
                isReturn = bus.B_IsReturns,
                isAuthentic = bus.B_IsAuthentic,
                isFree = bus.B_IsFree,
                isFamous = bus.B_IsFamous,

                description = bus.B_Desc,
                imgList = atts,

                serviceContent = bus.B_ServiceItem,
                servicePhone,
                goods,
                cards
            };

            return WebApiJsonResult.ToJson(rs);
        }
        #endregion
    }
}
