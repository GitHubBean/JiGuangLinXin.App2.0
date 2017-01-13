using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.StringHelper;

namespace JiGuangLinXin.App.App20Interface.Controllers
{

    /// <summary>
    /// webapi controller 基础方法
    /// </summary>
    [PermissionAttributeFilter]
    public class BaseController : ApiController
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

        /// <summary>
        /// 获取请求头信息
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected string GetValueByHeader(string key)
        {
            try
            {
                return Request.Headers.GetValues(key).FirstOrDefault();
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// 设置响应头信息
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        protected void SetValueByHeader(string key, string value)
        {
            try
            {
                HttpContext.Current.Response.Headers.Add(key, value);
            }
            catch (Exception ex)
            {
                //
            }
        }

        /// <summary>
        /// 设置响应头信息
        /// </summary>
        /// <param name="status">状态值</param>
        /// <param name="msg">返回消息</param>
        /// <param name="data">返回数据</param>
        protected void SetValueByHeader(string status, string msg, string data)
        {
            try
            {
                HttpContext.Current.Response.Headers.Add("state", status);
                HttpContext.Current.Response.Headers.Add("msg", HttpUtility.UrlEncode(msg));
                HttpContext.Current.Response.Headers.Add("data", HttpUtility.UrlEncode(data));
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 上传文件到服务器本地
        /// </summary>
        /// <param name="file">上传的文件</param>
        /// <param name="folder">上传的文件夹</param>
        /// <param name="fileName">文件重命名</param>
        /// <returns>是否上传成功</returns>
        protected FileUploadStateEnum UploadFileToServerPath(HttpPostedFile file, string folder, string fileName = "")
        {

            //文件上传的路径
            string basePath = ConfigurationManager.AppSettings["StaticFilePath"] + folder + "\\";
            if (!Directory.Exists(basePath))  //不存在就创建目录
                Directory.CreateDirectory(basePath);
            if (file != null) //文件存在
            {
                int sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["ImgSize"]);  //这里暂时只验证图片（如果后期可以传视频，继续扩展）
                if (sizeLimit < file.ContentLength)
                {
                    return FileUploadStateEnum.文件太大超出限制;
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    CreateRandomStr rdm = new CreateRandomStr();
                    //fileName = DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) + Path.GetExtension(file.FileName);
                    fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName); 
                }
                file.SaveAs(basePath + fileName);//上传文件
                return FileUploadStateEnum.上传成功; //成功返回 true
            }
            return FileUploadStateEnum.上传失败;
        } 
    }
}
