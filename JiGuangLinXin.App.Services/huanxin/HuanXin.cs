using System.Configuration;
using Newtonsoft.Json;

namespace JiGuangLinXin.App.Services
{
    public static class HuanXin
    {
        //public static string ClientID = "YXA6eP70oIHVEeWiInF3_PXfSw";
        //public static string ClientSecret = "YXA6zmk7lBIzNMTZDXeyzF8u9mlLn4Q";
        //public static string Name = "lx";
        //public static string OrgName = "linxin2015";

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="nickname">昵称</param>
        /// <returns></returns>
        public static bool CreateUser(string UserName, string nickname = "")
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string rs = ease.AccountCreate(UserName, "linxing", nickname);
            return true;
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <returns></returns>
        public static bool RemoveUser(string UserName)
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string rs = ease.AccountDel(UserName);
            return true;
        }
        /// <summary>
        /// 修改用户昵称
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool AccountResetNickname(string userName, string nickname)
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string r = ease.AccountResetNickname(userName, nickname);
            return true;
        }

        /// <summary>
        /// 加入群
        /// </summary>
        /// <param name="qunId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool AccountQunJoin(string qunId, string userName)
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string r = ease.AccountQunJoin(qunId, userName);
            return true;
        }
        /// <summary>
        /// 创建群
        /// </summary>
        /// <param name="GroupName"></param>
        /// <returns></returns>
        public static string CreateQun(string GroupName)
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string code = ease.QunCreate(GroupName, "邻信小区群", "true", "999", "false");
            HuanXinQun qun = JsonConvert.DeserializeObject<HuanXinQun>(code);
            return qun.data.groupid;

        }
        /// <summary>
        /// 退出群主
        /// </summary>
        /// <param name="qunId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool ExitQun(string qunId, string userName)
        {
            EaseMobDemo ease = new EaseMobDemo(ConfigurationManager.AppSettings["HxClientID"], ConfigurationManager.AppSettings["HxClientSecret"], ConfigurationManager.AppSettings["HxName"], ConfigurationManager.AppSettings["HxOrgName"]);
            string r = ease.ExitQun(qunId, userName);
            return true;
        }

    }

    public class HuanXinQun
    {
        public HuanXinQunData data;
    }
    public class HuanXinQunData
    {
        public string groupid;
    }
}