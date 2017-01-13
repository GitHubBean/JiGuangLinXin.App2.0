using System;
using System.Text;

namespace JiGuangLinXin.App.Services
{
    public class Base64
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="encode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EncodeBase64(Encoding encode, string source)
        {
            byte[] bytes = encode.GetBytes(source);
            string  res ="";
            try
            {
               res = Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string DecodeBase64(string code_type, string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        } 
    }
}
