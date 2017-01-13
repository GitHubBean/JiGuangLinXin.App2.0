using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    public class EditorController : Controller
    {
        //
        // GET: /Editor/

        #region 编辑器上传图片(最新版)
        public void UploadFile()
        {
            Response.Charset = "UTF-8";

            // 初始化一大堆变量
            string filePhyPath = Server.MapPath("~/cache/editor/");

            string inputname = "filedata";//表单文件域name 
            int dirtype = 1;                 // 1:按天存入目录 2:按月存入目录 3:按扩展名存目录  建议使用按天存
            int maxattachsize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ImgSize"]);// 最大上传大小，默认是5M
            string upext = "txt,rar,zip,jpg,jpeg,gif,png,swf,wmv,avi,wma,mp3,mid";    // 上传扩展名
            int msgtype = 2;                 //返回上传参数的格式：1，只返回url，2，返回参数数组
            string immediate = Request.QueryString["immediate"];//立即上传模式，仅为演示用
            byte[] file;                     // 统一转换为byte数组处理
            string localname = "";
            string disposition = Request.ServerVariables["HTTP_CONTENT_DISPOSITION"];

            string err = "";
            string msg = "''";

            if (disposition != null)
            {
                // HTML5上传
                file = Request.BinaryRead(Request.TotalBytes);
                localname = Server.UrlDecode(Regex.Match(disposition, "filename=\"(.+?)\"").Groups[1].Value);// 读取原始文件名
            }
            else
            {
                var filecollection = Request.Files;
                var postedfile = filecollection.Get(inputname);

                // 读取原始文件名
                localname = postedfile.FileName;
                // 初始化byte长度.
                file = new Byte[postedfile.ContentLength];

                // 转换为byte类型
                System.IO.Stream stream = postedfile.InputStream;
                stream.Read(file, 0, postedfile.ContentLength);
                stream.Close();

                filecollection = null;
            }

            if (file.Length == 0) err = "无数据提交";
            else
            {
                if (file.Length > maxattachsize) err = "文件大小超过" + maxattachsize + "字节";
                else
                {
                    string attach_dir, attach_subdir, filename, extension, target;

                    // 取上载文件后缀名
                    extension = GetFileExt(localname);

                    if (("," + upext + ",").IndexOf("," + extension + ",") < 0) err = "上传文件扩展名必需为：" + upext;
                    else
                    {
                        attach_dir = filePhyPath;
                        // 生成随机文件名
                        Random random = new Random(DateTime.Now.Millisecond);
                        filename = DateTime.Now.ToString("yyyyMMddhhmmss") + random.Next(1000) + "." + extension;

                        target = attach_dir + filename;
                        try
                        {
                            CreateFolder(attach_dir);

                            System.IO.FileStream fs = new System.IO.FileStream(target, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                            fs.Write(file, 0, file.Length);
                            fs.Flush();
                            fs.Close();

                            //打上水印
                            //string path2 = Server.MapPath("watermark.png");
                            //Provide.WatermarkHelper.warterPic wp = new Provide.WatermarkHelper.warterPic();
                            //wp.markwater(target, path2, Provide.WatermarkHelper.ImagePosition.RigthBottom);

                        }
                        catch (Exception ex)
                        {
                            err = ex.Message.ToString();
                        }

                        // 立即模式判断

                        // target = System.Configuration.ConfigurationManager.AppSettings["fileSiteUrl"].ToString() + "Editor/" + filename; //返回的资源地址
                        target = "/cache/editor/" + filename; //返回的资源地址
                        if (immediate == "1") target = "!" + target;
                        target = jsonString(target);
                        if (msgtype == 1) msg = "'" + target + "'";
                        else msg = "{'url':'" + target + "','localname':'" + jsonString(localname) + "','id':'1'}";
                    }
                }
            }

            file = null;

            Response.Write("{'err':'" + jsonString(err) + "','msg':" + msg + "}");

        }

        string jsonString(string str)
        {
            str = str.Replace("\\", "\\\\");
            str = str.Replace("/", "\\/");
            str = str.Replace("'", "\\'");
            return str;
        }


        string GetFileExt(string FullPath)
        {
            if (FullPath != "") return FullPath.Substring(FullPath.LastIndexOf('.') + 1).ToLower();
            else return "";
        }

        void CreateFolder(string FolderPath)
        {
            if (!System.IO.Directory.Exists(FolderPath)) System.IO.Directory.CreateDirectory(FolderPath);
        }

        #endregion

       

    }
}
