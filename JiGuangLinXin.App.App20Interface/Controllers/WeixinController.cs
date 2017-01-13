using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Services.weixin;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{

    /// <summary>
    /// 微信接口
    /// </summary>
    public class WeixinController:BaseController
    {
        private BusinessVillageCore bvCore = new BusinessVillageCore();
        private MallGoodsCore mallCore = new MallGoodsCore();
        private UserCore uCore = new UserCore();
        private BalanceCore bCore = new BalanceCore();
        private VillageCore vCore = new VillageCore();

        /// <summary>
        /// 微信首页
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public  HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0,"ok",null);
            dynamic obj = value;

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            Guid buildingId = obj.buildingId;

            //社区服务总数
            int serviceCount = bvCore.LoadEntities(p => p.BV_BusinessId == buildingId).Count(); 
            //精品汇总数
            int mallCount = new MallGoodsCore().CountGoods(buildingId, Guid.Empty); //mallCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default).Count();
            //我的邻友
            int userCount = uCore.LoadEntities(o => o.U_BuildingId == buildingId).Count();
            //我的余额
            decimal balance = bCore.LoadEntity(o => o.B_AccountId == uid).B_Balance;
            //我的社区
            var building = vCore.LoadEntity(o => o.V_Id == buildingId);
            string buildingImg = StaticHttpUrl + AttachmentFolderEnum.community + "/default.jpg";
            if (building!=null)
            {
                buildingImg = StaticHttpUrl + building.V_Img;
            }
            rs.Data = new
            {
                buildingImg ,
                serviceCount ,
                mallCount,
                userCount,
                balance
            };

            return WebApiJsonResult.ToJson(rs);
        }
        
    }


}