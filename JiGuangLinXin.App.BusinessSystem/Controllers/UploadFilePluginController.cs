using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.Entities.ViewModel;
using Newtonsoft.Json;

namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class UploadFilePluginController : BaseController
    {

        // POST api/values
        public HttpResponseMessage Post()
        {
            Result result = new Result();


            #region 处理原始图片

            // 默认的 file 域名称是__source，可在插件配置参数中自定义。参数名：src_field_name
            HttpPostedFile file = HttpContext.Current.Request.Files["__source"];
            // 如果在插件中定义可以上传原始图片的话，可在此处理，否则可以忽略。
            string folder = HttpContext.Current.Request.Form["folder"];
            if (!Directory.Exists(StaticFilePath + folder))
            {
                Directory.CreateDirectory(StaticFilePath + folder);
            }
            int sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["ImgSize"]);  //这里暂时只验证图片（如果后期可以传视频，继续扩展）

            if (file != null)
            {
                string fn = Guid.NewGuid().ToString("N").ToLower() + Path.GetExtension(file.FileName);
                string sourceUrl = string.Format("{0}/{1}", folder, fn);
                if (sizeLimit < file.ContentLength)
                {
                    result.msg = "文件太大超出限制";
                }
                else
                {
                    file.SaveAs(StaticFilePath + sourceUrl);

                    /*
                     * 可在此将 result.sourceUrl 储存到数据库，如果有需要的话。
                     * Save to database...
                     */

                    result.success = true;
                    result.msg = "Success!";
                    result.data = new
                    {
                        imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"],
                        filename = sourceUrl,
                        oldFileName = file.FileName,
                        size = file.ContentLength,
                        folder = folder
                    };
                }
            }
            #endregion

            #region 处理头像图片
            ArrayList avatarUrls = new ArrayList();
            //默认的 file 域名称：__avatar1,2,3...，可在插件配置参数中自定义，参数名：avatar_field_names
            string[] avatars = new string[3] { "__avatar1", "__avatar2", "__avatar3" };
            int avatar_number = 1;
            int avatars_length = avatars.Length;
            string tpname = Guid.NewGuid().ToString("N").ToLower();
            for (int i = 0; i < avatars_length; i++)
            {
                file = HttpContext.Current.Request.Files[avatars[i]];
                if (file != null)
                {
                    string fn = string.Format("{0}_{1}.jpg", tpname, avatar_number);
                    string virtualPath = string.Format("{0}/{1}", folder, fn);
                    
                    if (sizeLimit < file.ContentLength)
                    {
                        result.msg = "文件太大超出限制";
                    }
                    else
                    {
                        file.SaveAs(StaticFilePath + (virtualPath));  //上传拆建的图片
                        /*
                         *	可在此将 virtualPath 储存到数据库，如果有需要的话。
                         *	Save to database...
                         */
                        avatar_number++;
                        
                        avatarUrls.Add(new
                        {
                            filename = fn,
                            oldFileName = file.FileName,
                            size = file.ContentLength
                        });
                    }
                }
            }

            #endregion


            result.success = true;
            result.msg= "ok";
            result.data = new
            {
                imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"],
                folder = folder,
                fileList = avatarUrls
            };

            //返回图片的保存结果（返回内容为json字符串，可自行构造，该处使用Newtonsoft.Json构造）
            var rsJson = JsonConvert.SerializeObject(result);

            HttpResponseMessage rs = new HttpResponseMessage { Content = new StringContent(rsJson, Encoding.GetEncoding("UTF-8"), "application/json") };
            return rs;
        }

        /////////////////////////////////////

        /// <summary>
        /// 生成指定长度的随机码。
        /// </summary>
        private string CreateRandomCode(int length)
        {
            string[] codes = new string[36] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            StringBuilder randomCode = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                randomCode.Append(codes[rand.Next(codes.Length)]);
            }
            return randomCode.ToString();
        }
        /// <summary>
        /// 表示图片的上传结果。
        /// </summary>
        private struct Result
        {
            /// <summary>
            /// 表示图片是否已上传成功。
            /// </summary>
            public bool success;
            /// <summary>
            /// 自定义的附加消息。
            /// </summary>
            public string msg;
            /// <summary>
            /// 表示所有头像图片的保存地址，该变量为一个数组。
            /// </summary>
            public object data;
        }
    }

}