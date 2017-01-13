using System;
using System.Collections;
using System.IO;
using System.Web;

namespace JiGuangLinXin.App.AdminCenter.Extension.Editor
{
    /// <summary>
    /// UEditor编辑器通用上传类
    /// </summary>
    public class UmeditorUploader
    {
        string state = "SUCCESS";

        string URL = null;
        string currentType = null;
        string uploadpath = null;
        string filename = null;
        string originalName = null;
        HttpPostedFile uploadFile = null;

        /**
  * 上传文件的主处理方法
  * @param HttpContext
  * @param string
  * @param  string[]
  *@param int
  * @return Hashtable
  */
        public Hashtable UpFile(HttpContext cxt, string pathbase, string[] filetype, int size)
        {
            pathbase = pathbase + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            uploadpath = cxt.Server.MapPath(pathbase);//获取文件上传路径

            try
            {
                uploadFile = cxt.Request.Files[0];
                originalName = uploadFile.FileName;

                //目录创建
                CreateFolder();

                //格式验证
                if (CheckType(filetype))
                {
                    state = "不允许的文件类型";
                }
                //大小验证
                if (CheckSize(size))
                {
                    state = "文件大小超出网站限制";
                }
                //保存图片
                if (state == "SUCCESS")
                {
                    filename = ReName();
                    uploadFile.SaveAs(uploadpath + filename);
                    URL = pathbase + filename;
                }
            }
            catch (Exception e)
            {
                state = "未知错误";
                URL = "";
            }
            return GetUploadInfo();
        }

        /**
 * 上传涂鸦的主处理方法
  * @param HttpContext
  * @param string
  * @param  string[]
  *@param string
  * @return Hashtable
 */
        public Hashtable upScrawl(HttpContext cxt, string pathbase, string tmppath, string base64Data)
        {
            pathbase = pathbase + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            uploadpath = cxt.Server.MapPath(pathbase);//获取文件上传路径
            FileStream fs = null;
            try
            {
                //创建目录
                CreateFolder();
                //生成图片
                filename = System.Guid.NewGuid() + ".png";
                fs = File.Create(uploadpath + filename);
                byte[] bytes = Convert.FromBase64String(base64Data);
                fs.Write(bytes, 0, bytes.Length);

                URL = pathbase + filename;
            }
            catch (Exception e)
            {
                state = "未知错误";
                URL = "";
            }
            finally
            {
                fs.Close();
                DeleteFolder(cxt.Server.MapPath(tmppath));
            }
            return GetUploadInfo();
        }

        /**
* 获取文件信息
* @param context
* @param string
* @return string
*/
        public string GetOtherInfo(HttpContext cxt, string field)
        {
            string info = null;
            if (cxt.Request.Form[field] != null && !String.IsNullOrEmpty(cxt.Request.Form[field]))
            {
                info = field == "fileName" ? cxt.Request.Form[field].Split(',')[1] : cxt.Request.Form[field];
            }
            return info;
        }

        /**
     * 获取上传信息
     * @return Hashtable
     */
        private Hashtable GetUploadInfo()
        {
            Hashtable infoList = new Hashtable();

            infoList.Add("state", state);
            infoList.Add("url", URL);
            infoList.Add("originalName", originalName);
            infoList.Add("name", Path.GetFileName(URL));
            infoList.Add("size", uploadFile.ContentLength);
            infoList.Add("type", Path.GetExtension(originalName));

            return infoList;
        }

        /**
     * 重命名文件
     * @return string
     */
        private string ReName()
        {
            return System.Guid.NewGuid() + GetFileExt();
        }

        /**
     * 文件类型检测
     * @return bool
     */
        private bool CheckType(string[] filetype)
        {
            currentType = GetFileExt();
            return Array.IndexOf(filetype, currentType) == -1;
        }

        /**
     * 文件大小检测
     * @param int
     * @return bool
     */
        private bool CheckSize(int size)
        {
            return uploadFile.ContentLength >= (size * 1024 * 1024);
        }

        /**
     * 获取文件扩展名
     * @return string
     */
        private string GetFileExt()
        {
            string[] temp = uploadFile.FileName.Split('.');
            return "." + temp[temp.Length - 1].ToLower();
        }

        /**
     * 按照日期自动创建存储文件夹
     */
        private void CreateFolder()
        {
            if (!Directory.Exists(uploadpath))
            {
                Directory.CreateDirectory(uploadpath);
            }
        }

        /**
     * 删除存储文件夹
     * @param string
     */
        public void DeleteFolder(string path)
        {
            //if (Directory.Exists(path))
            //{
            //    Directory.Delete(path, true);
            //}
        }
    }
}