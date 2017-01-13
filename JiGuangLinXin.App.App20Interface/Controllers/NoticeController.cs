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
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class NoticeController : BaseController
    {

        private BillboardCore nCore = new BillboardCore();
        private UserCore uCore = new UserCore();

        private NoticeCore helpCore = new NoticeCore();
        #region 帮助中心

        public HttpResponseMessage HelperList()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            var notAllList = helpCore.LoadEntities(
                o =>
                    o.N_State == 0 && o.N_Flag == (int)PageEnum.帮助中心页).OrderByDescending(o => o.N_Clicks).ToList().Select(o => new
                {
                    proId = o.N_Id,
                    title = o.N_Title,
                    time = o.N_Date.ToString("yyyy-MM-dd HH:mm:ss")
                });

            if (notAllList.Any())
            {
                var qqListStr = ConfigurationManager.AppSettings["ServiceQQ"];
                rs.Data = new
                {
                    notAllList,
                    serviceList = qqListStr.Split('|').Select(o =>
                    {
                        var arr = o.Split(',');
                        return new
                        {
                            nickName = arr[0],
                            qq = arr[1]
                        };

                    })
                };
            }
            else
            {
                rs.State = 1;
                rs.Msg = "没有更多数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 社区公告详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage HelperDetail([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            int proId = obj.proId;


            var no = helpCore.LoadEntity(o => o.N_Id == proId && o.N_State == 0);

            if (no != null)
            {
                rs.Data = new
                {
                    proId = no.N_Id,
                    title = no.N_Title,
                    content = no.N_Content,
                    time = no.N_Date.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
            else
            {
                rs.State = 1;
                rs.Msg = "没有更多数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }
        #endregion

        #region 社区公告管理
        /// <summary>
        /// 社区公告列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage List([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            Guid buildingId = obj.buildingId;  //社区ID
            int pn = obj.pn;
            int rows = obj.rows;


            var notAllList = nCore.LoadEntities(
                o =>
                    o.B_State == 0 && o.B_BuildingId == buildingId)
                .OrderByDescending(o => o.B_Date).Skip((pn - 1) * rows).Take(rows).ToList().Select(o => new
                {
                    proId = o.B_Id,
                    title = o.B_Title,
                    content = o.B_Content,
                    time = o.B_Date.ToString("yyyy-MM-dd HH:mm:ss")
                });

            if (notAllList.Any())
            {
                rs.Data = notAllList;
            }
            else
            {
                rs.State = 1;
                rs.Msg = "没有更多数据";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Edit([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            Guid uId = Guid.Parse(GetValueByHeader("uid"));
            string uPhone = GetValueByHeader("phone");

            int proId = obj.proId;
            string nickname = obj.nickname;
            Guid bId = obj.buildingId;
            string bName = obj.buildingName;
            string title = obj.title;
            string content = obj.content;


            var user = uCore.LoadEntity(o => o.U_Id == uId && o.U_BuildingId == bId && o.U_Status == (int)UserStatusEnum.正常 && o.U_AuditingManager == (int)AuditingEnum.认证成功);

            if (user == null)
            {
                rs.State = 1;
                rs.Msg = "无权访问";
                return WebApiJsonResult.ToJson(rs);
            }

            if (proId > 0) //编辑
            {
                var info = nCore.LoadEntity(o => o.B_Id == proId);
                if (info != null)
                {
                    info.B_NickName = nickname;
                    info.B_UId = uId;
                    info.B_Title = title;
                    info.B_Content = content;
                    info.B_Date = DateTime.Now;


                    if (nCore.UpdateEntity(info))
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }
                else
                {
                    rs.State = 1;
                    rs.Msg = "数据不存在";
                }
            }
            else  //添加
            {
                var info = new Core_Billboard()
                {
                    B_BuildingId = bId,
                    B_BuildingName = bName,
                    B_Flag = 0,
                    B_NickName = nickname,
                    B_Title = title,
                    B_State = 0,
                    B_Date = DateTime.Now,
                    B_UId = uId,
                    B_UPhone = uPhone,
                    B_Content = content,
                    B_Clicks = 0

                };
                if (nCore.AddEntity(info) != null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 删除热线
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Remove([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            int proId = obj.proId;

            Guid uId = Guid.Parse(GetValueByHeader("uid"));
            Guid bId = obj.buildingId;

            var user = uCore.LoadEntity(o => o.U_Id == uId && o.U_BuildingId == bId && o.U_Status == (int)UserStatusEnum.正常 && o.U_AuditingManager == (int)AuditingEnum.认证成功);

            if (user == null)
            {
                rs.State = 1;
                rs.Msg = "无权访问";
                return WebApiJsonResult.ToJson(rs);
            }
            if (proId > 0)
            {
                var info = nCore.LoadEntity(o => o.B_Id == proId);
                if (info != null)
                {
                    if (nCore.DeleteEntity(info))
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }
                else
                {
                    rs.State = 1;
                    rs.Msg = "数据不存在";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }
        #endregion
    }
}
