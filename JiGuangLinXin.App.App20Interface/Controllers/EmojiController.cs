using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 系统表情包
    /// </summary>
    public class EmojiController : ApiController
    {
        public HttpResponseMessage List()
        {

            string StaticFilePath = ConfigurationManager.AppSettings["StaticFilePath"]; 
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            string listJson = new ReadWriteFile().ReadFile(StaticFilePath + "emoji/config.txt");

            if (string.IsNullOrEmpty(listJson))
            {
                rs.State = 1;
                rs.Msg = "没有更多表情";
            }
            else
            {
                rs.Data = JsonSerialize.Instance.JsonToObject<dynamic>(listJson);
            }

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
