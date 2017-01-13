using System;
using System.Collections.Generic;
using System.IO;
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
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 社区互动
    /// </summary>
    public class InteractiveController : BaseController
    {
        private InteractiveCore actCore = new InteractiveCore();
        private EventJoinHistoryCore eventhisCore = new EventJoinHistoryCore();

        private EventCore eventCore = new EventCore();

        private EventVoteItemCore eventitemCore = new EventVoteItemCore();

        /// <summary>
        /// 所有的社区互动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Index([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            var list =
                actCore.LoadEntities(o => o.I_VillageId == buildingId && o.I_Status == 0)
                    .OrderByDescending(o => o.I_Date).Skip(pn * rows).Take(rows).ToList().Select(o => new
                    {
                        itemId = o.I_Id,
                        coverImg = StaticHttpUrl + o.I_Img,
                        title = o.I_Title,
                        desc = o.I_Content,
                        time = o.I_Date,
                        userId = o.I_UserId,
                        userName = o.I_UserName,
                        userLogo =StaticHttpUrl + o.I_UserLogo,
                        flag = o.I_Flag,
                        tag = o.I_Tag //(0用户发布  1平台发布)

                    });

            rs.Data = list;
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 社区互动（普通）
        /// </summary>
        public HttpResponseMessage Publish()
        {
            ResultViewModel rs = new ResultViewModel();

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();


            string areaCode = HttpContext.Current.Request.Form["areaCode"];
            string title = HttpContext.Current.Request.Form["title"];
            string content = HttpContext.Current.Request.Form["content"];

            string nickname = HttpContext.Current.Request.Form["nickname"];
            string logo = HttpContext.Current.Request.Form["logo"];

            int logoIx = logo.IndexOf("avatar");  //截取头像字符串
            if (logoIx>-1)
            {
                logo = logo.Substring(logoIx);
            }

            Guid buildingId = Guid.Parse(HttpContext.Current.Request.Form["buildingId"]);

            #region  互动实体

            Guid actId = Guid.NewGuid();
            Core_LuckyGift hb = null;   // 互动带红包
            Core_Interactive act = new Core_Interactive()
            {
                I_VillageId = buildingId,
                I_AreaCode = areaCode,
                I_Clicks = 0,
                I_Comments = 0,
                I_Content = content,
                I_Date = DateTime.Now,
                I_Hongbao = 0,
                I_Id = actId,
                I_Likes = 0,
                I_Recom = 0,
                I_Remark = "",
                I_Status = 0,
                I_Tags = "",
                I_Title = title,
                I_Top = 0,
                I_Type = 0,
                I_Flag = (int)InteractiveFlagEnum.社区互动,
                I_UserId = uid,
                I_UserLogo = logo,
                I_UserName = nickname,
                I_Tag = 0
            };
            #endregion

            #region  封面上传
            HttpPostedFile coverImg = HttpContext.Current.Request.Files["coverImg"];


            if (coverImg != null)  //缴费图片存在
            {
                string bPath = Guid.NewGuid().ToString("N") + Path.GetExtension(coverImg.FileName);
                var upCover = UploadFileToServerPath(coverImg, AttachmentFolderEnum.interactive.ToString(), bPath);

                if (upCover != FileUploadStateEnum.上传成功)
                {
                    rs.Msg = upCover.ToString();
                    return WebApiJsonResult.ToJson(rs);
                }
                act.I_Img = AttachmentFolderEnum.interactive + "/" + bPath; //修改 话题封面图片
            }
            #endregion


            #region  附带红包
            string hbJson = HttpContext.Current.Request.Form["hongbao"];
            string paypwd = HttpContext.Current.Request.Form["payPwd"];  //如果带有红包，必须输入支付密码
            if (!string.IsNullOrEmpty(hbJson))  //包含有红包的话题
            {
                var hbObj = JsonSerialize.Instance.JsonToObject(hbJson);
                string moneyStr = hbObj.money;
                int count = hbObj.count;
                if (!string.IsNullOrEmpty(moneyStr))
                {
                    hb = new Core_LuckyGift();
                    act.I_Hongbao = 1;
                    hb.LG_Id = Guid.NewGuid();
                    hb.LG_Title = "用户发布带红包的社区互动";
                    hb.LG_Type = (int)LuckGiftTypeEnum.社区互动红包;
                    hb.LG_UserId = uid;
                    hb.LG_Money = Convert.ToDecimal(DESProvider.DecryptString(moneyStr));
                    hb.LG_RemainMoney = hb.LG_Money;
                    hb.LG_Count = Convert.ToInt32(count);
                    hb.LG_RemainCount = hb.LG_Count;
                    hb.LG_CreateTime = DateTime.Now;
                    hb.LG_Flag = null;//actId;  //红包与互动关联
                    hb.LG_Status = 0;
                    hb.LG_VillageId = act.I_VillageId;
                    hb.LG_AreaCode = act.I_AreaCode;
                    hb.LG_ProjectId = actId;
                    hb.L_ProjectTitle = act.I_Title;
                }
            }

            #endregion


            #region 投票互动

            List<Core_EventVoteItem> voteArr = new List<Core_EventVoteItem>();
            string voteJson = HttpContext.Current.Request.Form["voteJson"];
            if (!string.IsNullOrEmpty(voteJson)) //包含有投票的话题
            {
                dynamic votelist = JsonSerialize.Instance.JsonToObject(voteJson);
                //List<Core_EventVoteItem>   votelist = new  List<Core_EventVoteItem>();
                act.I_Flag = (int)InteractiveFlagEnum.投票互动;

                if (votelist != null && votelist.Count > 0)
                {
                    foreach (var item in votelist)
                    {
                        voteArr.Add(new Core_EventVoteItem()
                        {
                            I_Count = 0,
                            I_EventId = actId,
                            I_EventTtitle = act.I_Title,
                            I_Id = Guid.NewGuid(),
                            I_Rank = item.I_Rank,
                            I_State = 0,
                            I_Title = item.I_Title
                        });
                    }
                }
                //vote.T_UserId = uid;
                //ticket.T_TopicId = topic.T_Id;
                //ticket.T_TopicName = topic.T_Title;
                //ticket.T_Cinema = ticketObj.cinema;
                //ticket.T_MovieName = ticketObj.movieName;
                //ticket.T_MovieCode = ticketObj.movieCode;
                //ticket.T_DatetTime = DateTime.Now;
            }
            #endregion
            ResultMessageViewModel msg = actCore.AddOne(act, hb, phone, voteArr, paypwd);  //发布社区互动
            rs.State = msg.State;
            rs.Data = new
            {
                itemId = act.I_Id,
                flag = act.I_Flag
            };
            rs.Msg = msg.Msg;
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 查看社区互动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage View([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            Guid activeId = obj.activeId;
            rs = actCore.View(activeId, uid);
            rs.Data = new
            {
                imgBaseUrl = StaticHttpUrl,
                rs.Data
            };



            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 删除社区互动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Remove([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            Guid activeId = obj.activeId;
            UserCore uCore = new UserCore();
            var mgr = uCore.LoadEntity(o => o.U_Id == uid);


            var info = actCore.LoadEntity(o => o.I_Id == activeId);
            //只有管理员/或者 发布社区互动的用户，才能删除社区互动
            if ((mgr.U_AuditingManager == (int)AuditingEnum.认证成功 && mgr.U_BuildingId == info.I_VillageId) || info.I_UserId == uid)
            {
                info.I_Status = 1;//标记删除

                if (actCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                rs.Msg = "只有管理员才能删除社区互动";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 社区互动（投票互动）
        /// </summary>
        public HttpResponseMessage Vote([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            string nickname = obj.nickname;
            Guid proId = obj.proId;
            string proName = obj.proName;
            Guid voteId = obj.voteId;
            string voteTitle = obj.voteTitle;


            var history = eventhisCore.LoadEntities(p => p.H_VoteId == voteId && p.H_UserId == uid);
            if (history.Any())//有数据
            {
                rs.Msg = "谢谢您的参与，请勿重复投票";
            }
            else
            {
                Core_EventJoinHistory his = new Core_EventJoinHistory()
                {
                    H_EventId = proId,
                    H_EventTtitle = proName,
                    H_Id = Guid.NewGuid(),
                    H_Remark = "",
                    H_Time = DateTime.Now,
                    H_UserId = uid,
                    H_UserName = nickname,
                    H_VoteId = voteId,
                    H_VoteTitle = voteTitle,
                    H_Flag = (int)EventHistoryFlagEnum.社区互动
                };

                if (eventhisCore.AddEntity(his) != null)  //添加投票记录
                {
                    var evts = eventitemCore.LoadEntities(o => o.I_EventId == proId && o.I_State == 0).ToList();
                    var evt = evts.First(o => o.I_Id == voteId);
                    evt.I_Count += 1;  //累计票数
                    var voteHis = eventhisCore.LoadEntities(o => o.H_EventId == proId).ToList();
                    if (eventitemCore.UpdateEntity(evt))  //投票成功
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


            return WebApiJsonResult.ToJson(rs);
        }
    }
}
