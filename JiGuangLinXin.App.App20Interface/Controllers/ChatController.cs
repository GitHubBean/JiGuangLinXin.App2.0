using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 聊天大模块（社区中心、群相册等）
    /// </summary>
    public class ChatController : BaseController
    {
        private UserCore uCore = new UserCore();
        private UserFriendsApplyCore uaCore = new UserFriendsApplyCore();
        private AuditingGroupManangerCore umCore = new AuditingGroupManangerCore();
        private UserFriendCore ufCore = new UserFriendCore();
        //private EventScopeCore eventScopeCore = new EventScopeCore();
        private GroupbuyCore eventScopeCore = new GroupbuyCore();
        private BusinessServiceCore busVillCore = new BusinessServiceCore();
        private GroupAlbumPicCore picCore = new GroupAlbumPicCore();
        private InteractiveCore iCore = new InteractiveCore();
        private FeedbackCore fbCore = new FeedbackCore();
        private NoticeCore nCore = new NoticeCore();

        /// <summary>
        /// 获取好(邻)友列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage FriendList([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(1, "暂无数据", null);
            dynamic obj = value;

            string buildingId = obj.buildingId;  //小区ID

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());  //用户的id


            var list = uCore.GetFriendList(buildingId, uid);  //获取列表
            if (list != null && list.Count > 0)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    imgUrl = StaticHttpUrl,
                    list = list
                };
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 查询用户（根据电话号码）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage FindFriend([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            string queryNo = obj.queryNo;

            if (queryNo.IsMobilPhone())//验证帐号格式
            {
                var user = uCore.LoadEntity(o => o.U_LoginPhone == queryNo && o.U_Status != (int)UserStatusEnum.冻结);
                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return WebApiJsonResult.ToJson(rs);
                }
                if (uid == user.U_Id)  //查询出的好友是自己
                {
                    rs.State = 1;
                    rs.Msg = "用户自己";
                }
                else
                {
                    rs.State = 0;
                    rs.Msg = "ok";

                }
                rs.Data = new
                {
                    uid = user.U_Id,
                    huanxinId = user.U_ChatID,
                    logo = ConfigurationManager.AppSettings["ImgSiteUrl"] + user.U_Logo,
                    nikeName = user.U_NickName,
                    cityName = user.U_City,
                    buidingName = user.U_BuildingName,
                    sex = user.U_Sex,
                    age = user.U_Age
                };
            }
            else
            {
                rs.Msg = "用户的手机号码格式有误";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddFriendApply([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            Guid friendId = obj.friendId;
            string tips = obj.tips;
            string uname = obj.userNickname;
            string uLogo = obj.userLogo;
            string fNickname = obj.friendNickname;
            string fLogo = obj.friendLogo;

            if (uid == friendId)
            {
                rs.State = 1;
                rs.Msg = "不能添加自己为好友";
                return WebApiJsonResult.ToJson(rs);
            }

            if (!string.IsNullOrWhiteSpace(uLogo) && uLogo.IndexOf("avatar") > -1)
            {
                //uLogo = uLogo.Replace(StaticHttpUrl, "");
                uLogo = uLogo.Substring(uLogo.IndexOf("avatar"));
            }
            else
            {
                uLogo = "";
            }

            if (!string.IsNullOrWhiteSpace(fLogo) && fLogo.IndexOf("avatar") > -1)
            {
                //   fLogo = fLogo.Replace(StaticHttpUrl, "");
                fLogo = fLogo.Substring(fLogo.IndexOf("avatar"));
            }
            else
            {
                fLogo = "";
            }


            var isExist = uaCore.LoadEntities(o => (o.A_FriendId == friendId && o.A_UserId == uid) || (o.A_UserId == friendId && o.A_FriendId == uid));
            if (isExist.Any(o => o.A_State == (int)AuditingEnum.未认证))
            {
                rs.State = 1;
                rs.Msg = "此好友申请已经存在，等待处理中";
            }
            else if (isExist.Any(o => o.A_State == (int)AuditingEnum.认证成功))
            {

                rs.State = 1;
                rs.Msg = "此用户已存在您的好友列表中，请勿重复添加";
            }
            else
            {


                var ap = uaCore.AddEntity(new Core_UserFriendsApply()
                {
                    A_Echo = "",
                    A_FriendId = friendId,
                    A_State = (int)AuditingEnum.未认证,
                    A_Time = DateTime.Now,
                    A_TimeCheck = null,
                    A_Tips = tips,
                    A_UserId = uid,
                    A_UserLogo = uLogo,
                    A_UserNickName = uname,
                    A_FriendLogo = fLogo,
                    A_FriendNickName = fNickname

                });
                if (ap.A_Id > 0)
                {
                    #region 消息推送

                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = (int)PushMessageEnum.好友申请,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "好友申请",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        title = "有用户请求加您为好友",
                        tips = " 请求加您为好友",
                        uid = uid.ToString(),
                        nickname = uname
                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), friendId.ToString("N").ToLower());

                    #endregion

                    rs.State = 0;
                    rs.Msg = "好友请求已发出，等待对方通过";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 申请加好友（手机通讯录）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddFriendApplyNotes([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string friendPhone = obj.friendPhone;
            string tips = obj.tips;
            string uname = obj.userNickname;
            string uLogo = obj.userLogo;

            var friend = uCore.LoadEntity(o => o.U_LoginPhone == friendPhone && o.U_Status != (int)UserStatusEnum.冻结);


            if (friend != null)
            {
                var isExist = uaCore.LoadEntities(o => ((o.A_FriendId == friend.U_Id && o.A_UserId == uid) || (o.A_FriendId == friend.U_Id && o.A_UserId == uid)) && o.A_State==(int)AuditingEnum.认证成功);
                if (isExist.Any())
                {
                    rs.State = 1;
                    rs.Msg = "您已申请过添加此用户为好友";
                }
                else
                {

                    string fLogo = friend.U_Logo;
                    if (uLogo.Contains(StaticHttpUrl))
                    {
                        uLogo = uLogo.Replace(StaticHttpUrl, "");
                    }

                    if (fLogo.Contains(StaticHttpUrl))
                    {
                        fLogo = uLogo.Replace(StaticHttpUrl, "");
                    }
                    var ap = uaCore.AddEntity(new Core_UserFriendsApply()
                    {
                        A_Echo = "",
                        A_FriendId = friend.U_Id,
                        A_State = (int)AuditingEnum.未认证,
                        A_Time = DateTime.Now,
                        A_TimeCheck = null,
                        A_Tips = tips,
                        A_UserId = uid,
                        A_UserLogo = uLogo,
                        A_UserNickName = uname,
                        A_FriendLogo = fLogo,
                        A_FriendNickName = friend.U_NickName

                    });
                    if (ap.A_Id > 0)
                    {
                        //todo:添加好友申请，需要发送推送消息，格式待定
                        rs.State = 0;
                        rs.Msg = "好友请求已发出，等待对方通过";



                        #region 消息推送

                        JPushMsgModel jm = new JPushMsgModel()
                        {
                            code = (int)MessageCenterModuleEnum.邻妹妹,
                            proFlag = (int)PushMessageEnum.好友申请,
                            proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            tags = "好友申请",
                            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            title = "有用户请求加您为好友",
                            tips = " 请求加您为好友",
                            uid = uid.ToString(),
                            nickname = uname
                        };
                        Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), friend.U_Id.ToString("N").ToLower());

                        #endregion

                    }
                }
            }
            else
            {
                rs.Msg = "此用户暂无入驻邻信平台";
                rs.Data = new
                {
                    notes = "邻信是一款邻里之间的即时通讯软件，旨在促进邻里关系，在这里你会接触到更多的邻居，快来和大家认识一下！链接地址" + ConfigurationManager.AppSettings["H5AppDownloadUrl"]
                };
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 获得好友申请列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage FriendApplyList([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;


            rs.Data =
                uaCore.LoadEntities(p => p.A_UserId == uid || p.A_FriendId == uid)
                    .OrderByDescending(o => o.A_Time)
                    .Skip((pn - 1) * rows)
                    .Take(rows)
                    .Select(o => new
                    {
                        applyId = o.A_Id,
                        echo = o.A_Echo,
                        fId = o.A_FriendId,
                        state = o.A_State,
                        time = o.A_Time,
                        checkTime = o.A_TimeCheck,
                        tips = o.A_Tips,
                        uid = o.A_UserId,
                        userLogo = StaticHttpUrl + o.A_UserLogo,
                        userNickname = o.A_UserNickName,
                        fLogo = StaticHttpUrl + o.A_FriendLogo,
                        fNickname = o.A_FriendNickName,
                        flag = uid == o.A_UserId ? "0" : "1"  //0 我发出的请求 1别人发给我的请求

                    });
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 处理好友申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage CheckFriendApply([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int applyId = obj.applyId;  //申请ID
            int state = obj.state;
            string tips = obj.tips;  //处理意见



            var apply = uaCore.LoadEntity(o => o.A_Id == applyId && o.A_State == (int)AuditingEnum.未认证);

            if (apply != null)  //
            {
                apply.A_Echo = tips;
                apply.A_State = state;
                apply.A_TimeCheck = DateTime.Now;

                if (uaCore.UpdateEntity(apply))  //处理申请
                {
                    if (state == (int)AuditingEnum.认证成功)  //如果认证成功，添加好友关系
                    {
                        Core_UserFriend uf = new Core_UserFriend();
                        uf.F_FriendId = apply.A_FriendId;
                        uf.F_Remark = "";
                        uf.F_Time = DateTime.Now;
                        uf.F_UId = apply.A_UserId;
                        ufCore.AddEntity(uf);
                    }
                    rs.State = 0;
                    rs.Msg = "ok";

                    #region 消息推送
                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = (int)PushMessageEnum.好友申请,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "添加好友",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        title = "您的好友请求，对方已处理",
                        tips = string.Format(" {0}了您的添加好友请求",  state == 1 ? "同意" : "拒绝"),
                        uid = uid.ToString(),
                        nickname = apply.A_FriendNickName
                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), apply.A_UserId.ToString("N").ToLower());

                    #endregion
                }

            }
            else
            {
                rs.Msg = "数据异常，请刷新后再试";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 用户申请成为群主
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage GroupManagerApply([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            string trueName = obj.trueName;
            string linkPhone = obj.linkPhone;
            Guid buildingId = obj.buildingId;
            string buildingName = obj.buildingName;
            string tips = obj.tips;

            var auList =
                umCore.LoadEntities(
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
                string isExistMgr = mgrGroup.Any() ? "已存在管理员" : "";
                var ma = umCore.AddEntity(new Core_AuditingGroupMananger()
                {
                    M_Id = Guid.NewGuid(),
                    M_BuildingId = buildingId,
                    M_CheckBack = "",
                    M_CheckTime = null,
                    M_Phone = linkPhone,
                    M_QQ = isExistMgr,
                    M_Remark = tips,
                    M_Status = (int)AuditingEnum.未认证,
                    M_Time = DateTime.Now,
                    M_TrueName = trueName,
                    M_UId = uid,
                    M_UPhone = phone,
                    M_BuildingName = buildingName

                });

                if (ma != null)  //发起申请成功
                {
                    //变更用户的管理员申请状态
                    var us = uCore.LoadEntity(o => o.U_Id == uid);
                    us.U_AuditingManager = (int)AuditingEnum.认证中;
                    if (uCore.UpdateEntity(us))
                    {

                        rs.State = 0;
                        rs.Msg = "您的申请已经收到，审核中请耐心等待！";
                        #region 消息推送
                        JPushMsgModel jm = new JPushMsgModel()
                        {
                            code = (int)MessageCenterModuleEnum.邻妹妹,
                            proFlag = (int)PushMessageEnum.默认,
                            proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            tags = "反馈",
                            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            title = "我们已经收到了您的反馈",
                            tips = "您的申请已经收到,我们将尽快处理,审核中请耐心等待,谢谢！",
                        };
                        Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                        #endregion
                    }

                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 社群中心首页
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage GroupCenter([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;

            EventCore eCore = new EventCore();
            Guid buildingId = obj.buildingId;  //社区ID

            //社区服务总数
            //int lifeCount = lifeCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == buildingId).Count();
            //int lifeCount = busVillCore.LoadEntities(p => p.BV_VillageId == buildingId).Count();
            int lifeCount = busVillCore.LoadEntities(o => o.buildingId == buildingId).Count();

            //社区活动总数
            //int eventCount = eventScopeCore.LoadEntities(p => p.S_BuildingId == buildingId).Count();
            //int eventCount = eventScopeCore.GetGroupBuyCountByBuildingId(buildingId);
            int eventCount = eCore.GetBuildingEvents(buildingId).Count() + eCore.LoadEntities(o => o.E_Status == 0 && o.E_Target == 0).Count();


            //精品汇总数
            //int mallCount = new MallGoodsCore().CountGoods(buildingId, Guid.Empty);

            //精品汇总数[特意改成了邻里团]
            int mallCount = eventScopeCore.GetGroupBuyCountByBuildingId(buildingId);


            //mallCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == buildingId).Count();
            //社区相册
            //var albumlist =
            //    albumCore.LoadEntities(o => o.A_BuildingId == buildingId && o.A_State == (int)OperateStatusEnum.Default).OrderBy(o => o.A_Flag).ThenBy(o => o.A_Time)
            //        .Select(o => new
            //        {
            //            albumId = o.A_Id,
            //            coverImg = StaticHttpUrl + o.A_CoverImg
            //        });
            var albumlist =
                picCore.LoadEntities(p => p.P_BuildingId == buildingId && p.P_State == (int)OperateStatusEnum.Default)
                    .OrderByDescending(p => p.P_Time)
                    .Take(10)
                    .ToList()
                    .Select(o => new
                    {
                        albumId = o.P_Id,
                        coverImg = StaticHttpUrl + o.P_Folder + "/" + o.P_FileName
                    });
            int albumCount = picCore.LoadEntities(o => o.P_BuildingId == buildingId && o.P_State == (int)OperateStatusEnum.Default).Count();

            //我的邻居
            var neighbor =
                uCore.LoadEntities(o => o.U_Status != (int)UserStatusEnum.冻结 && o.U_BuildingId == buildingId);

            int neighborCount = neighbor.Count();  //邻居总数
            //最新的 10 位用户邻居 ;   有头像的在前面
            var neighborList = neighbor.OrderByDescending(o => o.U_Logo).ThenByDescending(o=>o.U_RegisterDate).Take(10).Select(user => new
            {
                uid = user.U_Id,
                huanxinId = user.U_ChatID,
                logo = StaticHttpUrl + user.U_Logo,
                nikeName = user.U_NickName,
                cityName = user.U_City,
                buidingName = user.U_BuildingName,
                sex = user.U_Sex,
                age = user.U_Age
            });

            //社区互动
            var interactive =
                iCore.LoadEntities(o => o.I_VillageId == buildingId && o.I_Status == (int)OperateStatusEnum.Default);
            int interactiveCount = interactive.Count();
            var interactiveList = interactive.OrderByDescending(o => o.I_Date).Take(5).Select(o => new
            {
                aid = o.I_Id,
                uid = o.I_UserId,
                imgUrl = StaticHttpUrl + o.I_Img,
                title = o.I_Title
            });

            //社区资讯

            dynamic notice = null;
            var notList = nCore.LoadEntities(
                o =>
                    o.N_State == 0 && o.N_Flag == (int)PageEnum.社区中心首页)
                .OrderByDescending(o => o.N_Date)
                .Take(8)
                .Select(o => new
                {
                    title = o.N_SubTitle,
                    tags = o.N_Tags,
                    endTime = o.N_Remark,
                    id = o.N_Id
                });  //公告

            if (!notList.Any())
            {
                notice = new List<dynamic>()
                {
                    new
                    {
                        title = "欢迎使用邻信",
                        tags = "公告",
                        endTime = "",
                        id = ""
                    }
                };

            }
            else
            {
                notice = notList;
            }

            rs.Data = new
            {
                lifeCount,
                eventCount,
                mallCount,
                album = new
                {
                    list = albumlist,
                    count = albumCount
                },
                neighbor = new
                {
                    list = neighborList,
                    count = neighborCount
                },
                interactive = new
                {
                    list = interactiveList,
                    count = interactiveCount
                },
                notice
            };
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 社区反馈(举报)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Feedback([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            string content = obj.content;
            int state = obj.state;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();




            var fb = fbCore.AddEntity(new Core_Feedback()
            {
                F_Content = content,
                F_Flag = state,  //0反馈 1举报
                F_Phone = phone,
                F_Status = 0,
                F_Time = DateTime.Now,
                F_UId = uid
            });
            if (fb != null)
            {

                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "反馈",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    title = "我们已经收到了您的反馈",
                    tips = "我们已经收到了您的反馈，感谢您对我们的支持，我们将尽快处理，谢谢！",
                };
                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());

                #endregion

                rs.State = 0;
                rs.Msg = "ok";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 根据环信的ChatId查询用户
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage QueryUserByChatId([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();

            dynamic obj = value;

            string cid = obj.chatId;
            var user = uCore.LoadEntity(o => o.U_ChatID == cid);
            if (user != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    logo = StaticHttpUrl + user.U_Logo,
                    huanxin = "lx_" + user.U_Id,
                    nickName = user.U_NickName,
                    trueName = user.U_TrueName,
                    sex = user.U_Sex,
                    age = user.U_Age,
                    birthday = user.U_Birthday,
                    signatures = user.U_Signatures,
                    buildingId = user.U_BuildingId,
                    buildingName = user.U_BuildingName,
                    state = user.U_Status,
                    auditingState = user.U_AuditingState,
                    areaCode = user.U_AreaCode,
                    areaName = user.U_City,
                    regTime = user.U_RegisterDate,
                    phone = user.U_LoginPhone,
                    uid = user.U_Id
                };
            }
            else
            {
                rs.Msg = "用户帐号不存在";
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 管理员禁言
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Silence([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid userId = obj.userId;//被禁言的用户Id
            Guid buildingId = obj.buildingId;

            var user = uCore.LoadEntity(o => o.U_Id == uid && o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_BuildingId == buildingId); //如果当前用户是管理员

            if (user != null)
            {
                var opUser = uCore.LoadEntity(p => p.U_Id == userId && p.U_BuildingId == buildingId);
                opUser.U_Status = opUser.U_Status == (int)UserStatusEnum.禁言 ? (int)UserStatusEnum.正常 : (int)UserStatusEnum.禁言;
                if (uCore.UpdateEntity(opUser))
                {
                    //todo:禁言之后，推送消息
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        status = opUser.U_Status
                    };



                    #region 消息推送

                    string tip = opUser.U_Status == (int)UserStatusEnum.禁言 ? "您已被禁言" : "您已被解禁";
                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = opUser.U_Status == (int)UserStatusEnum.禁言 ? (int)PushMessageEnum.禁言 : (int)PushMessageEnum.解禁,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "聊天",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        title = tip,
                        tips = tip
                    };
                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), userId.ToString("N").ToLower());

                    #endregion

                }
                else
                {
                    rs.Msg = "禁言操作失败";
                }
            }
            else
            {
                rs.Msg = "您不是小区管理员，无权禁言";
            }

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 用户屏蔽用户群消息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BlockGroupMsg([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid buildingId = obj.buildingId;

            var user = uCore.LoadEntity(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 && o.U_BuildingId == buildingId);

            if (user != null)
            {
                user.U_BlockGroupMsg = user.U_BlockGroupMsg == 0 ? 1 : 0;  //0否 1

                if (uCore.UpdateEntity(user))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        blockGroupMsg = user.U_BlockGroupMsg
                    };
                }
                else
                {
                    rs.Msg = "操作失败";
                }
            }
            else
            {
                rs.Msg = "用户帐号不存在";
            }

            return WebApiJsonResult.ToJson(rs);
        }


    }
}
