using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    /// <summary>
    /// 首页推荐位管理
    /// </summary>
    public class IndexRecommendController : BaseAdminController
    {
        private IndexRecommendCore irCore = new IndexRecommendCore();
        /// <summary>
        /// 新添加推荐管理
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;


            //Guid busId = obj.busId;
            //string busName = obj.busName;
            //string busPhone = obj.busPhone;
            //string proId = obj.proId;
            //string proName = obj.proName;
            //string tags = obj.tags;
            //string title = obj.title;
            //int target = obj.target;

            //string imgUrl = obj.imgUrl;

            //int typeId = obj.typeId;
            //string remark = obj.remark;
            //int rank = obj.rank;

            //DateTime etime = obj.etime;
            //DateTime stime = obj.stime;

            //Guid adminId = obj.adminId;
            //string adminName = obj.adminName;


            Sys_IndexRecommend ir = new Sys_IndexRecommend();
            ir.R_AdminId = obj.adminId;
            ir.R_AdminName = obj.adminName;
            ir.R_BusId = obj.busId;
            ir.R_BusName = obj.busName;
            ir.R_BusPhone = obj.busPhone;
            ir.R_ETime = obj.etime;
            ir.R_STime = obj.stime;
            ir.R_Id = Guid.NewGuid();
            ir.R_ImgUrl = obj.imgUrl;
            ir.R_ProId = obj.proId;
            ir.R_ProName = obj.proName;
            ir.R_Rank = obj.rank;
            ir.R_Remark = obj.remark;
            ir.R_State = 0;
            ir.R_Tags = obj.tags;
            ir.R_Target = obj.target;
            ir.R_Titlte = obj.title;
            ir.R_Type = obj.typeId;
            ir.R_Time = DateTime.Now;
            if (irCore.AddEntity(ir) != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    rid = ir.R_Id
                };

            }


            return rs;
        }

        /// <summary>
        /// 首页推荐首页
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
            int type = obj.type;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Sys_IndexRecommend, Boolean>> exp = t => t.R_State == 0;  //筛选条件

            if (state > 0)
            {
                exp = exp.And(o => o.R_State == state);
            }
            if (type>0)
            {
                exp = exp.And(o => o.R_Type == type);
            }
            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.R_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.R_Time < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.R_Titlte.Contains(title));
            }

            var list = irCore.LoadEntities(exp).OrderByDescending(o => o.R_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                o.R_Id,
                imgUrl = StaticHttpUrl + o.R_ImgUrl,
                o.R_BusName,
                o.R_BusId,
                o.R_BusPhone,
                o.R_STime,
                o.R_ETime,
                o.R_Rank,
                o.R_State
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
        /// 下架
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Off([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Guid proId= obj.proId;


            var info = irCore.LoadEntity(o => o.R_Id == proId && o.R_State == 0 );
            if (info != null)
            {
                info.R_State = 1; //标识下架

                if (irCore.UpdateEntity(info)) //下架
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
