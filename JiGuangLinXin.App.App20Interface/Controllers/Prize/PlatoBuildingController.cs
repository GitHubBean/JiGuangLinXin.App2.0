using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.ImgHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers.Prize
{
    /// <summary>
    /// 楼盘，模仿“柏拉图”趣味标签图片
    /// </summary>
    public class PlatoBuildingController : ApiController
    {
        public HttpResponseMessage TagFun([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            // Guid bid = value.bId;  //楼盘ID

            //var info = new VillageCore().LoadEntity(o => o.V_Id == bid);\

            dynamic obj = value;
            string bName = obj.bName; //楼盘名
            bName = bName.Trim();
            if (!string.IsNullOrEmpty(bName))
            {
                string filepath = ConfigurationManager.AppSettings["TagFunPath"];

                //获得所有的模版图片 
                //var files = Directory.GetFiles(filepath, "*.jpg");
                DirectoryInfo folder = new DirectoryInfo(filepath);
                var files = folder.GetFiles();

                Random rdm = new Random(Guid.NewGuid().GetHashCode());
                string fn = files[rdm.Next(0, files.Count())].Name;  //随机娶一个图片

                WaterImageManage water = new WaterImageManage();
                string tf = water.DrawWords(fn, bName, float.Parse("0.9"), ImagePosition.Building, filepath);
                if (!string.IsNullOrEmpty(tf))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        url =  ConfigurationManager.AppSettings["ImgSiteUrl"] +"tagFun/v/"+ tf
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }
    }
}
