using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class BaseAdminController : ApiController
    {
        /// <summary>
        /// 静态文件的httpurl站点地址
        /// </summary>
        public string StaticHttpUrl { get; set; }

        /// <summary>
        /// 服务静态文件的资源地址
        /// </summary>
        public string StaticFilePath { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];
            StaticFilePath = ConfigurationManager.AppSettings["StaticFilePath"];
        }
    }
}
