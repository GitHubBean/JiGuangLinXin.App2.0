using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class UploadFileController : ApiController
    {


        public string Options()
        {

            return null;
        }


        public ResultMessageViewModel Post()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            var files = HttpContext.Current.Request.Files;
            string folder = HttpContext.Current.Request.Form["folder"];
            if (files.Count > 0 && !string.IsNullOrEmpty(folder))
            {
                var file = files[0];

                //文件上传的路径
                string basePath = ConfigurationManager.AppSettings["StaticFilePath"] + folder + "\\";
                if (!Directory.Exists(basePath))  //不存在就创建目录
                    Directory.CreateDirectory(basePath);

                string type = HttpContext.Current.Request.Form["fileType"]; //上传的文件类型
                type = string.IsNullOrEmpty(type) ? "image" : type;
                int sizeLimit = 0;// 上传文件，大小限制
                switch (type)
                {
                    case "image":
                        sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["ImgSize"]);  
                        break;
                    case "video":
                        sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["VideoSize"]);
                        break;
                }
                
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
                        filename = fileName,
                        oldFileName = file.FileName,
                        size = file.ContentLength,
                        folder = folder
                    };
                }
            }
            return rs;
        }


    }
}
