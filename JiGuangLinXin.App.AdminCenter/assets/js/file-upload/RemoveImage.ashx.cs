

using System.Web;
using System.IO;
namespace WebApplication.Plugin.FileUpload
{
    /// <summary>
    /// RemoveImage 的摘要说明
    /// </summary>
    public class RemoveImage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string parameters = context.Request.QueryString["ID"].ToString();
            var ParamArray = parameters.Split('|');
            string filename = ParamArray[0];

            string phyPath = @System.Configuration.ConfigurationManager.AppSettings["filePhyPath"].ToString();
            string Path = "";
            string Folder = string.Empty;
            if (ParamArray.Length > 1) { Folder = ParamArray[1] + "/"; }
            Path = phyPath + ParamArray[1] + "/" + filename;
            //删除缩略图
            //var PathNew = phyPath + Folder + filename.ScaleImage(phyPath, System.Configuration.ConfigurationManager.AppSettings["ScaleExtensions"].ToString());
            if (File.Exists(Path))
            {
                File.Delete(Path);
                //if (Path != PathNew) { File.Delete(PathNew); }//删除缩略图
                ResponseWriteEnd(context, filename.Substring(0, filename.LastIndexOf('.')));
            }
            else
            {
                ResponseWriteEnd(context, "false");
            }
        }

        /// <summary>
        /// 用于输出流，并终止流
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="msg">你输出流信息</param>
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