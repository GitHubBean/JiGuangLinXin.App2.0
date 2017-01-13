using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;

namespace Com.Alipay.Mobile
{
    /// <summary>
    /// 类名：Config
    /// 功能：基础配置类
    /// 详细：设置帐户有关信息及返回路径
    /// 版本：3.3
    /// 日期：2012-07-05
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
    /// 
    /// 如何获取安全校验码和合作身份者ID
    /// 1.用您的签约支付宝账号登录支付宝网站(www.alipay.com)
    /// 2.点击“商家服务”(https://b.alipay.com/order/myOrder.htm)
    /// 3.点击“查询合作者身份(PID)”、“查询安全校验码(Key)”
    /// </summary>
    public class Config
    {
        #region 字段
        private static string partner = "";
        private static string private_key = "";
        private static string public_key = "";
        private static string input_charset = "";
        private static string sign_type = "";
        private static string seller_email = "";
        #endregion

        static Config()
        {
            //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            //合作身份者ID，以2088开头由16位纯数字组成的字符串
            partner = "2088121602726575";
            //商家的支付宝收款帐号
            seller_email = "1003339999@jgyx.com";

            //商户的私钥
            private_key = @"MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAMsnUtz5jopAa7bFUUK1/4gbhHrziWOl6LzIX5nNlwWgc3GL6jPXhyKXEuV52zHeOlV1VF1Nc5rAUFdyRnXe8Al8ClxdYejCIXSpRxQA1MKedZw6ifZZshTd7wkyMqfcJww+7eF/4YB/FwuusZZqKf0k8Q3WdHuU4RH+pA740hEvAgMBAAECgYBGiWHRDek7AYEk1cAQPKb7uCo4koSaj8mOergO6/5K2toai60G0Qe/r9rEyJmd5/4zG+jt+G1yRuHeavQiCwUmdqe9Gs0ed9ePBmZpFTr35Txg2x64ZlFKvjJvTlzAnecewZ6KxtZ1SYDcAEl84SNKXu/5NQXk/n1+klH684AJiQJBAP1fEVNSI4+DvtPvQ6cgCEg936W92432Ojr93zGJ6N1y5XCfw8iRuw7ofyFfSXuuMD9wKSa7PibRxsl1nkt5W90CQQDNQuHMVC4hJOJC3AtOTxGovEOLkMmRhGDV5H2YKnwGo1vXtP7Ri2s2IPbaryLK0XztTqui5arHPDRFQHil7sZ7AkEAvjQV34Sz6VKveI4PLXDghsrcD6IdJc8IG6zlVlz/EO7lysxEv1aXJDPo6/aKRWyYD6d1XPwHRkEIh8fiEyqBiQJAaiioLYxwGzY/S0MRGdwtDu7npDwq8/baOmWlS1jVsn00l/iFPgz0UxdzdKDVxr3X9cgVXveXftm1UwfIHlHDFwJAOMkpamHa5scQ5ztdyqEKO8cZ91ESNGYmWZ9E74gZ/3iX/DkBHs4BJViek/Z3+tyDhpHAUlAp8JuJRDrKMPklwQ==";

            //支付宝的公钥，无需修改该值
            public_key = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCnxj/9qwVfgoUh/y2W89L6BkRAFljhNhgPdyPuBV64bfQNN1PjbCzkIM6qRdKBoLPXmKKMiFYnkd6rAoprih3/PrQEB/VsW8OoM8fxn67UDYuyBTqA23MML9q1+ilIZwBC2AQ2UBVOrFXfFl75p6/B5KsiNG9zpgmLCUYuLkxpLQIDAQAB";

            //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑



            //字符编码格式 目前支持 gbk 或 utf-8
            input_charset = "utf-8";

            //签名方式，选择项：RSA、DSA、MD5
            sign_type = "RSA";
        }

        #region 属性
        /// <summary>
        /// 获取或设置合作者身份ID
        /// </summary>
        public static string Partner
        {
            get { return partner; }
            set { partner = value; }
        }

        /// <summary>
        /// 获取或设置商户的私钥
        /// </summary>
        public static string Private_key
        {
            get { return private_key; }
            set { private_key = value; }
        }

        /// <summary>
        /// 获取或设置支付宝的公钥
        /// </summary>
        public static string Public_key
        {
            get { return public_key; }
            set { public_key = value; }
        }

        /// <summary>
        /// 获取字符编码格式
        /// </summary>
        public static string Input_charset
        {
            get { return input_charset; }
        }

        /// <summary>
        /// 获取签名方式
        /// </summary>
        public static string Sign_type
        {
            get { return sign_type; }
        }
        /// <summary>
        /// 商家支付宝帐号（邮箱）
        /// </summary>
        public static string Seller_email
        {
            get { return seller_email; }
            set { seller_email = value; }
        }
        #endregion
    }
}