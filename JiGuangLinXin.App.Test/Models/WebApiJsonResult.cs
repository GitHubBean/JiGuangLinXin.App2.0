using System;
using System.Net.Http;
using System.Text;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.Test
{
    public class WebApiJsonResult
    {
        public static HttpResponseMessage ToJson(Object obj)
        {
            String str;
            if (obj is String || obj is Char)
            {
                str = obj.ToString();
            }
            else
            {
                str = JsonSerialize.Instance.ObjectToJson(obj);
            }
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        } 
    }

}