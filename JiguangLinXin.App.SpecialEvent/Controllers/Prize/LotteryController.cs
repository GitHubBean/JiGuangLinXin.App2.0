using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiguangLinXin.App.SpecialEvent.Models;
using JiGuangLinXin.App.App20Interface.Extension;
using Newtonsoft.Json.Linq;

namespace JiguangLinXin.App.SpecialEvent.Controllers.Prize
{
    public class RewardController : ApiController
    {
         

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Turn([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string uid = obj.uid;
             

            return WebApiJsonResult.ToJson(rs);
        }
    }
}
