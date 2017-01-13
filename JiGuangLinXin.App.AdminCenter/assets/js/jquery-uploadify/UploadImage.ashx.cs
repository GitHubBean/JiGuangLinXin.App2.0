using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Web.RemoteHandlers
{
    /// <summary>
    /// UploadImage 的摘要说明
    /// </summary>
    public class UploadImage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            DateTime date = DateTime.Now;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;
            try
            {

                HttpPostedFile postedFile = context.Request.Files["Filedata"];               
                string tempPath = string.Empty;
                string originalpath = string.Empty;
                string publishedPath = string.Empty;
                string thumbnailsPath = string.Empty;
                tempPath = System.Configuration.ConfigurationManager.AppSettings["FolderGalleryPath"];

                originalpath = context.Server.MapPath(tempPath+"Original");  //原图
                publishedPath = context.Server.MapPath(tempPath + "Published");  //水印图
                thumbnailsPath = context.Server.MapPath(tempPath+"Thumbnails");  //缩略图

                tempPath = tempPath + "Original/";// 临时路径
                string filename = postedFile.FileName;
                string sExtension = filename.Substring(filename.LastIndexOf('.'));
                CreateSubDir(originalpath);
                CreateSubDir(publishedPath);
                CreateSubDir(thumbnailsPath);
                string sNewFileName = Guid.NewGuid().ToString().ToUpper();
                //保存原图
                postedFile.SaveAs(originalpath + @"/" + sNewFileName + sExtension);
                //保存发布之后的图片(应用到列表)
             //   ImgZoomHelper.MakeThumbnail(originalpath + @"/" + sNewFileName + sExtension, publishedPath + @"/" + sNewFileName + sExtension, 232, 232, null, null);
                //保存缩略图(引用到详情)
               // ImgZoomHelper.MakeThumbnail(originalpath + @"/" + sNewFileName + sExtension, thumbnailsPath + @"/" + sNewFileName + sExtension, 350, 350, null, null);
                context.Response.Write(tempPath + sNewFileName + sExtension);
                context.Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                context.Response.Write("错误: " + ex.Message);
            }
        }
        public void CreateSubDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}