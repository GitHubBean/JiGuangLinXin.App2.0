using System;
using System.Collections.Generic;
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
    public class HotlineController : BaseController
    {
        private HotlineCore hCOre = new HotlineCore();
        private UserCore uCore = new UserCore();
        /// <summary>
        /// 编辑热线
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
            string phone = obj.phone;
            string title = obj.title;
            int flag = obj.flag;
            Guid bId = obj.buildingId;
            string bName = obj.buildingName;


            var user = uCore.LoadEntity(o => o.U_Id == uId && o.U_BuildingId == bId && o.U_Status == (int)UserStatusEnum.正常 && o.U_AuditingManager == (int)AuditingEnum.认证成功);

            if (user == null)
            {
                rs.State = 1;
                rs.Msg = "无权访问";
                return WebApiJsonResult.ToJson(rs);
            }

            if (proId > 0) //编辑
            {
                var info = hCOre.LoadEntity(o => o.H_Id == proId);
                if (info != null)
                {
                    info.H_Phone = phone;
                    info.H_NickName = nickname;
                    info.H_UId = uId;
                    info.H_Title = title;

                    if (hCOre.UpdateEntity(info))
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new { proId = info.H_Id };
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
                var temp = hCOre.LoadEntities(o => o.H_BuildingId == bId && o.H_Flag == flag);
                int limit = 1;  //便民电话数量限制
                if (flag != 1) //便利店、美食商店的电话
                {
                    limit = 5;
                }
                if (temp.Count() >= limit)
                {
                    rs.State = 1;
                    rs.Msg = "最多添加" + limit + "条热线信息";
                    return WebApiJsonResult.ToJson(rs);
                }

                var info = new Core_Hotline()
                {
                    H_BuildingId = bId,
                    H_BuildingName = bName,
                    H_Flag = flag,
                    H_NickName = nickname,
                    H_Phone = phone,
                    H_Rank = 0,
                    H_Time = DateTime.Now,
                    H_UId = uId,
                    H_Title = title,
                    H_UPhone = uPhone
                };
                if (hCOre.AddEntity(info) != null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new { proId = info.H_Id };
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
                var info = hCOre.LoadEntity(o => o.H_Id == proId);
                if (info != null)
                {
                    if (hCOre.DeleteEntity(info))
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

        /// <summary>
        /// 小区热线列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage List([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            Guid bId = obj.buildingId;

            var list = hCOre.LoadEntities(o => o.H_BuildingId == bId).OrderBy(o => o.H_Flag).Select(o => new { proId = o.H_Id, flag = o.H_Flag, title = o.H_Title, phone = o.H_Phone, state = 0 }).ToList();
            // List<dynamic> temp = new List<dynamic>();

            //for (int i = 1; i <= 3; i++)
            //{
            //    if (list.All(o => o.flag != i))
            //    {
            //        temp.Add(new
            //        {
            //            flag = i,
            //            phone = "仅管理员有权编辑此栏信息",
            //            title = "热线电话",
            //            state = 1
            //        });
            //    }
            //    else
            //    {
            //        temp.AddRange(list.Where(o => o.flag == i));
            //    }
            //}
            rs.Data = list;

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
