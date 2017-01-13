using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;
using WebGrease.Css.Extensions;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 认证接口
    /// </summary>
    public class AuditingController : BaseController
    {
        private AuditingVillageCore core = new AuditingVillageCore();
        private UserCore uCore = new UserCore();
        private AuditingGroupManangerCore mCore = new AuditingGroupManangerCore();
        private VillageCore vCore = new VillageCore();

        /// <summary>
        /// 小区社区认证
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BuildingAuditing()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            var checkExist = core.LoadEntity(o => o.A_UId == uid && o.A_Status == (int)AuditingEnum.未认证);
            if (checkExist != null && !string.IsNullOrEmpty(checkExist.A_UPhone))
            {
                rs.State = 1;
                rs.Msg = "您已提交用户实名审核，请耐心等待！";
                return WebApiJsonResult.ToJson(rs);
            }

            string phone = Request.Headers.GetValues("phone").FirstOrDefault();

            Guid buildingId;
            Guid.TryParse(HttpContext.Current.Request.Form["buildingId"], out buildingId);

            string buildingName = HttpContext.Current.Request.Form["buildingName"];
            string doorNo = HttpContext.Current.Request.Form["doorNo"];
            string trueName = HttpContext.Current.Request.Form["trueName"];
            string cityName = HttpContext.Current.Request.Form["cityName"];
            string areaCode = "";//new CityCore().LoadEntity(o => o.C_Name.Contains(cityName)).C_LevelCode;  //城市的区域code
            double coordX = Convert.ToDouble(HttpContext.Current.Request.Form["coordX"]);
            double coordY = Convert.ToDouble(HttpContext.Current.Request.Form["coordY"]);




            //new LogHelper().Write("-----------------------------------------------"+buildingName,LogLevel.Information);

            HttpPostedFile buildingImg = HttpContext.Current.Request.Files["buildingImg"];

            HttpPostedFile feeTicket = HttpContext.Current.Request.Files["feeTicket"];



            string bPath = "";
            string fPath = "";
            if (buildingImg != null) //小区图片存在
            {
                bPath = Guid.NewGuid().ToString("N") + Path.GetExtension(buildingImg.FileName);// DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) +Path.GetExtension(buildingImg.FileName);  
                // string.Format("{0}/{1}", AttachmentFolderEnum.identification, DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) + Path.GetExtension(buildingImg.FileName));


                //buildingImg.SaveAs(basePath + bPath);
                //Thread.Sleep(1000);
                UploadFileToServerPath(buildingImg, AttachmentFolderEnum.identification.ToString(), bPath);
            }


            if (feeTicket != null)  //缴费图片存在
            {
                fPath = Guid.NewGuid().ToString("N") + Path.GetExtension(feeTicket.FileName);// DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) +Path.GetExtension(feeTicket.FileName);  
                UploadFileToServerPath(feeTicket, AttachmentFolderEnum.identification.ToString(), fPath);
            }


            //用户信息
            var us = uCore.LoadEntity(o => o.U_Id == uid);
            string flag = HttpContext.Current.Request.Form["flag"];
            if ("1".Equals(flag))    //注册认证，不添加认证信息
            {
                us.U_AuditingState = (int)AuditingEnum.未认证;
            }
            else
            {
                core.AddEntity(new Core_AuditingVillage()
                {
                    A_BuildingId = buildingId,
                    A_BuildingName = buildingName,
                    A_CheckBack = "",
                    A_CheckTime = null,
                    A_DoorNo = doorNo,
                    A_Id = Guid.NewGuid(),
                    A_ImgBuilding = string.IsNullOrEmpty(bPath) ? "" : AttachmentFolderEnum.identification + "/" + bPath,
                    A_ImgFee = string.IsNullOrEmpty(fPath) ? "" : AttachmentFolderEnum.identification + "/" + fPath,
                    A_Remark = "",
                    A_Status = (int)AuditingEnum.未认证,
                    A_Time = DateTime.Now,
                    A_Title = "",
                    A_TrueName = trueName,
                    A_UId = uid,
                    A_UPhone = phone
                });   //1.添加认证记录
                us.U_AuditingState = (int)AuditingEnum.认证中;
            }
            Guid oldBuildingId = us.U_BuildingId;
            //us.U_AuditingState = (int)AuditingEnum.认证中;
            us.U_City = cityName;
            us.U_AreaCode = areaCode;
            us.U_BuildingId = buildingId;
            us.U_BuildingName = buildingName;
            us.U_AuditingManager = (int)AuditingEnum.未认证;  //每次提交用户社区认证（切换小区），都要重置他的管理员状态
            try
            {
                if (Math.Abs(coordX) > 0 && Math.Abs(coordY) > 0) //定位成功
                {
                    us.U_CoordX = coordY;
                    us.U_CoordY = coordX;
                    //todo:附近的人，暂时屏蔽
                    us.U_Location = System.Data.Entity.Spatial.DbGeography.FromText(string.Format("POINT({0} {1})", coordX, coordY), 4326);
                }
            }
            catch (Exception)
            {
            }
            us.U_TrueName = trueName;
            if (!uCore.UpdateEntity(us))//2.更新用户状态为 用户认证中
            {
                rs.State = 1;
                rs.Msg = "用户社区认证失败,请稍后再试";
            }
            else
            {
                //群聊天ID
                string villChatId = "";
                string villLogo = "";//小区LOGO
                if (buildingId != Guid.Empty)
                {
                    var vill = vCore.LoadEntity(o => o.V_Id == buildingId);
                    if (vill != null)
                    {
                        if (string.IsNullOrEmpty(vill.V_ChatID))  //小区还没有环信群
                        {
                            //创建群
                            string chatid = HuanXin.CreateQun("group_" + vill.V_Id);
                            vill.V_ChatID = chatid;
                            if (vCore.UpdateEntity(vill))
                            {
                                villChatId = chatid;
                            }
                        }
                        else
                        {
                            villChatId = vill.V_ChatID;
                        }
                        villLogo = vill.V_Img;
                    }

                    //villChatId = vCore.LoadEntity(p => p.V_Id == buildingId).V_ChatID;
                }


                //如果用户的小区ID不为空，标示之前是入住过小区的，需要从之前的小区退出
                if (oldBuildingId != Guid.Empty)
                {

                    var vill = vCore.LoadEntity(o => o.V_Id == oldBuildingId);  //退出老社区
                    if (!string.IsNullOrEmpty(vill.V_ChatID))
                    {
                        HuanXin.ExitQun(vill.V_ChatID, "lx_" + us.U_Id);
                    }
                }

                // 需求变更，没有实名认证的用户也可以加入到环信群
                HuanXin.AccountQunJoin(villChatId, us.U_ChatID);


                rs.Data =
                new
                {
                    logo = ConfigurationManager.AppSettings["ImgSiteUrl"] + us.U_Logo,
                    huanxin = "lx_" + us.U_Id,
                    nickName = us.U_NickName,
                    trueName = us.U_TrueName,
                    sex = us.U_Sex,
                    signatures = us.U_Signatures,
                    buildingId = us.U_BuildingId,
                    buildingName = us.U_BuildingName,
                    state = us.U_Status,
                    auditingState = us.U_AuditingState,
                    auditingManager = us.U_AuditingManager,
                    areaCode = us.U_AreaCode,
                    areaName = us.U_City,
                    regTime = us.U_RegisterDate,
                    phone = us.U_LoginPhone,
                    buildingChatId = villChatId,
                    buildingLogo = villLogo,
                    uid = us.U_Id
                };


                #region 消息推送,将认证信息推送给当前小区的管理员


                ////查找当前社区的管理员，推送给管理员审核
                ////StringBuilder sb = new StringBuilder();

                ////uCore.LoadEntities(
                ////    o => o.U_BuildingId == us.U_BuildingId &&o.U_AuditingManager == (int) AuditingEnum.认证成功 && o.U_Status!=(int)UserStatusEnum.冻结)
                ////    .Select(o => new
                ////    {
                ////        mid=o.U_Id.ToString("N").ToLower()
                ////    }).ForEach(o=>sb.Append(o.mid+","));
                //var mgrList = uCore.LoadEntities(
                //    o =>
                //        o.U_BuildingId == us.U_BuildingId && o.U_AuditingManager == (int)AuditingEnum.认证成功 &&
                //        o.U_Status != (int)UserStatusEnum.冻结).ToList()
                //    .Select(o => new
                //    {
                //        mid = o.U_Id.ToString("N").ToLower()
                //    });
                //if (mgrList.Any())  //有管理员
                //{
                //    JPushMsgModel jm = new JPushMsgModel()
                //    {
                //        code = (int)MessageCenterModuleEnum.用户认证,
                //        logo = ConfigurationManager.AppSettings["ImgSiteUrl"] + us.U_Logo,
                //        nickname = us.U_NickName,
                //        proFlag = (int)PushMessageEnum.默认,
                //        proId = "",
                //        proName = "",
                //        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //        tags = "邻友入驻",
                //        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //        title = "新用户社区认证",
                //        tips = string.Format("您的邻居 {0} 入驻{1}邻信社区", us.U_NickName, us.U_BuildingName),
                //        uid = us.U_Id.ToString()
                //    };

                //    string mgrStr = string.Join(",", mgrList.Select(o => o.mid));

                //    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), mgrStr.Split(','));
                //}



                ////短息通知平台管理员有新用户来咯
                //var mgrPhone = ConfigurationManager.AppSettings["CheckAuditingMgrPhone"];
                //if (!string.IsNullOrEmpty(mgrPhone))
                //{
                //    var mgrPhoneList = mgrPhone.Split(',');
                //    var logCore = new OperateLogCore();
                //    foreach (var s in mgrPhoneList)
                //    {
                //        if (!string.IsNullOrWhiteSpace(s) && s.IsMobilPhone())
                //        {

                //            sms sms = new sms();
                //            var msgContent = string.Format(ConfigurationManager.AppSettings["CheckAuditingSms"],
                //                string.Format("姓名-{0}，手机号-{1}，小区-{2}，城市-{3}", trueName, phone, buildingName, cityName));
                //            var srs = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                //                s, msgContent); //发送短消息

                //            int pla = 0;
                //            int.TryParse(Request.Headers.GetValues("platform").FirstOrDefault(), out pla);
                //            logCore.AddEntityNoSave(new Sys_OperateLog() { L_Desc = string.Format("用户社区认证申请，通知管理员审核,短信内容：{0}", msgContent), L_DriverType = pla, L_Flag = (int)ModuleEnum.发短信, L_Phone = s, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/Auditing/BuildingAuditing", L_Time = DateTime.Now });  //记录操作日志

                //        }
                //    }
                //    logCore.SaveChanges();
                //}

                #endregion
            }

            #region 九宫格活动

            //var pCore = new PrizeDetailCore();

            //pCore.AddOne(uid, phone, 3); //添加中奖记录

            //var topicCore = new TopicCore();
            //if (topicCore.LoadEntity(o => o.T_UserId == uid && o.T_Status == 0) != null)  //发表个邻里圈，获得第4次
            //{
            //    pCore.AddOne(uid, phone, 4); //添加中奖记录

            //    if (new BillMemberCore().LoadEntity(o => o.B_UId == uid && o.B_Module == (int)BillEnum.充值) != null)//冲过值
            //    {
            //        pCore.AddOne(uid, phone, 5); //添加中奖记录
            //    }
            //}
            #endregion

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 群管理员申请认证
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage GroupManangerAuditing([FromBody] JObject value)
        {
            AuditingGroupManangerCore core = new AuditingGroupManangerCore();
            ResultViewModel rs = new ResultViewModel(0, "ok", null);

            dynamic obj = value;

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            Guid buildingId = Guid.Parse(obj.buildingId);

            var auList =
                core.LoadEntities(
                    o => o.M_BuildingId == buildingId && o.M_UId == uid && o.M_Status == (int)AuditingEnum.未认证);
            if (auList.Any())
            {
                rs.State = 1;
                rs.Msg = "我们已经收到您的小区管理员申请，请勿重复提交！";
            }
            else
            {
                //查看当前小区是否已经存在管理员了
                var mgrGroup = uCore.LoadEntities(o => o.U_BuildingId == buildingId && o.U_AuditingManager == (int)AuditingEnum.认证成功);
                string isExistMgr = mgrGroup.Any() ? "当前小区已存在管理员" : "";
                //if (mgrGroup.Any())
                //{
                //    rs.State = 1;
                //    rs.Msg = "申请失败！当前小区已有管理员";
                //    return WebApiJsonResult.ToJson(rs);
                //}


                string linkPhone = obj.linkPhone;
                string trueName = obj.trueName;
                string remark = obj.remark;

                core.AddEntity(new Core_AuditingGroupMananger()
                {
                    M_BuildingId = buildingId,
                    M_CheckBack = "",
                    M_CheckTime = null,
                    M_Id = Guid.NewGuid(),
                    M_Phone = linkPhone,
                    M_QQ = isExistMgr,
                    M_Remark = remark,
                    M_Status = (int)AuditingEnum.未认证,
                    M_Time = DateTime.Now,
                    M_TrueName = trueName,
                    M_UId = uid,
                    M_UPhone = phone

                });   //1.添加社区管理员认证记录

                var us = uCore.LoadEntity(o => o.U_Id == uid);
                us.U_AuditingManager = (int)AuditingEnum.认证中;
                if (!uCore.UpdateEntity(us))//2.更新用户状态为 群管理员认证中
                {
                    rs.State = 1;
                    rs.Msg = "小区管理员申请失败,请稍后再试";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 通过小区认证（小区管理员审核）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BuildingAuditingCheck([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            dynamic obj = value;
            string ckName = obj.ckName;//审核人
            Guid ckId = obj.ckId;//审核人的ID
            Guid ckBuildingId = obj.ckBuildingId; //审核人的城市ID

            int state = obj.state; //拒绝2 或者 通过1 
            string remark = obj.remark;//审核人 反馈
            Guid aId = obj.aId;  //记录ID
            int role = (int)ManagerRoleEnum.小区管理员;// obj.role;  //审核人的角色 0系统管理员 1小区群主


            var mgrGroup = uCore.LoadEntity(o => o.U_Id == uid && o.U_AuditingManager == (int)AuditingEnum.认证成功);
            if (mgrGroup != null)
            {
                //是否是自家的社区认证
                var info =
                    core.LoadEntity(
                        o => o.A_Id == aId && o.A_Status == (int)AuditingEnum.未认证 && o.A_BuildingId == ckBuildingId);
                if (info != null)
                {

                    var flag = core.BuildingAuditingCheck(aId, ckName, uid, info.A_BuildingId, state, remark, role);

                    if (flag)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";


                        #region 消息推送
                        //JPushMsgModel jm = new JPushMsgModel()
                        //{
                        //    code = (int)MessageCenterModuleEnum.邻妹妹,
                        //    proFlag = state == 1 ? (int)PushMessageEnum.审核通过 : (int)PushMessageEnum.审核失败,
                        //    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tags = "社区认证",
                        //    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        //    tips = state == (int)AuditingEnum.认证成功 ? "您的社区认证已经通过审核" : "您的社区认证未通过审核，原因：" + remark,
                        //    title = " 您的社区认证已被处理"
                        //};
                        //Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), info.A_UId.ToString("N").ToLower());

                        #endregion
                    }
                    else
                    {
                        rs.Msg = "记录不存在";
                    }
                }

            }
            else
            {
                rs.Msg = "暂无权限";
            }


            return WebApiJsonResult.ToJson(rs);
        }
    }
}
