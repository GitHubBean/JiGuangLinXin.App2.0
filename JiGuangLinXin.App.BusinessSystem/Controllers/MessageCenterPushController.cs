using System;
using System.Collections.Generic;
using System.Drawing.Design;
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
using JiGuangLinXin.App.Provide.Rpg;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 消息中心推送
    /// </summary>
    public class MessageCenterPushController : BaseAdminController
    {
        private MessageCenterCore msgCore = new MessageCenterCore();
        /// <summary>
        /// 新推消息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            string pid = obj.A_ProjectId;
            Sys_MessageCenter msg = new Sys_MessageCenter()
            {
                A_Building = obj.A_Building,
                A_Place = obj.A_Place,
                A_Target = obj.A_Target,
                A_ImgUrl = obj.A_ImgUrl,
                A_Title = obj.A_Title,
                A_Desc = obj.A_Desc,
                A_Linkurl = obj.A_Linkurl,
                A_Tags = obj.A_Tags,
                A_Remark = obj.A_Remark,
                A_ProjectName = obj.A_ProjectName,
                A_ProjectId = string.IsNullOrEmpty(pid) ? Guid.NewGuid() : Guid.Parse(pid),

                A_Time = DateTime.Now,
                A_Status = 0,
                A_Clicks = 0
            };

            if (msgCore.AddEntity(msg) != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
            }

            return rs;
        }

        /// <summary>
        /// 编辑消息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int msgId = obj.msgId;
            Sys_MessageCenter msg = msgCore.LoadEntity(o => o.A_Id == msgId);
            if (msg != null)
            {
                msg.A_Building = obj.A_Building;
                msg.A_Place = obj.A_Place;
                msg.A_Target = obj.A_Target;
                msg.A_ImgUrl = obj.A_ImgUrl;
                msg.A_Title = obj.A_Title;
                msg.A_Desc = obj.A_Desc;
                msg.A_Linkurl = obj.A_Linkurl;
                msg.A_Tags = obj.A_Tags;
                msg.A_Remark = obj.A_Remark;
                msg.A_ProjectName = obj.A_ProjectName;
                msg.A_ProjectId = obj.A_ProjectId;
                if (msgCore.UpdateEntity(msg))
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
        /// 删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int msgId = obj.msgId;


            var info = msgCore.LoadEntity(o => o.A_Status == 0 && o.A_Id == msgId);
            if (info != null)
            {
                info.A_Status = 1; //标识删除

                if (msgCore.UpdateEntity(info))
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
        /// 推送的消息列表
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



            Expression<Func<Sys_MessageCenter, Boolean>> exp = t => t.A_Status == 0;  //筛选条件
            if (state > 0)  //仓库
            {
                exp = exp.And(o => o.A_Status == state);
            }


            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.A_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.A_Time < et);
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.A_Title.Contains(title));
            }

            var list = msgCore.LoadEntities(exp).OrderByDescending(o => o.A_Time).Skip(pn * rows).Take(rows).ToList().Select(o => new
            {
                place = Enum.GetName(typeof(MessageCenterModuleEnum), o.A_Place),
                msgId = o.A_Id,
                time = o.A_Time,
                state = o.A_Status,
                remark = o.A_Remark,
                target = o.A_Target,
                clicks = o.A_Clicks,
                tags = o.A_Tags,
                title = o.A_Title
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
        /// 极光推送：推多个人、社区、全平台
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel JPush([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;

            int code = obj.code;
            string tags = obj.tags;
            string tips = obj.tips;
            string title = obj.title;
            int target = obj.target;
            string proIds = obj.proIds;  //小区ID集合，用户ID集合

            string url = obj.url;
            string proName = obj.proName;



            string[] ids = { };
            if (!string.IsNullOrEmpty(proIds))
            {
                ids = proIds.Split(',');
                ids = ids.Select(o => o.Replace("-", "").ToLower()).ToArray();
            }

            #region 消息推送

            JPushMsgModel jm = new JPushMsgModel()
            {
                code = code,
                proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tags = tags,
                title = title,
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tips = tips,
                logo = url,
                proName = proName
            };
            if (target == (int)PushPlatformEnum.GroupMgr)  //如果推给所有管理员，其实也是按照 别名推送
            {
                ids =
                    new UserCore().LoadEntities(
                        o => o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_Status != (int)UserStatusEnum.冻结).ToList()
                        .Select(o => o.U_Id.ToString("N").ToLower())
                        .ToArray();  // 查询所有的管理员ID

                jm.proFlag = (int) PushMessageEnum.社区活动跳转;
                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), ids);
            }
            else
            {
                Tuisong.PushMessage(target, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), ids);
            }


            #endregion


            return rs;
        }


        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel QueryUserByPhoneOrNickName([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            string key = obj.key;

            key = key.Trim();
            UserCore userCore = new UserCore();
            var user = userCore.LoadEntity(o => o.U_LoginPhone == key || o.U_NickName == key);
            if (user != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    nickname = user.U_NickName,
                    phone = user.U_LoginPhone,
                    uid = user.U_Id,
                    buildingName = user.U_BuildingName
                };
            }

            return rs;
        }
    }
}
