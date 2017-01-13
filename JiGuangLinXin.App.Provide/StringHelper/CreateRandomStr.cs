using System;
using System.Security.Cryptography;
using System.Text;

namespace JiGuangLinXin.App.Provide.StringHelper
{
   public class CreateRandomStr
    {

       Random rnd = new Random((int)DateTime.Now.Ticks);
       /// <summary>
       /// 随机字母
       /// </summary>
       /// <returns></returns>
        public  char GetRandomChar()
        {
            int ret = rnd.Next(122);
            while (ret < 48 || (ret > 57 && ret < 65) || (ret > 90 && ret < 97))
            {
                ret = rnd.Next(122);
            }
            return (char)ret;
        }
       /// <summary>
       /// 生产随机码
       /// </summary>
       /// <param name="length">长度</param>
       /// <param name="num">是否生产纯数字</param>
       /// <returns></returns>
        public  string GetRandomString(int length,bool num = true)
        {
            StringBuilder sb = new StringBuilder(length);
            if (num)
            {
                int min = Convert.ToInt32(Math.Pow(10, length - 1));
                int max = Convert.ToInt32(Math.Pow(10, length));
                sb.Append(rnd.Next(min,max));
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    sb.Append(GetRandomChar());
                }
                
            }
            return sb.ToString();
        }

       /// <summary>
       /// 生产随机字符串
       /// </summary>
       /// <param name="length">字符串长度</param>
       /// <returns></returns>
        public string BuildRandomNumber(int length)
        {
            RandomNumberGenerator randgen = new RNGCryptoServiceProvider();
            byte[] data = new byte[8];
            randgen.GetBytes(data);
            return Math.Abs(BitConverter.ToInt64(data, 0)).ToString().Substring(0, 16);
        }
    }
}
