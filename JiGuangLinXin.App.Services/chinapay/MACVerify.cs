using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
namespace JiGuangLinXin.App.Services
{
    public class MACVerify_ANSIX99
    {
        /// <summary>
        /// 生成的8位长度的byte数组MAC校验码，没有DES加密过程
        /// </summary>
        /// <param name="MsgData">要生成Mac的数据</param>
        /// <returns>生成的MAC码</returns>
        public byte[] GetMAC_Byte(byte[] MsgData)
        {
            if (CheckConfigSetEmpty("初始向量"))
                return null;
            if (!CheckConfigSetFormat("初始向量", "IV"))
                return null;
            byte[] IV = ByteTool.strToHexByte("0000000000000000");
            byte[] Data = ByteTool.Add8Bit(MsgData);
            byte[] b_BufArr1 = new byte[8];
            byte[] b_BufArr2 = new byte[8];
            Array.Copy(IV, b_BufArr1, 8);
            int iGroup = 0;
            if (Data.Length % 8 == 0)
                iGroup = Data.Length / 8;
            else
                iGroup = Data.Length / 8 + 1;
            for (int i = 0; i < iGroup; i++)
            {
                Array.Copy(Data, 8 * i, b_BufArr2, 0, 8);
                b_BufArr1 = ByteTool.XOR(b_BufArr1, b_BufArr2);
            }
            return b_BufArr1;
        }
        /// <summary>
        /// 生成的8位长度的byte数组MAC校验码，经过DES加密过程
        /// </summary>
        /// <param name="MsgData">要生成Mac的数据</param>
        /// <returns>生成的MAC码</returns>
        public byte[] GetMACDES_Byte(byte[] MsgData)
        {
            if (CheckConfigSetEmpty("密钥") ||
            CheckConfigSetEmpty("初始向量"))
                return null;
            if (!CheckConfigSetFormat("密钥", "Key") &&
            !CheckConfigSetFormat("初始向量", "IV"))
                return null;
            byte[] Key = ByteTool.strToHexByte("13981A75E60BCB64");
            byte[] IV = ByteTool.strToHexByte("0000000000000000");
            byte[] Data = ByteTool.Add8Bit(MsgData);
            byte[] b_BufArr1 = new byte[8];
            byte[] b_BufArr2 = new byte[8];
            Array.Copy(IV, b_BufArr1, 8);
            int iGroup = 0;
            if (Data.Length % 8 == 0)
                iGroup = Data.Length / 8;
            else
                iGroup = Data.Length / 8 + 1;

            DES des = new DES(Key, IV);
            for (int i = 0; i < iGroup; i++)
            {
                Array.Copy(Data, 8 * i, b_BufArr2, 0, 8);
                b_BufArr1 = ByteTool.XOR(b_BufArr1, b_BufArr2);
                b_BufArr2 = des.HCDES(b_BufArr1);//bTmpBuf2 = des.Encrypt(bTmpBuf1);
                Array.Copy(b_BufArr2, b_BufArr1, 8);
            }
            return b_BufArr2;
        }
        /// <summary>
        /// 生成字符串MAC校验码
        /// </summary>
        /// <param name="MsgData">要生成Mac的数据</param>
        /// <param name="Key">密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <returns>生成的MAC码</returns>
        public string GetMAC_String16(byte[] MsgData,bool NeedDES)
        {
            byte[] Byte_MAC = new byte[8];
            if (NeedDES)
                Byte_MAC = GetMACDES_Byte(MsgData);
            else
                Byte_MAC = GetMAC_Byte(MsgData);
            return ByteTool.byteToHexStr(Byte_MAC);//BitConverterHexStr
        }
        /// <summary>
        /// 进行MAC验证，返回验证结果 0正常通过 -1验证失败 -2参数初始化失败 -3未知错误
        /// </summary>
        /// <param name="message">报文数据，不带长度信息</param>
        /// <param name="NeedDES">是否选择DES加密</param>
        /// <returns>验证结果</returns>
        //public int MacVerify(byte[] message,bool NeedDES)
        //{
        //    try
        //    {
        //        byte[] CalcMAC = new byte[8];
        //        int len = message.Length;
        //        if (len < 8)
        //            return -2;
        //        byte[] part_data = new byte[len - 8];
        //        byte[] part_mac = new byte[8];
        //        Array.Copy(message, part_data, len - 8);
        //        Array.Copy(message, len - 8, part_mac, 0, 8);
        //        MACVerify_ANSIX99 mc = new MACVerify_ANSIX99();
        //        if (NeedDES)
        //            CalcMAC = mc.GetMACDES_Byte(part_data);
        //        else
        //            CalcMAC = mc.GetMAC_Byte(part_data);
        //        if (CalcMAC == null)
        //            return -2;
        //        for (int i = 0; i < 8; i++)
        //        {
        //            if (CalcMAC[i] != part_mac[i])
        //                return -1;
        //        }
        //        return 0;
        //    }
        //    catch
        //    {
        //        return -3;
        //    }
        //}
        /// <summary>
        /// 进行MAC验证，返回验证结果 0正常通过 -1验证失败 -2参数初始化失败 -3未知错误
        /// </summary>
        /// <param name="message">报文数据，不带长度信息</param>
        /// <param name="NeedDES">是否选择DES加密</param>
        /// <returns>验证结果</returns>
        public int MacVerify(byte[] message, bool NeedDES)
        {
            try
            {
                byte[] CalcMAC = new byte[8];
                int len = message.Length;
                if (len < 8)
                    return -2;
                byte[] part_data = new byte[len - 16];
                byte[] part_mac = new byte[16];
                Array.Copy(message, part_data, len - 16);
                Array.Copy(message, len - 16, part_mac, 0, 16);
                part_mac = ByteTool.strToHexByte(Encoding.GetEncoding("GB2312").GetString(part_mac));
                MACVerify_ANSIX99 mc = new MACVerify_ANSIX99();
                if (NeedDES)
                    CalcMAC = mc.GetMACDES_Byte(part_data);
                else
                    CalcMAC = mc.GetMAC_Byte(part_data);
                if (CalcMAC == null)
                    return -2;
                for (int i = 0; i < 8; i++)
                {
                    if (CalcMAC[i] != part_mac[i])
                        return -1;
                }
                return 0;
            }
            catch
            {
                return -3;
            }
        }
        private bool CheckConfigSetEmpty(string ConfigKey)
        {
            string ConfigValue = "0000000000000000";
            if (ConfigValue.Equals(string.Empty))
                return true;
            else
                return false;
        }
        private bool CheckConfigSetFormat(string ConfigKey, string Type)
        {
            string ConfigValue = "0000000000000000";
            switch (Type)
            {
                case "Encoding":
                    {
                        try
                        {
                            Encoding.GetEncoding(ConfigValue);
                            return true;
                        }
                        catch 
                        { 
                            return false; 
                        }
                    }
                case "IV":case "Key":
                    {
                        if (ConfigValue.Length %8!=0)
                            return false;
                        else
                            return true;
                    }
                default:
                    return true;
            }
        }
    }
}
