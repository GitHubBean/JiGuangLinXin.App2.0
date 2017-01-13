using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Services
{
    public class ByteTool
    {
        /// <summary>
        /// 两相同长度的数组进行异或操作
        /// </summary>
        /// <param name="b1">第一个数组</param>
        /// <param name="b2">第二个数组</param>
        /// <returns>异或后的结果数组</returns>
        public static byte[] XOR(byte[] b1, byte[] b2)
        {
            int len = 0;
            if (b1.Length != b2.Length)
                return null;
            len = b1.Length;
            byte[] b_target = new byte[len];
            for (int i = 0; i < len; i++)
            {
                b_target[i] = (byte)(b1[i] ^ b2[i]);
            }
            return b_target;
        }
        /// <summary>
        /// 将不是8的倍数的byte数据，初始化为8的倍数，不足的补充
        /// </summary>
        /// <param name="SourMACData"></param>
        /// <returns></returns>
        public static byte[] Add8Bit(byte[] SourMACData)
        {
            byte[] TarMacData = null;
            int iGroup = 0;
            int len_source = SourMACData.Length;
            int YuShu = SourMACData.Length % 8;
            if ((YuShu == 0))
            {
                iGroup = SourMACData.Length / 8;
                return SourMACData;
            }
            else
            {
                iGroup = SourMACData.Length / 8 + 1;
                TarMacData = new byte[iGroup * 8];
                Array.Copy(SourMACData, TarMacData, len_source);
                return FillChar(len_source + 1, TarMacData, 0x00);
            }
        }
        /// <summary>
        /// byte数组补位操作
        /// </summary>
        /// <param name="StartFillIndex">开始补位的索引</param>
        /// <param name="b_data">原始byte数组</param>
        /// <param name="Fill">补充的数据byte</param>
        /// <returns>补位后的数组</returns>
        public static byte[] FillChar(int StartFillIndex, byte[] b_data, byte Fill)
        {
            for (int i = StartFillIndex; i < b_data.Length; i++)
            {
                b_data[i] = Fill;
            }
            return b_data;
        }
        /// <summary>
        /// 把字符串转换成16进制byte数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// byte数组转16进制字符串
        /// </summary>
        /// <param name="bytes">原始数组</param>
        /// <returns>16进制字符串</returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        //结果同上
        public static string BitConverterHexStr(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-","");
        }
        public static bool CheckEmpty(string str)
        {
            if (str.Trim().Equals(string.Empty))
                return true;
            else
                return false;
        }
    }
}
