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

namespace JiGuangLinXin.App.BusinessCenter.Controllers
{
    /// <summary>
    /// 商品分类
    /// </summary>
    public class MallGoodsTypeController : BaseController
    {
        private MallTypeCore typeCore = new MallTypeCore();
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
                tid = o.T_Id,
                tname = o.T_Title,
                img = StaticHttpUrl + o.T_CoverImg,
                remark = o.T_Remark
            });
            return rs; 
        }
    }
}
