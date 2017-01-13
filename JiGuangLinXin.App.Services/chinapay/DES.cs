using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.IO;
namespace JiGuangLinXin.App.Services
{
    public sealed class DES
    {
        public DES(string key, string iv)
        {
            this.Key = Encode.GetBytes(key);
            this.IV = Encode.GetBytes(iv);
        }
        public DES(byte[] key, byte[] iv)
        {
            this.Key = key;
            this.IV = iv;
        }

        /// <summary>
        /// DES加密偏移量，必须是>=8位长的字符串
        /// </summary>
        public byte[] IV { get; set; }
        /// <summary>
        /// DES加密的私钥，必须是8位长的字符串
        /// </summary>
        public byte[] Key { get; set; }
        /// <summary>
        /// 编码规则
        /// </summary>
        public Encoding Encode { get; set; }

        public byte[] HCDES(byte[] Data)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Mode = CipherMode.CBC;
            DES.Padding = PaddingMode.None;
            byte[] rgbiv = new byte[8];
            rgbiv = IV;
            ICryptoTransform MyTransform = DES.CreateEncryptor(Key, rgbiv);
            MemoryStream ms = new MemoryStream();
            CryptoStream MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
            MyCryptoStream.Write(Data, 0, Data.Length);
            byte[] bEncRet = new byte[8];
            bEncRet = ms.ToArray();
            MyCryptoStream.FlushFinalBlock();
            MyCryptoStream.Close();
            byte[] bTmp = ms.ToArray();
            ms.Close();
            return bTmp;
        }
        //---------------------------------------------------------------------------------------------------
        public string Encry(string myData)
        {
            byte[] data = Encode.GetBytes(myData);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = Key;
            DES.IV = IV;
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }
        public string Decry(string myData)
        {
            string[] mystring = myData.Split("-".ToCharArray());
            byte[] mybyte = new byte[mystring.Length];
            for (int i = 0; i < mystring.Length; i++)
            {
                mybyte[i] = byte.Parse(mystring[i], NumberStyles.HexNumber);
            }
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = Key;
            DES.IV = IV;
            ICryptoTransform desencrypt = DES.CreateDecryptor();
            byte[] result = desencrypt.TransformFinalBlock(mybyte, 0, mybyte.Length);
            return Encode.GetString(result);
        }
        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// 对字符串进行DES加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的BASE64编码的字符串</returns>
        public string Encrypt(string sourceString)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Encode.GetBytes(sourceString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    byte[] byte_ret = ms.ToArray();
                    return Convert.ToBase64String(byte_ret);
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 对字符串进行DES加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的BASE64编码的字符串</returns>
        public byte[] Encrypt(byte[] inData)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    byte[] byte_ret = ms.ToArray();
                    return byte_ret;
                    //return Convert.ToBase64String(byte_ret);
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 对DES加密后的字符串进行解密
        /// </summary>
        /// <param name="encryptedString">待解密的字符串</param>
        /// <returns>解密后的字符串</returns>
        public string Decrypt(string encryptedString)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Convert.FromBase64String(encryptedString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }

                    return Encode.GetString(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 对文件内容进行DES加密
        /// </summary>
        /// <param name="sourceFile">待加密的文件绝对路径</param>
        /// <param name="destFile">加密后的文件保存的绝对路径</param>
        public void EncryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);

            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 对文件内容进行DES加密，加密后覆盖掉原来的文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件的绝对路径</param>
        public void EncryptFile(string sourceFile)
        {
            EncryptFile(sourceFile, sourceFile);
        }

        /// <summary>
        /// 对文件内容进行DES解密
        /// </summary>
        /// <param name="sourceFile">待解密的文件绝对路径</param>
        /// <param name="destFile">解密后的文件保存的绝对路径</param>
        public void DecryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);

            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateDecryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 对文件内容进行DES解密，加密后覆盖掉原来的文件
        /// </summary>
        /// <param name="sourceFile">待解密的文件的绝对路径</param>
        public void DecryptFile(string sourceFile)
        {
            DecryptFile(sourceFile, sourceFile);
        }
    }
}
