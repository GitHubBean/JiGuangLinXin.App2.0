using System;
using System.Web;
using System.IO;

namespace WebApplication.Plugin.FileUpload
{

    /// <summary>
    /// FileUploadHandler 的摘要说明
    /// </summary>
    public class FileUploadHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            System.Threading.Thread.Sleep(2000);
            var file = context.Request.Files["userfile"];
            string folder = context.Request["Folder"].ToString();
            string optionType = context.Request["Type"].ToString();  //文件类型，(后面可根据文件类型限制上传文件的大小）
            string strFileName = context.Request["FileName"]; //自定义的上传文件名
            string useOldFileName = context.Request["UseOldFileName"];  //使用上传的文件名


            string waterMark = context.Request["WaterMark"]; //是否加水印

            if (file == null)
            {
                ResponseWriteEnd(context, "文件不存在");
                return;
            }

            string oldName = file.FileName;  //原文件名
            if ("1".Equals(useOldFileName))  //使用原文件名
            {
                strFileName = oldName;
            }
            else //使用重命名
            {
                string type = oldName.Substring(oldName.LastIndexOf('.'));  //后缀名
                if (string.IsNullOrEmpty(strFileName))  //如果没有自定义文件名，采用默认命名文件；
                {
                    strFileName = Guid.NewGuid().ToString("N") + type;
                }
                else
                {
                    if (strFileName.IndexOf('.') < 0)
                    {
                        strFileName += type;
                    }
                }
            }

            int bytes = context.Request.Files[0].ContentLength;
            string contentType = context.Request.Files[0].ContentType;
            string strLocation = string.Empty;
            //缩放图的路径 （统一上传到指定的物理盘符目录）
            string filePhyPath = System.Configuration.ConfigurationManager.AppSettings["StaticFilePath"]; // context.Server.MapPath("~/cache/");
            string path = filePhyPath + folder + "/";  //构造文件上传的物理目录
            context.Response.ContentType = "text/plain";

            switch (optionType)
            {
                case "Image":
                    int imgMaxLen = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ImgSize"].ToString());
                    //如果，上传的文件大于5MB的话，抛消息出来
                    if (bytes > imgMaxLen)
                    {
                        ResponseWriteEnd(context, "1");
                    }
                    break;
                case "Video":
                    int videoMaxLen = int.Parse(System.Configuration.ConfigurationManager.AppSettings["VideoSize"].ToString());
                    //如果，上传的文件大于25MB的话，抛消息出来
                    if (bytes > videoMaxLen)
                    {
                        ResponseWriteEnd(context, "1");
                    }
                    break;
            }
            if (!Directory.Exists(path))  //如果上传文件夹不存在，创建
            {
                Directory.CreateDirectory(path);
            }
            strLocation = path + strFileName;  //上传文件的完整路径

            context.Request.Files[0].SaveAs(strLocation);

            if (!string.IsNullOrEmpty(waterMark))  //上传的图片要求添加水印
            {
                Watermark(context, strLocation);
            }
            //资源文件存放的RUL
            string url = System.Configuration.ConfigurationManager.AppSettings["ImgSiteUrl"];//"/cache/";

            string msg = string.Format("{0}|{1}|{2}|{3}|{4}", strFileName, folder, url + folder + "/" + strFileName, oldName, bytes);
            ResponseWriteEnd(context, msg);
        }

        /// <summary>
        /// 加水印
        /// </summary>
        /// <param name="path"></param>
        private void Watermark(HttpContext context, string path)
        {

            string path2 = context.Server.MapPath("watermark.png");
            //WatermarkHelper.warterPic wp = new Provide.WatermarkHelper.warterPic();
            //wp.markwater(path, path2, WatermarkHelper.ImagePosition.RigthBottom);
        }


        /// <summary>
        /// 用于输出流，并终止流
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="msg">你输出流信息</param>
        /// <param name="optionType"></param>
        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
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