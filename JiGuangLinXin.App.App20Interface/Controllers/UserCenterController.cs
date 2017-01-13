using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using Com.Alipay.Mobile;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.Rpg;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 用户的个人中心
    /// </summary>
    public class UserCenterController : BaseController
    {

        private TopicCore tCore = new TopicCore();
        private LifestyleServicesLikeCore sCore = new LifestyleServicesLikeCore();
        private MallOrderDetailCore oCore = new MallOrderDetailCore();
        private UserCore uCore = new UserCore();
        private BalanceCore bCore = new BalanceCore();
        private BillMemberCore billCore = new BillMemberCore();
        //private EventScopeCore eventScopeCore = new EventScopeCore();
        private GroupbuyCore eventScopeCore = new GroupbuyCore();
        private BusinessServiceCore busVillCore = new BusinessServiceCore();
        private MallGoodsCore mallCore = new MallGoodsCore();
        private GroupAlbumCore albumCore = new GroupAlbumCore();
        private GroupAlbumPicCore picCore = new GroupAlbumPicCore();
        private OwnerCardHistoryCore ohCore = new OwnerCardHistoryCore();
        private VoucherCardHistoryCore vhCore = new VoucherCardHistoryCore();
        private AuditingCashCore auditingCashCore = new AuditingCashCore();
        private OperateLogCore logCore = new OperateLogCore();
        /// <summary>
        /// 我的邻友圈
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage MyTopic([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;

            rs.Data =
                tCore.LoadEntities(o => o.T_UserId == uid && o.T_Status == 0)
                    .OrderByDescending(o => o.T_Date)
                    .Skip((pn - 1) * rows)
                    .Take(rows)
                    .Select(o => new
                    {
                        topicId = o.T_Id,
                        title = o.T_Title,
                        coverImg = StaticHttpUrl + o.T_Img,
                        imgCount = o.T_ImgAttaCount,
                        time = o.T_Date,
                        ticket = o.T_Ticket
                    });

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 点击邻友头像，查看邻友的个人中心
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage LinyouCenter([FromBody] JObject value)
        {

            EventCore eCore = new EventCore();
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            Guid linyouId = obj.linyouId;
            //查询邻友的个人信息
            var linyou = uCore.LoadEntity(o => o.U_Id == linyouId);
            var isFriend =
                new UserFriendCore().LoadEntities(
                    o => (o.F_FriendId == uid && o.F_UId == linyouId) || (o.F_UId == uid && o.F_FriendId == linyouId)).Any();
            var info = new
            {
                logo = StaticHttpUrl + linyou.U_Logo,
                nickname = linyou.U_NickName,
                phone = linyou.U_LoginPhone,
                buildingName = linyou.U_BuildingName,
                buildingId = linyou.U_BuildingId,
                huanxinId = linyou.U_ChatID,
                sex = linyou.U_Sex,
                age = linyou.U_Age,
                birthday = linyou.U_Birthday,
                audtingState = linyou.U_AuditingState,
                isFriend = isFriend ? "1" : "0"
            };

            //社区服务总数
            //int lifeCount = lifeCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == buildingId).Count();
            //int lifeCount = busVillCore.LoadEntities(p => p.BV_VillageId == linyou.U_BuildingId).Count();
            int lifeCount = busVillCore.LoadEntities(o => o.buildingId == linyou.U_BuildingId).Count();

            //社区活动总数
            //int eventCount = eventScopeCore.LoadEntities(p => p.S_BuildingId == linyou.U_BuildingId).Count();
            //int eventCount = eventScopeCore.GetGroupBuyCountByBuildingId(linyou.U_BuildingId);
            int eventCount = eCore.GetBuildingEvents(linyou.U_BuildingId).Count() + eCore.LoadEntities(o => o.E_Status == 0 && o.E_Target == 0).Count();


            //精品汇总数[特意改成了邻里团]
            int mallCount = eventScopeCore.GetGroupBuyCountByBuildingId(linyou.U_BuildingId);  //new MallGoodsCore().CountGoods(linyou.U_BuildingId, Guid.Empty);


            //mallCore.LoadEntities(p => p.G_Status == (int)OperateStatusEnum.Default && p.G_VillageId == linyou.U_BuildingId).Count();

            //社区相册
            var albumlist =
                albumCore.LoadEntities(o => o.A_BuildingId == linyou.U_BuildingId && o.A_State == (int)OperateStatusEnum.Default).OrderBy(o => o.A_Flag).ThenBy(o => o.A_Time).ToList()
                    .Select(o => new
                    {
                        albumId = o.A_Id,
                        coverImg = StaticHttpUrl + o.A_CoverImg
                    });
            int albumCount = picCore.LoadEntities(o => o.P_BuildingId == linyou.U_BuildingId && o.P_State == (int)OperateStatusEnum.Default).Count();
            //邻友圈
            var topic =
                tCore.LoadEntities(o => o.T_UserId == linyouId && o.T_Status == 0)
                    .OrderByDescending(o => o.T_Date).Take(10)
                    .Select(o => new
                    {
                        topicId = o.T_Id,
                        title = o.T_Title,
                        coverImg = StaticHttpUrl + o.T_Img,
                        imgCount = o.T_ImgAttaCount,
                        time = o.T_Date
                    });


            rs.Data = new
            {
                lifeCount,
                eventCount,
                mallCount,
                userInfo = info,
                album = new
                {
                    list = albumlist,
                    count = albumCount
                },
                topicList = topic
            };
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 用户下单记录（我的精品汇）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage MyOrders([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;

            rs = oCore.GetUserOrderDetailList(uid, pn - 1, rows, StaticHttpUrl);

            //rs.Data =
            //    oCore.LoadEntities(o => o.OD_UId == uid)
            //        .OrderByDescending(o => o.OD_Time)
            //        .Skip((pn - 1) * rows)
            //        .Take(rows)
            //        .ToList()
            //        .Select(o => new
            //        {
            //            id = o.OD_Id,
            //            uid = o.OD_UId,
            //            imgUrl = StaticHttpUrl + o.OD_Img,
            //            goodsName = o.OD_Name,
            //            goodsCount = o.OD_Count,
            //            goodsId = o.OD_GoodsId,
            //            busName = o.OD_BusName,
            //            busId = o.OD_BusId,
            //            linkPhone = o.OD_BusPhone,
            //            price = o.OD_Price,
            //            time = o.OD_Time,
            //            //  orderNo = o.OD_OrderId
            //            orderNo = o.OD_Time.ToString("yyyyMMddHHmmssfff")
            //            //payState = o.GO_PayState,
            //            //orderState = o.GO_OrderState

            //        });
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 用户收藏的商家、社区服务
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage MyCollect([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;


            rs.Data =
                sCore.LoadEntities(o => o.L_UserId == uid && o.L_State == 0)
                    .OrderByDescending(o => o.L_Time)
                    .Skip((pn - 1) * rows)
                    .Take(rows)
                    .Select(o => new
                    {
                        id = o.L_Id,
                        uid = o.L_UserId,
                        serviceId = o.L_ServiceId,
                        serviceName = o.L_ServiceName,
                        imgUrl = StaticHttpUrl + o.L_ServiceImg
                    });
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 用户的余额查询
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage MyMoney()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            rs.Data = new
            {
                balance = bCore.LoadEntity(o => o.B_AccountId == uid).B_Balance.ToString("F2")
            };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 用户账单（所有）
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage MyBill([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            int pn = obj.pn;
            int rows = obj.rows;

            var billList = billCore.LoadEntities(p => p.B_UId == uid).OrderByDescending(o => o.B_Time)
                .Skip((pn - 1) * rows)
                .Take(rows).Select(o => new
                {
                    id = o.B_Id,
                    title = o.B_Title,
                    money = o.B_Money,
                    time = o.B_Time
                });


            rs.Data = billList;
            return WebApiJsonResult.ToJson(rs);
        }





        /// <summary>
        /// 编辑用户个人资料
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage EditUserInfo()
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            HttpPostedFile logo = HttpContext.Current.Request.Files["logo"];

            string nickname = HttpContext.Current.Request.Form["nickname"];

            string birthday = HttpContext.Current.Request.Form["birthday"];

            string sex = HttpContext.Current.Request.Form["sex"];


            var user = uCore.LoadEntity(o => o.U_Id == uid);
            if (user != null)
            {
                if (user.U_NickName != nickname.Trim()) //昵称已变更、需要同时更改环信的昵称
                {
                    HuanXin.AccountResetNickname(user.U_ChatID, nickname);  //更改环信的昵称
                }

                user.U_NickName = nickname;
                user.U_Birthday = birthday;

                DateTime temp;
                if (!string.IsNullOrEmpty(birthday) && DateTime.TryParse(birthday, out temp))
                {
                    user.U_Age = DateTime.Now.Year - Convert.ToDateTime(birthday).Year;
                }

                user.U_Sex = (sex == "男" || sex == "1") ? 1 : 0;

                if (logo != null)  //更换头像
                {
                    string bPath = Guid.NewGuid().ToString("N") + Path.GetExtension(logo.FileName);

                    var r = UploadFileToServerPath(logo, AttachmentFolderEnum.avatar.ToString(), bPath);
                    if (r == FileUploadStateEnum.上传成功)
                    {
                        user.U_Logo = AttachmentFolderEnum.avatar.ToString() + "/" + bPath;
                    }
                }
                if (uCore.UpdateEntity(user))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        avatar = StaticHttpUrl + user.U_Logo
                    };
                }
            }


            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 修改个人密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [OperateLog(Desc = "修改密码", Flag = (int)ModuleEnum.个人信息)]
        public HttpResponseMessage ChangePwd([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            string oldPwd = obj.oldPwd;
            string newPwd = obj.newPwd;



            var user = uCore.LoadEntity(o => o.U_Id == uid);
            if (user != null)
            {
                string code = user.U_PwdCode;

                oldPwd = DESProvider.DecryptString(oldPwd);
                if (Md5Extensions.MD5Encrypt(oldPwd + code) != user.U_LoginPwd)
                {
                    rs.Msg = "原密码不正确";
                }
                else
                {
                    newPwd = Md5Extensions.MD5Encrypt(DESProvider.DecryptString(newPwd) + code);

                    user.U_LoginPwd = newPwd;

                    if (uCore.UpdateEntity(user))
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new
                        {
                            avatar = StaticHttpUrl + user.U_Logo
                        };
                    }
                }

            }

            return WebApiJsonResult.ToJson(rs);
        }




        /// <summary>
        /// 余额充值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [OperateLog(Desc = "账户余额充值", Flag = (int)ModuleEnum.充值)]
        public HttpResponseMessage Recharge([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            string orderNo = obj.order;  //客户端加密
            //string enMoney = obj.money;
            if (!string.IsNullOrEmpty(orderNo)) //查询支付宝订单是否存在
            {
                orderNo = DESProvider.DecryptString(orderNo);

                //查看此订单在数据库是否已经被销帐
                AlipayOrderCore aliCore = new AlipayOrderCore();
                var info = aliCore.LoadEntity(o => o.A_OrderNo == orderNo);
                if (info == null)
                {
                    rs.Msg = "订单不存在";
                }
                else if (info.A_Status != (int)PayOffEnum.未销帐)
                {
                    #region 九宫格活动抽奖

                    //var pCore = new PrizeDetailCore();

                    //pCore.AddOne(uid, phone, 5); //添加中奖记录 

                    #endregion
                    #region 消息推送

                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻妹妹,
                        proFlag = (int)PushMessageEnum.默认,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "充值",
                        title = "余额充值成功",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tips = " 余额充值成功，充值的金额为" + info.A_Money.ToString("N") + " 元",
                    };

                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                    #endregion

                    rs.State = 1;
                    rs.Msg = "充值成功";//"此订单已销帐";
                }
                else  //如果没有销帐，就远程查询支付宝的账单
                {
                    SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                    sParaTemp.Add("partner", Com.Alipay.Config.Partner);
                    sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
                    sParaTemp.Add("service", "single_trade_query");
                    //sParaTemp.Add("trade_no", order);//使用支付宝交易号查询
                    sParaTemp.Add("out_trade_no", orderNo); //使用商户订单号查询

                    string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sHtmlText);
                    if ("TRADE_SUCCESS".Equals(xmlDoc.DocumentElement["response"]["trade"]["trade_status"].InnerText)) //支付宝订单正常
                    {
                        decimal money = 0;  //阿里订单的money
                        string moneys = xmlDoc.DocumentElement["response"]["trade"]["total_fee"].InnerText;
                        string aliOrderId = xmlDoc.DocumentElement["response"]["trade"]["trade_no"].InnerText;  //阿里生成的订单号
                        string partner = xmlDoc.DocumentElement["response"]["trade"]["seller_id"].InnerText; //xmlDoc.DocumentElement["request"]["partner"].InnerText;  //商户号
                        string sellerEmail = xmlDoc.DocumentElement["response"]["trade"]["seller_email"].InnerText;  //卖家（平台）的帐号
                        //支付宝账单是否合法
                        if (decimal.TryParse(moneys, out money) && !string.IsNullOrEmpty(aliOrderId) && Config.Partner.Equals(partner) && Config.Seller_email.Equals(sellerEmail))
                        {
                            BalanceCore aCore = new BalanceCore();

                            rs = aCore.RechargeAlipay(orderNo, money, aliOrderId);  //销帐 0成功 1已经销帐 2销帐失败

                            if (rs.State == 0 || rs.State == 1)
                            {

                                #region 九宫格活动抽奖

                                //var pCore = new PrizeDetailCore();

                                //pCore.AddOne(uid, phone, 5); //添加中奖记录 

                                #endregion
                                #region 消息推送

                                JPushMsgModel jm = new JPushMsgModel()
                                {
                                    code = (int)MessageCenterModuleEnum.邻妹妹,
                                    proFlag = (int)PushMessageEnum.默认,
                                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    tags = "充值",
                                    title = "余额充值成功",
                                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    tips = " 余额充值成功，充值的金额为" + money + " 元",
                                };

                                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                                #endregion
                            }
                        }
                    }
                }

            }

            return WebApiJsonResult.ToJson(rs);
        }




        /// <summary>
        /// 便民账单
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ConvenienceBill([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            rs.Data =
                billCore.LoadEntities(o => o.B_UId == uid && (o.B_Module == (int)BillEnum.便民缴费 || o.B_Module == (int)BillEnum.话费充值85折))
                    .OrderByDescending(o => o.B_Time)
                    .Skip(pn * rows)
                    .Take(rows).ToList()
                    .Select(n => new
                    {
                        id = n.B_Id,
                        title = n.B_Title.Split(':')[0],
                        orderNo = n.B_Title.Split(':').Count() > 1 ? n.B_Title.Split(':')[1] : "暂无单号",
                        remark = n.B_Remark,
                        date = n.B_Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        money = -n.B_Money,//客户端要求，金额为正
                        flag = n.B_Flag


                    });

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 领取的商家抵用券
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage MyVoucherCard([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            rs.Data = vhCore.LoadEntities(o => o.H_UserId == uid).OrderByDescending(o => o.H_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                historyId = o.H_Id,
                title = o.H_Title,
                money = o.H_Money,
                sTime = o.H_STime,
                eTime = o.H_ETime,
                busName = o.H_Nickname,
                busId = o.H_BusId,
                cardId = o.H_CardId
            });

            return WebApiJsonResult.ToJson(rs);
        }

        #region 业主卡
        /// <summary>
        /// 用户购买的业主卡记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OwnerCardHistory([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;

            rs.Data = ohCore.LoadEntities(o => o.O_UserId == uid).OrderByDescending(o => o.O_Time).Skip(pn * rows).Take(rows).ToList().Select(o => new
            {
                ways = Enum.GetName(typeof(OwnerCardWays), o.O_Way),
                money = o.O_Money,
                time = o.O_Time,
                bId = o.O_Id
            });

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 业主卡流水账单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OwnerCardBill([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;

            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;
            rs.Data =
             billCore.LoadEntities(o => o.B_UId == uid && o.B_Module == (int)BillEnum.平台业主卡)
                 .OrderByDescending(o => o.B_Time)
                 .Skip(pn * rows)
                 .Take(rows).ToList()
                 .Select(n => new
                 {
                     bId = n.B_Id,
                     title = n.B_Title,
                     remark = n.B_Remark,
                     time = n.B_Time,
                     money = n.B_Money.ToString("N")
                 });

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 购买业主卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage BuyOwnerCard([FromBody] JObject value)
        {

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            dynamic obj = value;

            string nickName = obj.nickName;
            string enmoney = obj.money;
            int ways = obj.way;
            int flag = obj.flag;
            decimal money = Convert.ToDecimal(DESProvider.DecryptString(enmoney));

            string enPayPwd = obj.payPwd; //支付密码

            //OwnerCardWays way = (OwnerCardWays)Enum.Parse(typeof(OwnerCardWays), ways);

            ResultMessageViewModel rs = ohCore.BuyOwnerCard(uid, nickName, phone, money, ways, flag, enPayPwd);

            if (rs.State == 0)  //购卡成功
            {
                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "业主卡",
                    title = "您购买了一张业主卡",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您购买了一张业主卡，面额" + money + " 元",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                #endregion


            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 卡密兑换业主卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage KeyOwnerCard([FromBody] JObject value)
        {

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            dynamic obj = value;
            string nickName = obj.nickName;
            string key = obj.key;
            ResultMessageViewModel rs = ohCore.KeyOwnerCard(uid, nickName, phone, key);
            //VoucherCardPostCore

            Core_OwnerCardHistory his = (Core_OwnerCardHistory)rs.Data;

            if (rs.State == 0)
            {
                rs.Data = new
                {
                    postFee = 10, //邮费
                    historyId = his.O_Id,
                    money = his.O_Money
                };


                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "业主卡",
                    title = "您卡密兑换了一张业主卡",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您卡密兑换了一张业主卡，面额" + his.O_Money + " 元",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                #endregion

            }
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 卡密支付完成，申请邮寄实体卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage PostOwnerCard([FromBody] JObject value)
        {

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            string nickName = obj.nickName;
            string linkName = obj.linkName;
            string linkPhone = obj.linkPhone;
            string linkAddress = obj.linkAddress;
            Guid historyId = obj.historyId;
            string enmoney = obj.money;

            string paypwd = obj.payPwd;  // 支付密码

            Core_VoucherCardPost post = new Core_VoucherCardPost()
            {
                P_Address = linkAddress,
                P_CardHistoryId = historyId,
                P_CardMoney = Convert.ToDecimal(DESProvider.DecryptString(enmoney)),
                P_Id = Guid.NewGuid(),
                P_LinkName = linkName,
                P_LinkPhone = linkPhone,
                P_NickName = nickName,
                P_PostMoney = 10,
                P_Remark = "",
                P_State = 0,
                P_Time = DateTime.Now,
                P_UId = uid

            };

            ResultMessageViewModel rs = ohCore.PostOwnerCard(post, paypwd);
            if (rs.State == 0)
            {

                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "邮寄",
                    title = "您申请了邮寄业主卡实体卡",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您申请了邮寄业主卡实体卡，邮费10元",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                #endregion
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 业主卡余额
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OwnerCardBalance([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            var balance = bCore.LoadEntity(o => o.B_AccountId == uid);
            rs.Data = new
            {
                cardBalance = balance.B_CouponMoney
            };
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [OperateLog(Desc = "申请提现", Flag = (int)ModuleEnum.提现)]
        public HttpResponseMessage ApplyCash([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            dynamic obj = value;

            string accout = obj.account;
            string trueName = obj.trueName;
            string enMoney = obj.money;
            decimal money = Convert.ToDecimal(DESProvider.DecryptString(enMoney));
            string enPaypwd = obj.payPwd;  //必须输入支付密码

            int flag = obj.flag;
            var balance = bCore.LoadEntity(o => o.B_AccountId == uid);
            if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
            {
                rs.State = 2;
                rs.Msg = "您还未设置支付密码,请立即前去设置！";

                return WebApiJsonResult.ToJson(rs);
            }
            else
            {
                string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// 加密支付密码
                if (!balance.B_PayPwd.Equals(payPwd))
                {
                    rs.Msg = "支付密码错误！";

                    return WebApiJsonResult.ToJson(rs);
                }
            }




            if (balance.B_Balance >= money)  //余额足够，可以提现
            {
                Core_AuditingCash cash = new Core_AuditingCash()
                {
                    M_BankAccount = accout,
                    M_BankName = trueName,
                    M_Id = Guid.NewGuid(),
                    M_Money = money,
                    M_Status = (int)ApplyCashStateEnum.等待审核,
                    M_Time = DateTime.Now,
                    M_UId = uid,
                    M_Phone = phone,
                    M_Flag = flag,
                    M_Role = (int)MemberRoleEnum.会员
                };

                //确认提现
                if (auditingCashCore.ApplyCash(cash))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        cId = cash.M_Id.ToString("N"),
                        cMoney = cash.M_Money,
                        cDate = cash.M_Time.ToShortDateString(),
                        cAccount = cash.M_BankAccount,
                        cTrueName = cash.M_BankName,
                        cPhone = cash.M_Phone
                    };
                }
            }
            else
            {
                rs.Msg = "余额不足";
            }


            return WebApiJsonResult.ToJson(rs);
        }


        #endregion

        #region 支付密码
        /// <summary>
        /// 设置支付密码发送短信
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage PayPwdSendSMS([FromBody]JObject value)
        {
            dynamic oc = value;
            ResultViewModel rs = new ResultViewModel();
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            int pla = 0;
            //号码合法，并且号码存在
            if (!phone.IsMobilPhone())
            {
                rs.Msg = "手机号不合法";
            }
            else if (!int.TryParse(dr, out pla))
            {
                rs.Msg = "拒绝访问!!";
            }
            else
            {
                var resetUser = uCore.LoadEntity(o => o.U_LoginPhone == phone && o.U_Status != (int)UserStatusEnum.冻结);
                if (resetUser != null)
                {
                    int sendCount = 0;//发送次数
                    int.TryParse(CacheHelper.GetCacheString("payPwdCode" + phone), out sendCount);
                    if (sendCount > 5) //超过发送次数，防止机器人频繁触发
                    {
                        rs.Msg = "验证码发送已超过5次，明日再试";
                    }
                    else
                    {
                        ////todo:发验证码测试
                        //rs.State = 0;
                        //rs.Msg = "ok,忘记密码验证码测试：123456";

                        //rs.Data = new { uid = resetUser.U_Id, phone = phone, code = DESProvider.EncryptString("123456"), time = DateTime.Now.ToString(), sendCount = sendCount };

                        //return WebApiJsonResult.ToJson(rs);



                        string code = new CreateRandomStr().GetRandomString(6);
                        sms sms = new sms();
                        var rr = sms.Submit(ConfigurationManager.AppSettings["SmsName"], ConfigurationManager.AppSettings["SmsPwd"],
                            phone, string.Format(ConfigurationManager.AppSettings["SmsRegTmp"], code)); //发送短消息


                        sendCount = sendCount + 1;
                        CacheHelper.SetCache("payPwdCode" + phone, sendCount, DateTime.Now.AddDays(1));  //发送记录
                        CacheHelper.SetCache("payCode" + phone, DESProvider.EncryptString(code));  
                        logCore.AddEntity(new Sys_OperateLog() { L_Desc = string.Format("设置支付密码，发送短信验证码。验证码：{0},当天发送次数{1}", code, sendCount), L_DriverType = pla, L_Flag = (int)ModuleEnum.发短信, L_Phone = phone, L_UId = Guid.NewGuid(), L_Status = 0, L_Url = "/User/ResetPayPwd", L_Time = DateTime.Now });  //记录操作日志

                        rs.State = 0;
                        rs.Msg = "ok";
                        if (pla == (int)DriversEnum.Android)
                        {
                            rs.Data = new { phone = phone, code = "", time = DateTime.Now.ToString(), sendCount = sendCount };//code= DESProvider.EncryptString(code)
                        }
                        else
                        {
                            rs.Data = new { phone = phone, code = DESProvider.EncryptString(code), time = DateTime.Now.ToString(), sendCount = sendCount };//code= DESProvider.EncryptString(code)
                        }
                    }
                }
                else
                {
                    rs.Msg = "手机帐号已被冻结或不存在！";
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>

        public HttpResponseMessage SetPayPwd([FromBody] JObject ob)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = ob;
            string dr = Request.Headers.GetValues("platform").FirstOrDefault();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault()); //重置会员的ID
            int pla = 0;
            if (!int.TryParse(dr, out pla))  //header 简单验证
            {
                rs.Msg = "拒绝访问";
            }
            else
            {
                string enPwd = obj.newPassword;
                string enCode = obj.code;
                string token = obj.callback;

                if (!string.IsNullOrEmpty(token))  //token不对
                {
                    dynamic tk = JsonSerialize.Instance.JsonToObject(token);

                    string codeTime = tk.time;
                    if (Convert.ToDateTime(codeTime).AddMinutes(30) < DateTime.Now)  //验证码已经超时了
                    {
                        rs.State = 2;
                        rs.Msg = "验证码超时";
                    }
                    else if (enCode.Equals(CacheHelper.GetCacheString("payCode" + tk.phone)))  //合法
                    {
                        //Guid uid = tk.uid;//重置会员的ID
                        string newPwd = DESProvider.DecryptString(enPwd);
                        if (newPwd.Length != 6)
                        {
                            rs.Msg = "密码长度必须6位";

                            return WebApiJsonResult.ToJson(rs);
                        }

                        if (uCore.SetPayPwd(uid, newPwd, pla))  //用户重置密码
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                        }
                        else
                        {
                            rs.Msg = "设置支付密码成功";
                        }
                    }
                    else
                    {
                        rs.Msg = "验证码错误";
                    }
                }
                else
                {
                    rs.Msg = string.Format("token验证的口令失败,token={0} ", token);
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 查询用户是否设置过支付密码
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public HttpResponseMessage ExistPayPwd([FromBody] JObject ob)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault()); //重置会员的ID

            var ba = bCore.LoadEntity(o => o.B_AccountId == uid);
            if (string.IsNullOrEmpty(ba.B_PayPwd) || string.IsNullOrEmpty(ba.B_EncryptCode))
            {
                rs.State = 1;
                rs.Msg = "您还未设置支付密码";
            }
            else
            {
                rs.State = 0;
                rs.Msg = "ok";
            }

            return WebApiJsonResult.ToJson(rs);
        }


        #endregion

        #region  App 检查版本更新(接口已迁移)

        ///// <summary>
        ///// 检测是否有新版本
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public HttpResponseMessage CheckVersion([FromBody] JObject value)
        //{
        //    ResultViewModel rs = new ResultViewModel(1, "当前已是最新版本",null);
        //    dynamic obj = value;
        //    string version = obj.version;
        //    int flag = obj.flag;


        //    AppVersionCore avCore = new AppVersionCore();
        //    var last = avCore.LoadEntities(o => o.V_Flag == flag).OrderBy(o => o.V_Id).FirstOrDefault(); //最新版本号

        //    if (!string.IsNullOrEmpty(version) && last != null)
        //    {
        //        //if (version.Equals(last.V_Code))
        //        //{
        //        //    rs.State = 1;
        //        //    rs.Msg = "当前已是最新版本";
        //        //}
        //        if (!version.Equals(last.V_Code))
        //        {
        //            rs.State = 0;
        //            rs.Msg = "ok";
        //            rs.Data = new
        //            {
        //                download = string.Format("{0}{1}/{2}", StaticHttpUrl, AttachmentFolderEnum.appdownload, last.V_FileName)// + 
        //            };
        //        }
        //    }
        //    //version = version.Replace(".", "");
        //    //int ver = 0;
        //    //if (int.TryParse(version,out ver))
        //    //{
        //    //}

        //    return WebApiJsonResult.ToJson(rs);
        //}

        #endregion

    }
}
