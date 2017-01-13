using System;
using System.IO;
using System.Text;

namespace JiGuangLinXin.App.Provide.StringHelper
{
    public class ReadWriteFile
    {
        public readonly static ReadWriteFile Instance = new ReadWriteFile();
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public string ReadFile(string filePath)
        {
            string rs = string.Empty;
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
                    rs = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

            return rs;
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="filePath">目标文件地址</param>
        /// <param name="content">写入的内容</param>
        /// <param name="isAppend">是否追加到末尾，true:追加 false:覆盖</param>
        /// <returns>文件是否写入成功</returns>
        public bool WriteFile(string filePath, string content, bool isAppend)
        {
            //文件不存在，就创建
            if (!File.Exists(filePath)) { File.Create(filePath); }
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, isAppend, Encoding.UTF8))
                {
                    sw.Write(content);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        } 
    }
}
