using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JiGuangLinXin.App.Provide.StringHelper
{
    public static class StringExt
    {
        /// <summary>
        /// 获取字符串的MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5(this string str)
        {
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

                pwd = pwd + s[i].ToString("X");

            }
            return pwd;

        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string CutString(this string inputString, int len)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }
            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0;
            string tempString = "";
            byte[] s = ascii.GetBytes(inputString);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
                try
                {
                    tempString += inputString.Substring(i, 1);
                }
                catch
                {
                    break;
                }
                if (tempLen > len)
                    break;
            }
            //如果截过则加上半个省略号 
            byte[] mybyte = System.Text.Encoding.Default.GetBytes(inputString);
            if (mybyte.Length > len)
                tempString += "...";

            return tempString;
        }

        /// <summary>
        /// 去除字符串中的网页代码
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string removeHtml(this string strHtml)
        {
            Regex objRegExp = new Regex("<(.|\n)+?>");
            string strOutput = objRegExp.Replace(strHtml, "");
            strOutput = strOutput.Replace("<", "&lt;");
            strOutput = strOutput.Replace(">", "&gt;");

            Regex r = new Regex(@"\s+");
            strOutput = r.Replace(strOutput, "");

            strOutput = strOutput.Replace("&nbsp;", "");
            strOutput.Trim();

            return strOutput;
        }

        /// <summary>
        /// 验证是否为数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsUint(this string input)
        {
            Regex regex = new Regex("^[0-9]*[1-9][0-9]*$");
            return regex.IsMatch(input);
        }
        /// <summary>
        /// 验证是否符合价格格式
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPrice(this string input)
        {
            Regex regex = new Regex(@"^(0|([1-9]\d*))(\.\d+)?$");
            return regex.IsMatch(input);
        }

        /// <summary>
        /// 验证是否为E-mail
        /// </summary>
        /// <param name="str_Email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string str_Email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_Email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }


        /// <summary>
        /// 验证是否为手机号码
        /// </summary>
        /// <param name="phoneStr"></param>
        /// <returns></returns>
        public static bool IsMobilPhone(this string phoneStr)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phoneStr, @"^1[3|4|5|7|8][0-9]\d{4,8}$");
        }


        /// <summary>
        /// 获取汉字星期
        /// </summary>
        /// <param name="str_Email"></param>
        /// <returns></returns>
        public static string ToWeekDay(this string dayofweek)
        {
            string day = string.Empty;
            switch (dayofweek)
            {
                case "Sunday":
                    day = "星期日";
                    break;
                case "Monday":
                    day = "星期一";
                    break;
                case "Tuesday":
                    day = "星期二";
                    break;
                case "Wednesday":
                    day = "星期三";
                    break;
                case "Thursday":
                    day = "星期四";
                    break;
                case "Friday":
                    day = "星期五";
                    break;
                case "Saturday":
                    day = "星期六";
                    break;
            }
            return day;
        }
        /// <summary>
        /// 将是否项转换为中文的"是、否"字串
        /// </summary>
        /// <param name="isReverse">是否反序</param>
        /// <returns></returns>
        public static string ToChnBool(this bool isBool, bool isReverse, string yes, string no)
        {
            string green = "<span class='boxBorder boxGreen' >";
            string red = "<span class='boxBorder boxRed' >";
            string append = "</span>";
            return (isReverse) ? ((isBool) ? red + yes + append : green + no + append) : ((isBool) ? green + yes + append : red + no + append);
        }
    }    
}
