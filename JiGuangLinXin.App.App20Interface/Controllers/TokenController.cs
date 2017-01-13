using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 口令
    /// </summary>
    public class TokenController : ApiController
    {
        private UserCore uCore = new UserCore();
        /// <summary>
        /// 更新口令token
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage RefreshToken()
        {
            ResultViewModel rs = new ResultViewModel();
            int platform = int.Parse(Request.Headers.GetValues("platform").FirstOrDefault());
            string uid = Request.Headers.GetValues("uid").FirstOrDefault();

            string pwd = HttpContext.Current.Request.Form["pwd"];
            Guid gId = Guid.Empty;
            if (Guid.TryParse(uid, out gId))
            {
                var obj = uCore.LoadEntity(o => o.U_Id == gId);
                if (obj != null)
                {
                    string dePwd = DESProvider.DecryptString(pwd);  //传递过来的密码是客户端加密过后的

                    string enPwd = Md5Extensions.MD5Encrypt(dePwd + obj.U_PwdCode);
                    if (obj.U_LoginPwd == enPwd)  //密码正确
                    {
                        TokenViewModel vm = new TokenViewModel() { Phone = obj.U_LoginPhone, Platform = platform, Time = DateTime.Now, Uid = uid,AreaCode = obj.U_AreaCode };

                        string token = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(vm));
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = token;
                        //return WebApiJsonResult.ToJson(new ResultViewModel() { State = 0, Msg = "ok", Data = token });
                    }
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

    }
}
