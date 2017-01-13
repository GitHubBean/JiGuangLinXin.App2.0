using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services.weixin;
using Microsoft.Ajax.Utilities;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    public class CommonController : ApiController
    {
        /// <summary>
        /// 通用图片上传
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage UploadImg()
        {
            ResultViewModel rs = new ResultViewModel();
            var files = HttpContext.Current.Request.Files;
            string folder = HttpContext.Current.Request.Form["folder"];
            if (files.Count > 0 && !string.IsNullOrEmpty(folder))
            {
                var file = files[0];

                //文件上传的路径
                string basePath = ConfigurationManager.AppSettings["StaticFilePath"] + folder + "\\";
                if (!Directory.Exists(basePath))  //不存在就创建目录
                    Directory.CreateDirectory(basePath);

                int sizeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["ImgSize"]);  //这里暂时只验证图片（如果后期可以传视频，继续扩展）
                if (sizeLimit < file.ContentLength)
                {
                    rs.Msg = "文件太大超出限制";
                }
                else
                {
                    CreateRandomStr rdm = new CreateRandomStr();
                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                    file.SaveAs(basePath + fileName);//上传文件
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        filename = ConfigurationManager.AppSettings["ImgSiteUrl"] + folder + fileName
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// app启动页面广告
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage StartPageAdImg()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            string platform = HttpContext.Current.Request.Form["platform"]; //平台标识

            var url = System.Configuration.ConfigurationManager.AppSettings["StartPageAdImg"].Split('#');

            rs.Data = new
            {
                imgUrl = url[0] + "?v=" + platform + DateTime.Now.Ticks,
                version = url[1]

            };
            return WebApiJsonResult.ToJson(rs);
        }


        //
        // GET: /Common/


        /// <summary>
        /// 选择获取所有城市以及热门城市
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage CityQuery()
        {
            ResultViewModel rs = new ResultViewModel();

            CityCore cityCore = new CityCore();
            var list1 = cityCore.LoadEntities().ToList().OrderBy(o => o.C_Pinyin);
            var list2 = list1.Where(o => o.C_Hot == 1).ToList().OrderBy(o => o.C_Pinyin);
            var jsonObj =
                new
                {
                    hotCity =
                        list2.Select(
                            s =>
                                new
                                {
                                    city = s.C_Name,
                                    code = s.C_LevelCode,
                                    py = s.C_Pinyin.Substring(0, 1),
                                    areaCode = s.C_LevelCode
                                }),
                    allCity =
                        list1.Select(
                            s =>
                                new
                                {
                                    city = s.C_Name,
                                    code = s.C_LevelCode,
                                    py = s.C_Pinyin.Substring(0, 1),
                                    areaCode = s.C_LevelCode
                                })
                };

            //返回的结果集
            rs.Data = jsonObj;
            rs.Msg = "ok";
            rs.State = 0;
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 获得微信签名
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage WechatSignature()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            var appId = System.Configuration.ConfigurationManager.AppSettings["AppID"];
            var appSecret = System.Configuration.ConfigurationManager.AppSettings["appSecret"];
            string token = BasicAPI.GetAccessToken(appId, appSecret).access_token;
            var domain = System.Configuration.ConfigurationManager.AppSettings["Domain"];

            var url = domain + Request.RequestUri.PathAndQuery;

            var nonceStr = Util.CreateNonce_str();
            var timestamp = Util.CreateTimestamp();
            var string1 = "";
            string jsTickect = JSAPI.GetTickect(token).ticket;
            var signature = JSAPI.GetSignature(jsTickect, nonceStr, timestamp, url, out string1);
            rs.Data = new
             {
                 appId = appId,
                 nonceStr = nonceStr,
                 signature = signature,
                 timestamp = timestamp,
                 jsapiTicket = jsTickect,
                 string1 = string1,
             };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Des()
        {
            ResultViewModel rs = new ResultViewModel();

            string str = HttpContext.Current.Request.Form["str"];


            //返回的结果集
            rs.Data = DESProvider.EncryptString(str);
            rs.Msg = "ok";
            rs.State = 0;
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 返回话费充值时，可选的各种金额
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PhoneMoneyList()
        {
            ResultViewModel rs = new ResultViewModel();
            //返回的结果集
            var list = ConfigurationManager.AppSettings["phoneMoneyList"].Split('|');


            //rs.Data = list.Select(o =>  new
            //{
            //    discountMoney = Convert.ToDouble(o) * 0.8,
            //    money = o
            //});

            //查看是否已经参加过活动了
            //Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            //var bl = new BillMemberCore().LoadEntity(o => o.B_UId == uid && o.B_Module == (int)BillEnum.话费充值85折);
            //if (bl != null)   //已经享受过85折充值优惠，特别声明：下面两个属性由于某些蛋疼的原因，被互换了，切记切记
            //{
            //    rs.Data = list.Select(o => new
            //    {
            //        discountMoney = o.Split(',')[1],  //原价
            //        money = o.Split(',')[1]  //原价
            //    });
            //}
            //else
            //{
            //    rs.Data = list.Select(o => new
            //    {
            //        discountMoney = o.Split(',')[1],  //原价
            //        money = o.Split(',')[0]  //折扣价
            //    });
            //}


            rs.Data = list.Select(o => new
            {
                discountMoney = o.Split(',')[1],  //原价
                money = o.Split(',')[0]  //折扣价
            });


            rs.Msg = "ok";
            rs.State = 0;
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 获得平台兑换的比率
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage DiscountRate()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            DiscountRateCore rateCore = new DiscountRateCore();
            var rate = rateCore.LoadEntity(o => o.R_State == 0);
            rs.Data = new
            {
                time = rate.R_Time,
                busRate = rate.R_GoodsRate,
                payRate = rate.R_PayRate
            };
            return WebApiJsonResult.ToJson(rs);
        }


        ///// <summary>
        ///// 协议公告
        ///// </summary>
        ///// <returns></returns>
        //public HttpResponseMessage Protocol()
        //{
        //    ResultViewModel rs = new ResultViewModel(0, "ok", null);
        //    string flag = HttpContext.Current.Request.Form["flag"];

        //    BaseInfoCore bCore = new BaseInfoCore();

        //    var info = bCore.LoadEntity(o => o.B_Code == flag);

        //    //如果协议的扩展字段不为空，标识跳转一个指定的url，如果为空，跳转到对应的通用页面
        //    string url = string.IsNullOrEmpty(info.B_ExProp)
        //        ? System.Configuration.ConfigurationManager.AppSettings["OutsideUrl"] + "Protocol/Index" + info.B_Id
        //        : info.B_ExProp;

        //    rs.Data = new
        //    {
        //        url
        //    };
        //    return WebApiJsonResult.ToJson(rs);
        //}



        /// <summary>
        /// 第三方的url(协议、关于我们、邻里团、活动、新盘等)
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage QueryOutsideUrl()
        {

            string outsideUrl = ConfigurationManager.AppSettings["OutsideUrl"];

            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            int flag = Convert.ToInt32(HttpContext.Current.Request.Form["flag"]);  //标识
            string id = HttpContext.Current.Request.Form["id"];  //项目ID
            string uid = HttpContext.Current.Request.Form["uid"];  //用户ID


            string url = "";


            switch (flag)
            {
                case (int)ProtocolEnum.关于我们:
                case (int)ProtocolEnum.用户协议:
                case (int)ProtocolEnum.邻里团介绍:
                    //如果协议的扩展字段不为空，标识跳转一个指定的url，如果为空，跳转到对应的通用页面
                    BaseInfoCore bCore = new BaseInfoCore();
                    var info = bCore.LoadEntity(o => o.B_Code == flag.ToString());
                    url = string.IsNullOrEmpty(info.B_ExProp)
                      ? outsideUrl + "Protocol/" + info.B_Id
                      : info.B_ExProp;
                    break;
                case (int)ProtocolEnum.邻里团:
                    //url = outsideUrl + "GroupBuy/" + id;
                    url = outsideUrl + "/html/llt/index.html?gid=" + id;
                    break;
                case (int)ProtocolEnum.社区活动:
                    //url = outsideUrl + "OutsideUrl/" + "event/" + uid + "_" + id;
                    Guid proId = Guid.Parse(id);
                    var eventInfo = new EventCore().LoadEntity(o => o.E_Id == proId);
                    if (eventInfo.E_Flag == (int)EventFlagEnum.投票)
                    {
                        url = string.Format("{0}html/eventVote/index.html?proId={1}&uid={2}", outsideUrl, proId, uid);// outsideUrl + "/html/eventVote/index.html?proId=" + id;    
                    }
                    else if (eventInfo.E_Flag == (int)EventFlagEnum.报名)
                    {
                        url = string.Format("{0}html/eventApply/index.html?proId={1}&uid={2}", outsideUrl, proId, uid);
                    }
                    else
                    {
                        url = ConfigurationManager.AppSettings["H5AppDownloadUrl"];
                    }
                    break;
                case (int)ProtocolEnum.楼盘推荐:
                    //url = outsideUrl + "sp/" + uid + "_" + id;
                    //url = outsideUrl + "/html/xjtj/index.html?share=true";
                    string tip = HttpContext.Current.Request.Form["tip"];  //tip 1，标识是APP内部打开的链接，点击活动中的参加，则调用参加接口；否则标示是分享链接，点击参加直接跳转APPH5下载
                    //url = string.Format("{0}html/xjtj/index.html?buildingId={1}&userId={2}", outsideUrl, id, uid);

                    if (tip == "1")  //tip 分享0 广告1
                    {
                        url = string.Format("{0}html/xjtj/index.html?buildingId={1}&userId={2}", outsideUrl, id, uid);  //显示红包、隐藏顶部tip下载
                    }
                    else
                    {
                        url = string.Format("{0}html/building/index.html?bId={1}&uId={2}&tip=1", outsideUrl, id, uid);
                    }

                    break;
                case (int)ProtocolEnum.App下载H5:
                    url = ConfigurationManager.AppSettings["H5AppDownloadUrl"]; // outsideUrl + "Download/Index" ;
                    break;
                case (int)ProtocolEnum.全景看房:
                    //根据户型的ID查询楼盘，如果楼盘的flag为0默认官方户型（默认户型的全景是固定的）、如果为1，则是正式户型（户型根据楼盘ID作为文件夹名字）
                    var cbId = Guid.Parse(id);
                    var cube = new BuidingCubeCore().LoadEntity(o => o.BC_Id == cbId);
                    var bInfo = new BuildingCore().LoadEntity(o => o.B_Id == cube.BC_BuildingId);
                    if (bInfo.B_Flag == 0)
                    {
                        url = string.Format("{0}building/cube.html?startscene={2}&id={1}", ConfigurationManager.AppSettings["VRUrl"], id, cube.BC_Rank);
                    }
                    else
                    {
                        url = string.Format("{0}{1}/cube.html?startscene={3}&id={2}", ConfigurationManager.AppSettings["VRUrl"], bInfo.B_Id.ToString("N"), id, cube.BC_Rank);
                    }
                    break;
                case (int)ProtocolEnum.游戏中心:
                    url = string.Format("{0}index.html?uid={1}", ConfigurationManager.AppSettings["GamesUrl"], uid);
                    break;
                case (int)ProtocolEnum.社区中心:
                    url = string.Format("{0}html/groupCenter/index.html?buildingId={1}&uId={2}&tip=1", outsideUrl, id, uid);
                    break;
                case (int)ProtocolEnum.商家服务:
                    url = string.Format("{0}html/ServiceCenter/index.html?buildingId={1}&uId={2}&tip=1", outsideUrl, id, uid);
                    break;
                case (int)ProtocolEnum.群主中心:
                    //url = string.Format("{0}html/adminCenter/#/home/{1}", outsideUrl, uid);
                    url = string.Format("http://192.168.3.178:8008/#/home/{0}", uid);
                    break;

            }


            rs.Data = new
            {
                url
            };
            return WebApiJsonResult.ToJson(rs);
        }




        /// <summary>
        /// 上传应用程序的错误日志（ANDROID IOS）
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage UploadApplicationLog()
        {
            ResultViewModel rs = new ResultViewModel();
            int driver = Convert.ToInt32(HttpContext.Current.Request.Form["flag"]);  //标识 1android 2ios
            string phone = HttpContext.Current.Request.Form["phone"];  //用户phone
            string uidStr = HttpContext.Current.Request.Form["uid"];  //用户ID
            string phoneType = HttpContext.Current.Request.Form["phoneType"];  //手机型号
            string desc = HttpContext.Current.Request.Form["desc"];  //描述

            Guid uid = Guid.Parse(uidStr);


            ErrorLogCore core = new ErrorLogCore();

            Sys_ErrorLog log = new Sys_ErrorLog()
            {
                L_Desc = desc,
                L_DriverType = driver,
                L_Phone = phone,
                L_UId = uid,
                L_Url = phoneType,
                L_Time = DateTime.Now,
                L_Flag = 0  //默认
            };

            if (core.AddEntity(log) != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        #region  App 检查版本更新、检测插件更新
        /// <summary>
        /// 检测是否有新版本
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage CheckVersion()
        {
            ResultViewModel rs = new ResultViewModel(1, "当前已是最新版本", null);

            string version = HttpContext.Current.Request.Form["version"];

            int flag = Convert.ToInt32(HttpContext.Current.Request.Form["flag"]);


            AppVersionCore avCore = new AppVersionCore();

            var appVersion = avCore.LoadEntities(o => o.V_Flag == flag);//获取所有的android或者android插件

            if (flag == 0)  //检查 android插件
            {

                string fn = HttpContext.Current.Request.Form["fn"];  //插件名
                var plugin = appVersion.Where(o => o.V_FileName == fn).OrderByDescending(o => o.V_Id).FirstOrDefault();
                if (plugin != null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";

                    if (!string.IsNullOrEmpty(version) && version.Equals(plugin.V_Code))
                    {
                        rs.Data = null;
                    }
                    else
                    {
                        rs.Data = new
                        {
                            download = string.Format("{0}{1}/{2}", ConfigurationManager.AppSettings["ImgSiteUrl"], AttachmentFolderEnum.appdownload, plugin.V_FileName),
                            version = plugin.V_Code
                        };
                    }
                }
                else
                {
                    rs.State = 1;
                    rs.Msg = "没有更多插件";
                }
            }
            else if (flag == 1) //检查android 应用版本
            {

                var cur = avCore.LoadEntities(o => o.V_Code == version).OrderByDescending(o => o.V_Time).FirstOrDefault(); //当前版本
                if (cur.V_State == 1)
                {
                    rs.State = 0;
                    rs.Msg = "当前版本已被禁用";
                    rs.Data = new
                    {
                        state = 1,
                        tips = cur.V_ForzenTips
                    };
                }
                else
                {

                    var last = appVersion.OrderByDescending(o => o.V_Id).FirstOrDefault(); //最新版本号

                    if (!string.IsNullOrEmpty(version) && last != null)
                    {
                        //if (version.Equals(last.V_Code))
                        //{
                        //    rs.State = 1;
                        //    rs.Msg = "当前已是最新版本";
                        //}


                        Version v1 = new Version(version);
                        Version v2 = new Version(last.V_Code);


                        if (v2 > v1)  //有新版本更新 !version.Equals(last.V_Code)
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                            rs.Data = new
                            {
                                download = string.Format("{0}{1}/{2}", ConfigurationManager.AppSettings["ImgSiteUrl"], AttachmentFolderEnum.appdownload, last.V_FileName)
                            };
                        }
                    }
                }
            }

            //version = version.Replace(".", "");
            //int ver = 0;
            //if (int.TryParse(version,out ver))
            //{
            //}

            return WebApiJsonResult.ToJson(rs);
        }

        #endregion

        [System.Web.Http.HttpOptions]
        public string Options()
        {

            return null;
        }
    }
}
