using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessCenter.Extension;
using WebGrease.Css.Extensions;

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    /// <summary>
    /// 各种审核
    /// </summary>
    public class CheckController : BaseAdminController
    {
        private EventCore eventCore = new EventCore();
        private MallGoodsCore goodsCore = new MallGoodsCore();
        private BusinessCore busCore = new BusinessCore();
        private CheckHistoryCore chCore = new CheckHistoryCore();
        private GroupbuyCore gpCore = new GroupbuyCore();
        /// <summary>
        /// 商家活动审核
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel EventCheck([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid eId = obj.eId;
            string tips = obj.tips;
            int state = obj.state;  //1成功 2失败

            var info = eventCore.LoadEntity(o => o.E_Id == eId && o.E_Status == 0 && o.E_AuditingState == (int)AuditingEnum.认证中);

            if (info != null)
            {
                info.E_AuditingState = state;
                info.E_CheckTime = DateTime.Now;
                info.E_CheckTips = tips;

                if (eventCore.UpdateEntity(info))
                {
                    //审核记录
                    chCore.AddEntity(new Sys_CheckHistory()
                    {
                        H_AdminId = obj.adminId,
                        H_AdminName = obj.adminName,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.商家活动,
                        H_ProId = info.E_Id.ToString(),
                        H_ProName = info.E_Title,
                        H_Role = 1,//0超级管理员 1商家管理员
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = tips
                    });

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
        /// 精品汇，审核
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel GoodsCheck([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid eId = obj.eId;
            string tips = obj.tips;
            int state = obj.state;  //1成功 2失败

            var info = goodsCore.LoadEntity(o => o.G_Id == eId && o.G_Status == 0 && o.G_AuditingState == (int)AuditingEnum.认证中);

            if (info != null)
            {
                info.G_AuditingState = state;
                info.G_CheckTime = DateTime.Now;
                info.G_CheckTips = tips;

                if (goodsCore.UpdateEntity(info))
                {
                    //审核记录
                    chCore.AddEntity(new Sys_CheckHistory()
                    {
                        H_AdminId = obj.adminId,
                        H_AdminName = obj.adminName,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.精品汇,
                        H_ProId = info.G_Id.ToString(),
                        H_ProName = info.G_Name,
                        H_Role = 1,//0超级管理员 1商家管理员
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = tips
                    });
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
        /// 商家服务，列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel BusinessList([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int state = obj.state;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            string title = obj.title;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_Business, Boolean>> exp = t => true;  //筛选条件

            if (state > 0)
            {
                exp = exp.And(o => o.B_AuditingState == state);
            }
            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.B_RegisterDate > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.B_RegisterDate < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.B_NickName.Contains(title));
            }
            LifestyleTypeCore mtCore = new LifestyleTypeCore();
            var typeList = mtCore.LoadEntities().ToList();
            var list = busCore.LoadEntities(exp).OrderByDescending(o => o.B_RegisterDate).Skip(pn * rows).Take(rows).Select(o => new
            {

                busId = o.B_Id,
                busName = o.B_NickName,
                busPhone = o.B_LoginPhone,
                busRole = o.B_Role,
                busLogo = o.B_Logo,
                address = o.B_Address,
                remark = o.B_Remark,
                time = o.B_RegisterDate,

                busType = typeList.FirstOrDefault(t => t.T_Id == o.B_Category).T_Title,
                state = o.B_Status
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
        /// 审核商家入驻
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel BusinessCheck([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int proId = obj.proId;
            string busName = obj.busName;
            Guid busId = obj.busId;
            string tips = obj.tips;
            int state = obj.state;  //1成功 2失败
            int category = obj.category;
            IEnumerable<dynamic> buildings = obj.buildings;
            string buildingStr = obj.buildingStr;
            string remark = obj.remark; //如果选择了小区，备注一下，选择了多少城市多少小区：重庆[20] 北京[50]

            rs = busCore.CheckBusApplly(proId, category, state, tips, buildings,remark);

            if (rs.State == 0)
            {
                //审核记录
                chCore.AddEntity(new Sys_CheckHistory()
                {
                    H_AdminId = obj.adminId,
                    H_AdminName = obj.adminName,
                    H_CheckState = state,
                    H_Flag = (int)CheckHistoryStateEnum.商家入驻,
                    H_ProId = proId.ToString(),
                    H_ProName = busName,
                    H_Role = 1,//0超级管理员 1商家管理员
                    H_State = 0,
                    H_Time = DateTime.Now,
                    H_Tips = tips
                });


                #region 消息推送

                if (!string.IsNullOrWhiteSpace(buildingStr))
                {
                    
                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    nickname = busName,
                    proFlag = (int)PushMessageEnum.社区服务跳转,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "社区服务",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    title = "您入驻的小区有新的社区服务",
                    tips = " 已入驻到您的小区",
                    uid = busId.ToString()
                };

                //string[] ids = string.Join(",", buildings.Select(o => o.B_Id.ToString("N"))).Split(',');
                //buildings.Select(o => o.B_Id.ToString("N").)

                var strs = buildingStr.Split(',').Select(o => Guid.Parse(o).ToString("N").ToLower()).ToArray();
                //推送给商家分配的社区
                Tuisong.PushMessage((int)PushPlatformEnum.Tags, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), strs);

                }
                #endregion
            }
            return rs;
        }



        /// <summary>
        /// 邻里团审核
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel GroupbuyCheck([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            string tips = obj.tips;
            int state = obj.state;  //1成功 2失败

            var info = gpCore.LoadEntity(o => o.GB_Id == proId && o.GB_State == 0 && o.GB_AuditingState == (int)AuditingEnum.认证中);

            if (info != null)
            {
                info.GB_AuditingState = state;
                info.GB_CheckTime = DateTime.Now;
                info.GB_CheckTips = tips;

                if (gpCore.UpdateEntity(info))
                {
                    //审核记录
                    chCore.AddEntity(new Sys_CheckHistory()
                    {
                        H_AdminId = obj.adminId,
                        H_AdminName = obj.adminName,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.邻里团,
                        H_ProId = info.GB_Id.ToString(),
                        H_ProName = info.GB_Titlte,
                        H_Role = 1,//0超级管理员 1商家管理员
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = tips
                    });
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
