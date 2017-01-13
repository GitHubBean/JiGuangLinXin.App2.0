using System;

namespace JiGuangLinXin.App.Provide.EncryptHelper
{
    public class Base64
    {
        /// <summary>
        /// 将字符串使用base64算法加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <param name="ens">System.Text.Encoding 对象，如创建中文编码集对象：
        /// System.Text.Encoding.GetEncoding("gb2312")</param>
        /// <returns>编码后的文本字符串</returns>
        public static string EncodingString(string sourceString, System.Text.Encoding ens)
        {
            return Convert.ToBase64String(ens.GetBytes(sourceString));
        }

        /// <summary>
        /// 使用缺省的代码页将字符串使用base64算法加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的文本字符串</returns>
        public static string EncodingString(string sourceString)
        {
            return EncodingString(sourceString, System.Text.Encoding.Default);
        }

        /// <summary>
        /// 从base64编码的字符串中还原字符串，支持中文
        /// </summary>
        /// <param name="base64String">Base64加密后的字符串</param>
        /// <param name="ens">System.Text.Encoding对象，如创建中文编码集对象：
        /// System.Text.Encoding.Default</param>
        /// <returns>还原后的文本字符串</returns>
        public static string DecodingString(string base64String, System.Text.Encoding ens)
        {
            return ens.GetString((Convert.FromBase64String(base64String)));
        }

        /// <summary>
        ///使用缺省的代码页从Base64编码的字符串中还原字符串，支持中文
        /// </summary>
        /// <param name="base64String">Base64加密后的字符串</param>
        /// <returns>还原后的文本字符串</returns>
        public static string DecodingString(string base64String)
        {
            return DecodingString(base64String, System.Text.Encoding.Default);
        }

        /// <summary>
        /// 对一个文件进行Base64编码，并返回编码后的字符串
        /// </summary>
        /// <param name="strFileName">文件的路径和文件名</param>
        /// <returns>对文件进行Base64编码后的字符串</returns>
        public static string EncodingFileToString(string strFileName)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(strFileName);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

            string Base64String = Convert.ToBase64String(br.ReadBytes((int)fs.Length));

            br.Close();
            fs.Close();
            return Base64String;
        }


        /// <summary>
        /// 对一个文件进行Base64编码，并将编码后的内容写入到一个文件
        /// </summary>
        /// <param name="strSourceFileName">要编码的文件地址，支持任何类型的文件</param>
        /// <param name="strSaveFileName">要写入的文件路径</param>
        /// <returns>如果写入成功，则返回真</returns>
        public static bool EncodingFileToFile(string strSourceFileName, string strSaveFileName)
        {
            string strBase64 = EncodingFileToString(strSourceFileName);

            System.IO.StreamWriter fs = new System.IO.StreamWriter(strSaveFileName);
            fs.Write(strBase64);
            fs.Close();
            return true;
        }

        /// <summary>
        /// 将Base64编码字符串解码并存储到一个文件中
        /// </summary>
        /// <param name="Base64String">经过Base64编码后的字符串</param>
        /// <param name="strSaveFileName">要输出的文件路径，如果文件存在，将被重写</param>
        /// <returns>如果操作成功，则返回True</returns>
        public static bool DecodingFileFromString(string Base64String, string strSaveFileName)
        {
            System.IO.FileStream fs = new System.IO.FileStream(strSaveFileName, System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
            bw.Write(Convert.FromBase64String(Base64String));
            //bw.Write(Convert.ToBase64String)
            bw.Close();
            fs.Close();
            return true;
        }

        /// <summary>
        /// 将一个由Base64编码产生的文件解码并存储到一个文件
        /// </summary>
        /// <param name="strBase64FileName">以Base64编码格式存储的文件</param>
        /// <param name="strSaveFileName">要输出的文件路径，如果文件存在，将被重写</param>
        /// <returns>如果操作成功，则返回True</returns>
        public static bool DecodingFileFromFile(string strBase64FileName, string strSaveFileName)
        {
            System.IO.StreamReader fs = new System.IO.StreamReader(strBase64FileName, System.Text.Encoding.ASCII);
            char[] base64CharArray = new char[fs.BaseStream.Length];
            fs.Read(base64CharArray, 0, (int)fs.BaseStream.Length);
            string Base64String = new string(base64CharArray);
            fs.Close();
            return DecodingFileFromString(Base64String, strSaveFileName);
        } 
    }

}
