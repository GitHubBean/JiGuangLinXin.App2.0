using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;

namespace JiGuangLinXin.App.Test.Controllers
{
    public class EmojiController : ApiController
    {
        public HttpResponseMessage Get()
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
