using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using JiGuangLinXin.App.Entities.ServicesModel;
using Newtonsoft.Json;

namespace JiGuangLinXin.App.App20Interface
{
    public class Juhe
    {
        public static string OpenID = "JHe21b9c6732c7beb9ee5c62b0da5c5348";

        //水电气缴费
        public static string PublicSite = "http://op.juhe.cn/ofpay/public/";
        public static string PublicKey = "b40ddcf722cd26b1eb61557e007b8ea6";

        //手机话费充值
        public static string MobileSite = "http://op.juhe.cn/ofpay/mobile/";
        public static string MobileKey = "1701509f09e1c77a9f141a26d79742d2";


        //手机流量充值
        public static string TrafficSite = "http://v.juhe.cn/flow/recharge";
        public static string TrafficKey = "24d74dec95dbddd4b1952a3ba422a4d2";


        public static void Update(string filename)
        {
            List<string> codes = new List<string>();
            codes.Add("delete AppPayment;");//删除以前的数据

            string nullStr = "";
            //得到省份列表
            JuheProvinceList provinces = JsonConvert.DeserializeObject<JuheProvinceList>(Client(PublicSite + "province?key=" + PublicKey));
            foreach (var province in provinces.result)
            {
                //得到城市列表
                JuheCityList citys = JsonConvert.DeserializeObject<JuheCityList>(Client(PublicSite + "city?key=" + PublicKey + "&provid=" + province.provinceId));
                if (citys.result == null) continue;
                foreach (var city in citys.result)
                {
                    //得到充值类型
                    JuheProjectList projects = JsonConvert.DeserializeObject<JuheProjectList>(Client(PublicSite + "project?key=" + PublicKey + "&provid=" + province.provinceId + "&cityid=" + city.cityId));
                    if (projects.result == null) continue;
                    foreach (var project in projects.result)
                    {
                        //获得缴费单位
                        JuheUnitList units = JsonConvert.DeserializeObject<JuheUnitList>(Client(PublicSite + "unit?key=" + PublicKey + "&provid=" + province.provinceId + "&cityid=" + city.cityId + "&type=" + project.payProjectId));
                        if (units.result == null) continue;
                        foreach (var unit in units.result)
                        {
                            try
                            {
                                //获得缴费方式
                                JuheProductList products = JsonConvert.DeserializeObject<JuheProductList>(Client(PublicSite + "query?key=" + PublicKey + "&provid=" + province.provinceId + "&cityid=" + city.cityId + "&type=" + project.payProjectId + "&code=" + unit.payUnitId));

                                if (products.result != null)
                                {
                                    codes.Add("insert Core_JuheDept values('"
                                        + province.provinceId + "','" + province.provinceName + "','"
                                        + city.cityId + "','" + city.cityName + "','"
                                        + project.payProjectId + "','" + project.payProjectName + "','"
                                        + unit.payUnitId + "','" + unit.payUnitName + "',"
                                        + products.result.productId + ",'" + products.result.productName + "')");
                                }
                                else
                                {
                                    nullStr += unit.payUnitName + "--" + "&provid=" + province.provinceId + "&cityid=" +
                                               city.cityId + "&type=" + project.payProjectId + "&code=" + unit.payUnitId + "--\n";
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            codes.Add(nullStr);
            File.WriteAllLines(filename, codes.ToArray());
        }

        public static string Client(string url)
        {
            string value = "";
            try
            {

                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            value = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                int a = 0;
            }
            return value;
        }


        /// <summary>
        /// MD5加密算法
        /// </summary>
        public static string MakeMD5(string data)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(data, "MD5").ToLower();
        }
    }
}