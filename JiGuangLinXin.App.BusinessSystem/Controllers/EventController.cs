using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Antlr.Runtime;
using JiGuangLinXin.App.BusinessSystem.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class EventController : BaseController
    {
        private EventCore eventCore = new EventCore();
        private EventVoteItemCore viCore = new EventVoteItemCore();
        private EventApplyCore applyCore = new EventApplyCore();
        private MallGoodsCore goodsCore = new MallGoodsCore();
        /// <summary>
        /// 新增活动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Core_Event et = new Core_Event(); //活动对象
            //string json = obj.images;
            IEnumerable<dynamic> voteItems = obj.voteItem;  //如果是投票项目
            IEnumerable<dynamic> images = obj.images;//JsonSerialize.Instance.JsonToObject();  //活动的图片附件

            et.E_BusId = obj.eventInfo.E_BusId;
           // et.E_BusName = obj.eventInfo.E_BusName;
            //et.E_BusRole = obj.eventInfo.E_BusRole;
            et.E_Title = obj.eventInfo.E_Title;
            et.E_Flag = obj.eventInfo.E_Flag;
            et.E_Img = obj.eventInfo.E_Img;
            et.E_Content = obj.eventInfo.E_Content;
            et.E_Tags = obj.eventInfo.E_Tags;
            et.E_STime = obj.eventInfo.E_STime;
            et.E_ETime = obj.eventInfo.E_ETime;
            et.E_Remark = obj.eventInfo.E_Remark;
            et.E_Desc = obj.eventInfo.E_Desc;
            et.E_Address = obj.eventInfo.E_Address;
            et.E_LinkPhone = obj.eventInfo.E_LinkPhone;
            et.E_IsSelectedOne = obj.eventInfo.E_IsSelectedOne;
            et.E_TargetUrl = obj.eventInfo.E_TargetUrl;
            et.E_GoodsName = obj.eventInfo.E_GoodsName;
            Guid gid = Guid.Empty;
            string gidStr = obj.eventInfo.E_GoodsId;
            if (!string.IsNullOrEmpty(gidStr) && Guid.TryParse(gidStr, out gid))
            {
                et.E_GoodsId = gid;
            }
             //et.E_Target = obj.eventInfo.E_Target;  //活动发布的范围以商家角色而定：平台商家发布到全国 社区商家发布到自家服务社区


            et.E_Id = Guid.NewGuid();
            et.E_Date = DateTime.Now;
            et.E_Status = 0;
            et.E_AuditingState = (int)AuditingEnum.认证中;


            var bus = new BusinessCore().LoadEntity(o => o.B_Id == et.E_BusId && o.B_Status == 0);
            if (bus!=null)
            {
                //活动发布的范围以商家角色而定：平台商家发布到全国 社区商家发布到自家服务社区
                et.E_Target = bus.B_Role == (int) MemberRoleEnum.平台 ? 0 : 1;

                et.E_BusName = bus.B_NickName;

                et.E_BusRole = bus.B_Role;

                if (eventCore.AddOneEvent(et, voteItems, images))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new { eventId = et.E_Id };
                }

            }

            //Guid E_Id = obj.E_Id;
            //Guid E_BusId = obj.E_BusId;
            //string E_BusName = obj.E_BusName;
            //int E_BusRole = obj.E_BusRole;
            //string E_Title = obj.E_Title;
            //int E_Flag = obj.E_Flag;
            //string E_Img = obj.E_Img;
            //string E_Content = obj.E_Content;
            //string E_Tags = obj.E_Tags;
            //DateTime E_STime = obj.E_STime;
            //DataTableExtensions 

            return rs;
        }
        /// <summary>
        /// 编辑活动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid eventId = obj.eventInfo.E_Id;

            Core_Event et = eventCore.LoadEntity(o => o.E_Id == eventId); //活动对象
            if (et != null)
            {
                et.E_BusId = obj.eventInfo.E_BusId;
                et.E_BusName = obj.eventInfo.E_BusName;
                et.E_BusRole = obj.eventInfo.E_BusRole;
                et.E_Title = obj.eventInfo.E_Title;
                et.E_Flag = obj.eventInfo.E_Flag;
                et.E_Img = obj.eventInfo.E_Img;
                et.E_Content = obj.eventInfo.E_Content;
                et.E_Tags = obj.eventInfo.E_Tags;
                et.E_STime = obj.eventInfo.E_STime;
                et.E_ETime = obj.eventInfo.E_ETime;
                et.E_Remark = obj.eventInfo.E_Remark;
                et.E_Desc = obj.eventInfo.E_Desc;
                et.E_Address = obj.eventInfo.E_Address;
                et.E_LinkPhone = obj.eventInfo.E_LinkPhone;
                et.E_IsSelectedOne = obj.eventInfo.E_IsSelectedOne;
                et.E_TargetUrl = obj.eventInfo.E_TargetUrl;
                et.E_GoodsName = obj.eventInfo.E_GoodsName;

                et.E_Date = DateTime.Now;
                Guid gid = Guid.Empty;
                string gidStr = obj.eventInfo.E_GoodsId;
                if (!string.IsNullOrEmpty(gidStr) && Guid.TryParse(gidStr, out gid))
                {
                    et.E_GoodsId = gid;
                }
                et.E_Target = obj.eventInfo.E_Target;
                IEnumerable<dynamic> voteItems = obj.voteItem;  //如果是投票项目
                IEnumerable<dynamic> images = obj.images;//JsonSerialize.Instance.JsonToObject();  //活动的图片附件

                if (et.E_AuditingState == (int)AuditingEnum.认证失败)
                {
                    et.E_AuditingState = (int)AuditingEnum.认证中;
                }
                if (eventCore.EditOneEvent(et, voteItems, images))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return rs;
        }
        /// <summary>
        /// 获得商家发布的精品汇（商品）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel GoodsList([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;

            var list = goodsCore.LoadEntities(o => o.G_BusId == busId && o.G_Status == 0).OrderByDescending(o => o.G_Time);
            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = list.Select(o => new
                {
                    goodsId = o.G_Id,
                    goodsName = o.G_Name
                });
            }
            else
            {
                rs.State = 1;
                rs.Msg = "暂无数据";
            }
            return rs;
        }


        /// <summary>
        /// 活动列表
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
            int eventType = obj.eventType;
            string busId = obj.busId;
            string busName = obj.busName;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_Event, Boolean>> exp = t => t.E_Status == 0;  //筛选条件

            if (!string.IsNullOrEmpty(busId))
            {
                Guid gi = Guid.Parse(busId);
                exp = exp.And(o => o.E_BusId == gi);
            }

            if (!string.IsNullOrEmpty(busName))
            {
                exp = exp.And(o => o.E_BusName.Contains(busName));
            }


            if (state > -1)
            {
                exp = exp.And(o => o.E_AuditingState == state);
            }
            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.E_Date > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.E_Date < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.E_Title.Contains(title));
            }
            if (eventType > -1)
            {
                exp = exp.And(o => o.E_Flag == eventType);
            }

            var list = eventCore.LoadEntities(exp).OrderByDescending(o => o.E_Date).Skip(pn * rows).Take(rows).Select(o => new
            {
                eid = o.E_Id,
                flag = o.E_Flag,
                time = o.E_Date,
                title = o.E_Title,
                stime = o.E_STime,
                etime = o.E_ETime,
                address = o.E_Address,
                desc = o.E_Desc,
                coverImg = StaticHttpUrl + o.E_Img,

                busId = o.E_BusId,
                busName = o.E_BusName,
                busPhone = o.E_LinkPhone,

                tageturl = o.E_TargetUrl,

                proName = o.E_GoodsName,
                proId = o.E_GoodsId,

                checkTime = o.E_CheckTime,
                checkTips = o.E_CheckTips
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
        /// 活动下架
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid busId = obj.busId;
            Guid eventId = obj.eventId;
            int state = obj.state;

            var info = eventCore.LoadEntity(o => o.E_BusId == busId && o.E_Status == 0 && o.E_Id == eventId);
            if (info != null)
            {
                info.E_Status = state; //标识1下架、2删除

                if (eventCore.UpdateEntity(info)) //下架
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

        /// <summary>
        /// 单个活动详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid eventId = obj.eventId;


            var et = eventCore.LoadEntity(o => o.E_Id == eventId);
            if (et != null)
            {
                dynamic voteItems = null;
                if (et.E_Flag == (int)EventFlagEnum.投票)  //投票活动，加载投票项目
                {
                    voteItems = viCore.LoadEntities(o => o.I_State == 0 && o.I_EventId == eventId).OrderBy(o => o.I_Rank).Select(o => new
                    {
                        o.I_Title,
                        o.I_Rank,
                        o.I_Img
                    });
                }

                AttachmentCore attCore = new AttachmentCore();
                var attList = attCore.LoadEntities(o => o.A_PId == eventId).OrderBy(o => o.A_Rank).Select(o => new
                {
                    o.A_FileNameOld,
                    o.A_FileName,
                    o.A_Size,
                    o.A_Folder,
                    o.A_Rank
                });


                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    et.E_Id,
                    et.E_BusId,
                    et.E_BusName,
                    et.E_BusRole,
                    et.E_Title,
                    et.E_Flag,
                    et.E_Img,
                    et.E_Content,
                    et.E_Tags,
                    et.E_STime,
                    et.E_ETime,
                    et.E_Remark,
                    et.E_Desc,
                    et.E_Address,
                    et.E_LinkPhone,
                    et.E_IsSelectedOne,
                    et.E_Target,
                    et.E_GoodsId,
                    et.E_GoodsName,
                    et.E_TargetUrl,

                    images = attList,
                    voteItem = voteItems
                };
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }


        /// <summary>
        /// 投票活动，结果详细
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel VoteEventResult([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid eventId = obj.eventId;

            int pn = obj.pn;
            int rows = obj.rows;

            var info = eventCore.LoadEntity(o => o.E_Status == 0 && o.E_Id == eventId);
            if (info != null && info.E_Flag == (int)EventFlagEnum.投票)
            {
                var voteItem = viCore.LoadEntities(o => o.I_State == 0 && o.I_EventId == eventId).OrderBy(o => o.I_Rank).Select(o => new
                {
                    o.I_Title,
                    o.I_Rank,
                    o.I_Img,
                    o.I_Count

                });
                EventJoinHistoryCore hisCore = new EventJoinHistoryCore();
                var voteHistory =
                    hisCore.LoadEntities(o => o.H_EventId == eventId)
                        .OrderByDescending(o => o.H_Time).Skip(pn * rows).Take(rows)
                        .Select(o => new
                        {
                            o.H_UserId,
                            o.H_UserName,
                            o.H_VoteId,
                            o.H_VoteTitle,
                            o.H_Remark
                        });
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    eventInfo = new
                    {
                        info.E_Id,
                        info.E_Title
                    },
                    voteItem,
                    voteHistory
                };
            }
            else
            {
                rs.Msg = "记录不存在";
            }
            return rs;
        }
        /// <summary>
        /// 报名活动，结果详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ApplyEventResult([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid eventId = obj.eventId;
            string bname = obj.bname;//按小区名筛选

            int pn = obj.pn;
            int rows = obj.rows;

            var et = eventCore.LoadEntity(o => o.E_Id == eventId && o.E_Flag == (int)EventFlagEnum.报名);
            if (et != null)
            {
                // && o.A_Remark.Contains(bname)
                var history =
                    applyCore.LoadEntities(o => o.A_EventId == eventId)
                        .OrderByDescending(o => o.A_Time).Skip(pn * rows).Take(rows)
                        .Select(o => new
                        {
                            o.A_Remark,
                            o.A_UId,
                            o.A_UPhone
                        });

                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    eventInfo = new { et.E_Id, et.E_Title },
                    history
                };
            }
            else
            {
                rs.Msg = "记录不存在";

            }

            return rs;
        }



    }
}
