using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{

    /// <summary>
    /// 审核管理
    /// </summary>
    public class CheckController : BaseController
    {
        private VillageCore buildingCore = new VillageCore();

        private AuditingVillageCore vCore = new AuditingVillageCore();
        private AuditingGroupManangerCore amCore = new AuditingGroupManangerCore();
        private UserCore uCore = new UserCore();
        private AuditingCashCore cashCore = new AuditingCashCore();
        private CheckHistoryCore chCore = new CheckHistoryCore();

        #region 用户社区认证
        /// <summary>
        /// 用户社区认证审核
        /// </summary>
        /// <returns></returns>
        public ActionResult UserReg(int id = 1, int size = 10, string key = "")
        {
            //AuditingVillageCore audCore = new AuditingVillageCore();
            //return Content((audCore.CreateHuanxinGroup("LX00000")));
            //string cc = audCore.CreateHuanxinGroup("D6EC30C0-72A0-45FD-A8AC-44E06CE1D7FD");
            //return Content(cc);

            Expression<Func<Core_AuditingVillage, Boolean>> expr = t => true;
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.A_UPhone.Contains(key) || t.A_TrueName.Contains(key) || t.A_BuildingName.Contains(key);
            }

            var list = vCore.LoadEntities(expr).OrderByDescending(o => o.A_Time).ThenBy(o => o.A_Status).ThenBy(o => o.A_BuildingId).ToPagedList(id, size);
            return View(list);
        }


        /// <summary>
        /// 认证结果
        /// </summary>
        /// <param name="id">认证的记录id</param>
        /// <returns></returns>
        public ActionResult Allow(Guid id)//Guid id,int state = 1
        {
            var obj = vCore.LoadEntity(p => p.A_Id == id && p.A_Status == (int)AuditingEnum.未认证);
            if (obj == null)
            {
                return Content("数据不存在");
            }
            //if (state == 2)  //拒绝
            //{
            //    obj.A_Status = 2;
            //    vCore.UpdateEntity(obj);
            //    return Content("ok");
            //} 
            //通过
            //超级管理员如果要审核通过，那么需要先确定小区是否存在
            return View(obj);
            //return View(new Core_AuditingVillage());
        }

        /// <summary>
        /// 检查审核的小区是否存在
        /// </summary>
        /// <param name="sName">小区名字</param>
        /// <returns></returns>
        [HttpPost]
        public string BuildingNameIsExist(string sName)
        {
            Guid sid;
            if (Guid.TryParse(sName, out sid))
            {
                var obj = buildingCore.LoadEntity(o => o.V_Id == sid);
                if (obj != null)
                {
                    return string.Format("{0},{1}【{2}】", obj.V_CityName, obj.V_DistrictName, obj.V_BuildingName);
                }
            }
            return "err";
        }

        /// <summary>
        /// 审核 社区认证
        /// </summary>
        /// <param name="id">认证的序号</param>
        /// <param name="buildName">小区name</param>
        /// <param name="buildId">小区ID</param>
        /// <param name="state">状态（1通过 2拒绝）</param>
        /// <returns></returns>
        [HttpPost]
        public string Auditing(Guid id, Guid? buildId, int state, string tips = "")
        {

            var aud = vCore.LoadEntity(o => o.A_Id == id && o.A_Status == (int)AuditingEnum.未认证);
            if (aud == null)  //记录不存在
            {
                //之前逻辑是，管理员不能擅自更改用户社区认证的小区
                //if (aud.A_BuildingId == Guid.Empty && !string.IsNullOrEmpty(buildName) && state == (int)AuditingEnum.认证成功)  //通过审核，如果申请的小区不存在，超管新增了小区后，更改申请
                //{
                //    //aud.A_CheckBack = string.Format("#新增小区：[{0},{1}],申请社区认证的小区名：{2}#", buildId, buildName, aud.A_BuildingName);
                //    aud.A_CheckBack = tips;
                //    aud.A_BuildingName = buildName;
                //    aud.A_BuildingId = (Guid)buildId;
                //    aud.A_CheckTime = DateTime.Now;

                //    if (!vCore.UpdateEntity(aud))
                //    {
                //        return "err";
                //    }
                //}
                return "err";
            }

            //开始审核
            var flag = vCore.BuildingAuditingCheck(id, GetUser().A_Account, GetUser().A_Id, buildId, state, tips);

            #region 消息推送
            JPushMsgModel jm = new JPushMsgModel()
            {
                code = (int)MessageCenterModuleEnum.邻妹妹,
                proFlag = state == 1 ? (int)PushMessageEnum.审核通过 : (int)PushMessageEnum.审核失败,
                proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tags = "社区认证",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                tips = state == 1 ? "您已成功入驻自家小区:" + aud.A_BuildingName + "，认识更多邻居，玩转邻里圈、邻里团、畅享社区服务尽在邻信1.0。" : "您的社区认证未通过审核，原因：" + tips,
                title = " 您的社区认证已被处理"
            };
            Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), aud.A_UId.ToString("N").ToLower());

            //if (state == 1)  //通过认证
            //{
            //    //查询小区是否有管理员
            //    var mgr =
            //        uCore.LoadEntities(o => o.U_BuildingId == buildId && o.U_AuditingManager == (int)AuditingEnum.认证成功);

            //    if (!mgr.Any())
            //    {
            //        JPushMsgModel jm1 = new JPushMsgModel()
            //        {
            //            code = (int)MessageCenterModuleEnum.邻妹妹,
            //            proFlag = (int)PushMessageEnum.社区活动跳转,
            //            proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //            tags = "招聘小区管理员",
            //            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //            tips = "欢迎您入驻自家小区，邻信管理员招聘火热进行中，快来参加吧",
            //            title = "邻信管理员招聘火热进行中，快来参加吧",
            //            logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/mgrInvite/index.html",
            //            proName = "曝光社区管理员的逆天福利"
            //        };
            //        Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), aud.A_UId.ToString("N").ToLower());

            //    }
            //}
            #endregion

            if (flag)
            {
                return "ok";
            }
            return "err";
        }

        #endregion

        #region 小区用户管理员申请

        /// <summary>
        /// 小区管理员申请审核
        /// </summary>
        /// <returns></returns>
        public ActionResult GroupManager(int id = 1, int size = 10, string key = "")
        {

            Expression<Func<Core_AuditingGroupMananger, Boolean>> expr = t => true;
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.M_Phone.Contains(key) || t.M_TrueName.Contains(key) || t.M_BuildingName.Contains(key);
            }

            var list = amCore.LoadEntities(expr).OrderBy(o => o.M_Status).ThenByDescending(o => o.M_Time).ToPagedList(id, size);
            return View(list);
        }
        /// <summary>
        /// 审核群主管理员
        /// </summary>
        /// <param name="id">申请的ID</param>
        /// <param name="state">1通过 2 拒绝</param>
        /// <returns></returns>
        [HttpPost]
        public string GroupManagerAuditing(Guid id, int state, string tips = "")
        {
            string rs = "err";
            var aud = amCore.LoadEntity(o => o.M_Id == id && o.M_Status == (int)AuditingEnum.未认证);
            if (aud != null)  //记录存在
            {
                if (state == (int)AuditingEnum.认证成功)   //通过审核
                {
                    //检查当前社区是否有管理员
                    //var uv =
                    //    uCore.LoadEntities(
                    //        o => o.U_BuildingId == aud.M_BuildingId && o.U_AuditingManager == (int) AuditingEnum.认证成功).Count();
                    //if (uv>0)  //已经有个管理员了
                    //{
                    //    return "此社区管理员已经有管理员，请问重复申请";
                    //}
                }

                aud.M_Status = state;
                aud.M_CheckTime = DateTime.Now;
                aud.M_CheckBack = tips;

                bool flag = amCore.UpdateEntity(aud);
                if (flag)
                {
                    var user = uCore.LoadEntity(o => o.U_Id == aud.M_UId);
                    user.U_AuditingManager = state;
                    uCore.UpdateEntity(user);

                    //审核记录
                    chCore.AddEntity(new Sys_CheckHistory()
                    {
                        H_AdminId = GetUser().A_Id,
                        H_AdminName = GetUser().A_Name,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.群管理员审核,
                        H_ProId = aud.M_Id.ToString(),
                        H_ProName = aud.M_TrueName,
                        H_Role = 0,//0超级管理员 1商家管理员
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = tips
                    });

                    rs = "ok";

                    //var flag = uCore.UpdateByExtended(o => new Core_User() { U_AuditingManager = state },
                    //    o => o.U_Id == aud.M_UId);
                    //if (flag > 0)  //处理成功
                    //{
                    //}
                }

                #region 消息推送
                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = state == 1 ? (int)PushMessageEnum.管理员审核通过 : (int)PushMessageEnum.管理员审核失败,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "小区管理员认证",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = state == 1 ? "您的小区管理员申请已经通过审核" : "您的小区管理员申请未通过审核，原因：" + tips,
                    title = " 您的小区管理员申请已被处理"
                };
                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), aud.M_UId.ToString("N").ToLower());



                JPushMsgModel jm1 = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.社区活动跳转,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "管理员课堂",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = "热烈庆祝您已成功当上咱们社区的管理员。好好学习，天天拿福利，么么哒！加油！",
                    title = "邻信管理员课堂开课啦",
                    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/classroom/index.html",
                    proName = "管理员课堂第一讲：管理员晋升法则"
                };
                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm1), aud.M_UId.ToString("N").ToLower());


                JPushMsgModel jm11 = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.社区活动跳转,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "招聘小区管理员",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = "欢迎您入驻自家小区，邻信管理员招聘火热进行中，快来参加吧",
                    title = "邻信管理员招聘火热进行中，快来参加吧",
                    logo = ConfigurationManager.AppSettings["OutsideUrl"] + "html/mgrInvite/index.html",
                    proName = "邻信社区小管家聘请中"
                };
                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm1.title, jm1.title, JsonSerialize.Instance.ObjectToJson(jm11), aud.M_UId.ToString("N").ToLower());



                #endregion

            }
            return rs;
        }

        #endregion


        #region 提现申请

        /// <summary>
        /// 提现申请审核
        /// </summary>
        /// <returns></returns>
        public ActionResult CashingOut(int id = 1, int size = 10, string key = "")
        {

            Expression<Func<Core_AuditingCash, Boolean>> expr = t => true;
            key = key.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                expr = t => t.M_Phone.Contains(key) || t.M_BankAccount.Contains(key) || t.M_BankName.Contains(key);
            }
            var list = cashCore.LoadEntities(expr).OrderBy(o => o.M_Status).ThenByDescending(o => o.M_Time).ToPagedList(id, size);

            return View(list);
        }

        /// <summary>
        /// 审核提款
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public string CashingOutAuditing(Guid id, int state, string tips = "")
        {
            var aud = cashCore.LoadEntity(o => o.M_Id == id && o.M_Status == (int)AuditingEnum.未认证);
            if (aud != null)  //记录存在
            {
                aud.M_Status = state;
                aud.M_CheckTime = DateTime.Now;
                //aud.M_CheckBack = string.Format("#审核人：{0},{1}#", GetUser().A_Id, GetUser().A_Name);
                aud.M_CheckBack = tips;

                var rs = cashCore.ApplyCashAuditing(aud);


                //审核记录
                chCore.AddEntity(new Sys_CheckHistory()
                {
                    H_AdminId = GetUser().A_Id,
                    H_AdminName = GetUser().A_Name,
                    H_CheckState = state,
                    H_Flag = (int)CheckHistoryStateEnum.商家提现,
                    H_ProId = aud.M_Id.ToString(),
                    H_ProName = aud.M_BankAccount,
                    H_Role = 0,//0超级管理员 1商家管理员
                    H_State = 0,
                    H_Time = DateTime.Now,
                    H_Tips = tips
                });

                return rs.Msg;
            }
            return "err";
        }


        #endregion


    }
}
