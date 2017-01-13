using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json.Linq;
namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class GroupbuyTypeController : BaseController
    {
        private GroupbuyTypeCore typeCore = new GroupbuyTypeCore();
        /// <summary>
        /// 商品分类列表
        /// </summary> 
        /// <returns></returns>
        public ResultMessageViewModel List()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            var types = typeCore.LoadEntities(o => o.T_State == 0).OrderBy(o => o.T_Rank);
            rs.Data = types.Select(o => new
            {
                imgBaseUrl = StaticHttpUrl,
                tid = o.T_Id,
                tname = o.T_Title,
                img = o.T_CoverImg,
                remark = o.T_Remark,
                rank = o.T_Rank
            });
            return rs;
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <returns></returns>
        public ResultMessageViewModel EditList([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            IEnumerable<dynamic> itemList = obj.itemList;

            if (itemList.Any())
            {
                foreach (var itemTemp in itemList)
                {
                    Core_GroupBuyType item = new Core_GroupBuyType()
                    {
                        T_Title = itemTemp.T_Title,
                        T_State = itemTemp.T_State,
                        T_CoverImg = itemTemp.T_CoverImg,
                        T_CoverImgRecom = itemTemp.T_CoverImgRecom,
                        T_Id = itemTemp.T_Id,
                        T_Rank = itemTemp.T_Rank,
                        T_Recom = itemTemp.T_Recom,
                        T_Remark = itemTemp.T_Remark

                    };
                    if (item.T_Id > 0) //修改
                    {

                        typeCore.UpdateEntitiesNoSave(item);
                    }
                    else
                    {
                        typeCore.AddEntityNoSave(item);  //新增
                    }
                }
                if (typeCore.SaveChanges() > 0)  //更新成功
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return rs;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Add([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            Core_GroupBuyType tp = new Core_GroupBuyType()
            {
                T_Rank = obj.rank,
                T_Recom = obj.recom,
                T_State = obj.state,
                T_Remark = obj.remark,
                T_Title = obj.title

            };

            if (!string.IsNullOrEmpty(tp.T_Title) && typeCore.AddEntity(tp) != null)  //新增成功
            {
                rs.State = 0;
                rs.Msg = "ok";
            }

            return rs;
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Edit([FromBody] JObject value)
        {

            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int tid = obj.tid;
            Core_GroupBuyType tp = typeCore.LoadEntity(o => o.T_Id == tid);
            if (tp != null)
            {
                tp.T_Rank = obj.rank;
                tp.T_Recom = obj.recom;
                tp.T_State = obj.state;
                tp.T_Remark = obj.remark;
                tp.T_Title = obj.title;
                if (!string.IsNullOrEmpty(tp.T_Title) && typeCore.UpdateEntity(tp))  //修改成功
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            return rs;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel RemoveType([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            int tid = obj.tid;
            Core_GroupBuyType tp = typeCore.LoadEntity(o => o.T_Id == tid);

            tp.T_State = 1;
            if (typeCore.UpdateEntity(tp))  //修改成功
            {
                rs.State = 0;
                rs.Msg = "ok";
            }

            return rs;
        }

    }
}

