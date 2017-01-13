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
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Provide.StringHelper;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class UploadFileController : ApiController
    {


        public string Options()
        {

            return null;
        }


        public HttpResponseMessage Post()
        {
            ResultViewModel rs = new ResultViewModel();
            var files = HttpContext.Current.Request.Files;
            string folder = HttpContext.Current.Request.Form["folder"];
            if (files.Count > 0 && !string.IsNullOrEmpty(folder))
            {
                var file = files[0];

                //文件上传的路径
                string basePath = ConfigurationManager.AppSettings["StaticFilePath"] + folder + "\\";
                if (!Directory.Exists(basePath))  //不存在就创建目录
                    Directory.CreateDirectory(basePath);

                int sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["ImgSize"]);  //这里暂时只验证图片（如果后期可以传视频，继续扩展）
                if (sizeLimit < file.ContentLength)
                {
                    rs.Msg = "文件太大超出限制";
                }
                else
                {
                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                    file.SaveAs(basePath + fileName);//上传文件
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"],
                        filename = folder + fileName
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }


    }
}
