using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 移动端接口
    /// </summary>
    public class TopicViewModel
    {
        //0 话题 1商家推广 2社区互动
        public int Flag { get; set; }
        /// <summary>
        /// 社区互动
        /// </summary>
        public ActiveIndexViewModel Actives { get; set; }
        /// <summary>
        /// 话题
        /// </summary>
        public TopicIndexViewModel Topics { get; set; }
        /// <summary>
        /// 推广活动（商家付费）
        /// </summary>
        public dynamic Events { get; set; }
    }
    /// <summary>
    /// 邻里圈儿
    /// </summary>
    public class TopicController : BaseController
    {
        private LikesCore lCore = new LikesCore();
        private TopicCore tCore = new TopicCore();
        private ShowcaseCore eCore = new ShowcaseCore();
        private ShowcaseCore scCore = new ShowcaseCore();
        private InteractiveCore iCore = new InteractiveCore();
        private AttachmentCore attCore = new AttachmentCore();

        /// <summary>
        /// 邻友圈 列表（话题、邻友的差别就 是，邻友圈列表必须传递一个buildingId；话题默认是全国性质的）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Index([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid buildingId;
            string bid = obj.buildingId;
            Guid.TryParse(bid, out buildingId);  //（如果是话题、好友列表，可为空）
            int pn = obj.pn;
            int rows = obj.rows;

            string imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];  //领红包有个图像，路径一并发回在集合中


            var tList = tCore.GetLinyouTopics(buildingId, pn, rows);

            if (tList != null && tList.Any())  //有话题内容
            {
                var topicList = tList.Select(o => new TopicViewModel()
                {
                    Flag = 0,
                    Topics = o,
                    Actives = null,
                    Events = null

                }).ToList();  //话题的列表
                //todo:此处已做变更，不显示商家活动，改为显示商家发布到邻友圈的广告位：链接、视频、楼盘等
                #region 作废
                //  var eventList = eCore.GetLinyouEvents(buildingId, pn, rows);  //活动列表
                //var innteractiveList =
                //    iCore.LoadEntities(o => o.T_Status == 0 && o.T_VillageId == buildingId)
                //        .OrderByDescending(o => o.T_Recom)
                //        .ThenByDescending(o => o.T_Date)
                //        .Skip((pn - 1)*rows)
                //        .Take(rows)
                //        .Select(o => new
                //        {
                //            title = o.I_Title,
                //        });

                //List<TopicViewModel> vmList = new List<TopicViewModel>();
                //for (int i = 0; i < topicList.Count(); i++)  //默认每页总共有10条
                //{
                //    TopicViewModel vm = new TopicViewModel();
                //    //if (i == 1)  //第2位放商家推广（目前只放一个）
                //    //{
                //    //    vm.Flag = 1;
                //    //}
                //    //else if (i == 3 && activeList.Any())//第4位放社区互动（目前只放一个）
                //    //{
                //    //    vm.Flag = 2;
                //    //    vm.Topics = null;
                //    //    vm.Actives = activeList.ElementAtOrDefault(0);
                //    //    vm.Events = null;
                //    //}
                //    //else
                //    //{
                //    //    vm.Flag = 0;
                //    //    vm.Topics = topicList[i];
                //    //    vm.Actives = null;
                //    //    vm.Events = null;
                //    //}
                //    vm.Flag = 0;
                //    vm.Topics = topicList[i];
                //    vm.Actives = null;
                //    vm.Events = null;

                //    vmList.Add(vm);
                //}
                #endregion

                if (topicList.Count() > 3)  //第4位显示“社区互动”
                {

                    //插入社区互动
                    var activeList = iCore.GetLinyouActives(buildingId, pn, 1);
                    if (activeList.Any())
                    {
                        TopicViewModel vmActive = new TopicViewModel() { Flag = 2, Topics = null, Actives = activeList.FirstOrDefault(), Events = null };
                        topicList.Insert(3, vmActive);
                    }

                }
                //插入商家广告
                if (topicList.Count() > 1)  //第二位显示“商家推广”
                {
                    var showcase = scCore.QueryShowcase(buildingId, pn);
                    if (showcase != null)
                    {
                        TopicViewModel vmAd = new TopicViewModel() { Flag = 1, Events = showcase, Topics = null, Actives = null };
                        topicList.Insert(1, vmAd);
                    }
                }

                //社区互动列表
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new { siteUrl = imgUrl, pn, rows, vmList = topicList };
            }
            else
            {
                rs.Msg = "没有更多的内容";
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 全国话题
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage All([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            int pn = obj.pn;
            int rows = obj.rows;

            string imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];  //领红包有个图像，路径一并发回在集合中

            var topicList = tCore.GetLinyouTopics(null, pn, rows);


            if (topicList != null && topicList.Any()) //有话题内容
            {
                var rsData = topicList.Select(o => new TopicViewModel()
                {
                    Flag = 0,
                    Topics = o,
                    Actives = null,
                    Events = null

                }).ToList();  //话题的列表

                //todo:此处已做变更，不显示商家活动，改为显示商家发布到邻友圈的广告位：链接、视频、楼盘等

                //社区互动列表
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new { siteUrl = imgUrl, pn, rows, vmList = rsData };
            }
            else
            {
                rs.Msg = "没有更多的内容";
            }


            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 好友 话题
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Friends([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            int pn = obj.pn;
            int rows = obj.rows;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());


            string imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];  //领红包有个图像，路径一并发回在集合中


            var topicList = tCore.GetFriendsTopics(uid, null, pn, rows);

            if (topicList != null && topicList.Any()) //有话题内容
            {

                //todo:此处已做变更，不显示商家活动，改为显示商家发布到邻友圈的广告位：链接、视频、楼盘等
                //社区互动列表
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    siteUrl = imgUrl,
                    pn,
                    rows,
                    vmList = topicList.Select(o => new TopicViewModel()
                        {
                            Flag = 0,
                            Topics = o,
                            Actives = null,
                            Events = null

                        }).ToList()  //好友的话题列表 
                };

            }
            else
            {
                rs.Msg = "没有更多的内容";
            }

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 用户发表 话题到邻友圈
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Publish()
        {

            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();

            ResultViewModel rs = new ResultViewModel();
            //dynamic obj = value;
            Core_Topic topic = new Core_Topic();  //话题

            Core_LuckyGift hb = null;   // 话题带红包
            Core_TopicMovieTicket ticket = null;  //话题带礼物 电影票票
            string paypwd = HttpContext.Current.Request.Form["payPwd"];  //如果带有红包，必须输入支付密码

            topic.T_Id = Guid.NewGuid();
            topic.T_UserId = uid;
            topic.T_Title = HttpContext.Current.Request.Form["title"];
            topic.T_Hongbao = 0;
            topic.T_Ticket = 0;
            topic.T_ImgAttaCount = Convert.ToInt32(HttpContext.Current.Request.Form["imgAttaCount"]);
            topic.T_Clicks = 0;
            topic.T_Comments = 0;
            topic.T_Likes = 0;
            topic.T_Recom = 0;
            topic.T_Top = 0;
            topic.T_Status = 0;
            topic.T_AreaCode = HttpContext.Current.Request.Form["areaCode"];
            topic.T_VillageId = Guid.Parse(HttpContext.Current.Request.Form["buildingId"]);
            topic.T_Date = DateTime.Now;

            #region 上传话题封面、图片
            //上传文件的基础路径
            string basePath = ConfigurationManager.AppSettings["StaticFilePath"] + AttachmentFolderEnum.topic + "\\";
            HttpPostedFile coverImg = HttpContext.Current.Request.Files["coverImg"];

            if (!Directory.Exists(basePath))  //不存在就创建目录
                Directory.CreateDirectory(basePath);

            if (coverImg != null)  //缴费图片存在
            {
                string bPath = Guid.NewGuid().ToString("N") + Path.GetExtension(coverImg.FileName);
                var upCover = UploadFileToServerPath(coverImg, AttachmentFolderEnum.topic.ToString(), bPath);
                if (upCover != FileUploadStateEnum.上传成功)
                {
                    rs.Msg = upCover.ToString();
                    return WebApiJsonResult.ToJson(rs);
                }
                topic.T_Img = AttachmentFolderEnum.topic + "/" + bPath; //修改 话题封面图片
            }

            #endregion

            string hbJson = HttpContext.Current.Request.Form["hongbao"].ToString();
            if (!string.IsNullOrEmpty(hbJson))  //包含有红包的话题
            {
                var hbObj = JsonSerialize.Instance.JsonToObject(hbJson);
                string moneyStr = hbObj.money;
                if (!string.IsNullOrEmpty(moneyStr))
                {


                    hb = new Core_LuckyGift();
                    topic.T_Hongbao = 1;
                    hb.LG_Id = Guid.NewGuid();
                    hb.LG_Title = "邻友圈用户红包";
                    hb.LG_Type = (int)LuckGiftTypeEnum.邻友圈用户红包;
                    hb.LG_UserId = uid;
                    hb.LG_UserNickname = HttpContext.Current.Request.Form["nickname"];
                    hb.LG_Money = Convert.ToDecimal(DESProvider.DecryptString(moneyStr));
                    hb.LG_RemainMoney = hb.LG_Money;
                    hb.LG_Count = Convert.ToInt32(hbObj.count);
                    hb.LG_RemainCount = hb.LG_Count;
                    hb.LG_CreateTime = DateTime.Now;
                    hb.LG_Flag = null;//topic.T_Id;  //红包与话题关联
                    hb.LG_Status = 0;
                    hb.LG_VillageId = topic.T_VillageId;
                    hb.LG_AreaCode = topic.T_AreaCode;
                    hb.LG_ProjectId = topic.T_Id;
                    hb.L_ProjectTitle = topic.T_Title;
                }
            }

            string ticketJson = HttpContext.Current.Request.Form["ticket"];
            if (!string.IsNullOrEmpty(ticketJson)) //包含有票的话题
            {
                var ticketObj = JsonSerialize.Instance.JsonToObject(ticketJson);
                string codeStr = ticketObj.movieCode;
                if (!string.IsNullOrEmpty(codeStr))
                {
                    ticket = new Core_TopicMovieTicket();
                    topic.T_Ticket = 1;

                    ticket.T_UserId = uid;
                    ticket.T_TopicId = topic.T_Id;
                    ticket.T_TopicName = topic.T_Title;
                    ticket.T_Cinema = ticketObj.cinema;
                    ticket.T_MovieName = ticketObj.movieName;
                    ticket.T_MovieCode = ticketObj.movieCode;
                    ticket.T_DatetTime = DateTime.Now;
                }
            }

            ResultMessageViewModel msg = tCore.PublishTopic(topic, hb, ticket, phone, paypwd);  //发布话题

            if (msg.State == 0) //话题发布成功了，添加话题的其他图片附件
            {
                var files = HttpContext.Current.Request.Files;

                List<Sys_Attachment> attList = new List<Sys_Attachment>();  //所有的附件图片
                if (files.Count > 1)  //除了封面，还有其他图片附件
                {
                    var keys = files.AllKeys;
                    int temRank = 1;
                    foreach (var key in keys)
                    {
                        if (!key.Equals("coverImg"))    //排除封面，剩余的其他图片作为附件图片
                        {
                            Sys_Attachment att = new Sys_Attachment();
                            var f = HttpContext.Current.Request.Files[key];

                            // DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) + Path.GetExtension(coverImg.FileName);
                            att.A_FileName = Guid.NewGuid().ToString("N") + Path.GetExtension(coverImg.FileName);
                            att.A_FileNameOld = f.FileName;
                            att.A_Folder = AttachmentFolderEnum.topic.ToString();
                            att.A_Id = Guid.NewGuid();
                            att.A_PId = topic.T_Id;
                            att.A_Rank = temRank;
                            att.A_Size = f.ContentLength;
                            att.A_Time = topic.T_Date;
                            att.A_Type = (int)AttachmentTypeEnum.图片;
                            // f.SaveAs(basePath + att.A_FileName);//上传附件到服务器
                            UploadFileToServerPath(f, AttachmentFolderEnum.topic.ToString(), att.A_FileName);


                            attList.Add(att);
                            temRank++;
                        }

                    }

                    attCore.AddEntities(attList);  //批量入库

                }

                rs.Data = new
                {
                    id = topic.T_Id,
                    title = topic.T_Title,
                    imgUrl = string.IsNullOrEmpty(topic.T_Img) ? "" : ConfigurationManager.AppSettings["ImgSiteUrl"] + topic.T_Img,
                    hasHongbao = topic.T_Hongbao,
                    hasTicket = topic.T_Ticket,
                    time = topic.T_Date,
                    attImgUrl = attList.Select(o => new
                    {
                        imgUrl = ConfigurationManager.AppSettings["ImgSiteUrl"] + o.A_Folder + "/" + o.A_FileName
                    })
                };
            }
            rs.Msg = msg.Msg;
            rs.State = msg.State;


            #region 九宫格活动

            //var pCore = new PrizeDetailCore();

            //pCore.AddOne(uid, phone, 4); //添加中奖记录 

            //if (new BillMemberCore().LoadEntity(o => o.B_UId == uid && o.B_Module == (int)BillEnum.充值) != null)//冲过值
            //{
            //    pCore.AddOne(uid, phone, 5); //添加中奖记录 
            //}
            #endregion
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 邻友圈话题 点赞
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage LikeTopic([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            Guid proId = obj.proId;  //点赞项目的ID
            string proName = obj.proName; //点赞项目标题
            //string topicTag = obj.topicTag;  //点赞项目的标签 ，区别是话题、互动、活动等‘
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string uName = obj.nikeName;

            string logo = obj.logo;
            Guid proUid = obj.proUid;
            //是否已经点赞过了
            if (lCore.IsExist(proId, uid))
            {
                rs.Msg = "请勿重复点赞";
            }
            else
            {
                if (tCore.Like(proId, uid, proName, uName))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new LikeIndexViewModel
                    {
                        L_Id = Guid.Empty,
                        L_Time = DateTime.Now,
                        L_UserId = uid,
                        U_Logo = "",
                        U_NickName = uName
                    };


                    #region 消息推送
                    JPushMsgModel jm = new JPushMsgModel()
                    {
                        code = (int)MessageCenterModuleEnum.邻里圈,
                        logo = logo,
                        nickname = uName,
                        proFlag = (int)PushMessageEnum.用户跳转,
                        proId = proId.ToString(),
                        proName = proName,
                        proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        tags = "动态点赞",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        title = "您有动态被点赞",
                        tips = " 赞了您的动态",
                        uid = uid.ToString()
                    };

                    Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), proUid.ToString("N").ToLower());

                    #endregion
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 点赞 商家活动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage LikeEvent([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            Guid proId = obj.proId;  //点赞项目的ID
            string proName = obj.proName; //点赞项目标题
            //string topicTag = obj.topicTag;  //点赞项目的标签 ，区别是话题、互动、活动等‘
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string uName = obj.nikeName;
            //是否已经点赞过了
            if (lCore.IsExist(proId, uid))
            {
                rs.Msg = "请勿重复点赞";
            }
            else
            {
                if (eCore.Like(proId, uid, proName, uName))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new LikeIndexViewModel
                    {
                        L_Id = Guid.Empty,
                        L_Time = DateTime.Now,
                        L_UserId = uid,
                        U_Logo = "",
                        U_NickName = uName
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 评论 用户话题
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage CommentTopic([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid proId = obj.proId;
            string refUid = obj.refUid;
            string refUname = obj.refUname;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string uname = obj.nikeName;
            string content = obj.content;

            string logo = obj.logo;
            Guid proUid = obj.proUid;

            //添加评论
            decimal hbMoney = 0;
            rs = tCore.Comment(proId, uid, uname, refUid, refUname, content, ref hbMoney);
            if (rs.State > 0)  //留言成功
            {
                //todo:推送短消息到前端 提醒您有新的评论 
                #region 消息推送

                string hbstr = rs.State == 2 ? "，并获得红包:" + hbMoney + "元" : "";
                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻里圈,
                    logo = logo,
                    nickname = uname,
                    proFlag = (int)PushMessageEnum.用户跳转,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "动态评论",
                    title = "您有新的评论",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 评论了您的动态" + hbstr,
                    uid = uid.ToString(),
                    proId = proId.ToString()
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), proUid.ToString("N").ToLower());

                #endregion

            }
            else
            {
                rs.State = 9999;
                rs.Msg = "评论失败";
            }

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 评论 商家活动
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage CommentEvent([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;


            Guid proId = obj.proId;
            string refUid = obj.refUid;
            string refUname = obj.refUname;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string uname = obj.nikeName;

            string content = obj.content;
            Guid buildingId = obj.buildingId;


            //添加评论
            //bool addRs = false;// 

            decimal hbMoney = 0;
            rs = eCore.Comment(proId, uid, uname, refUid, refUname, content, buildingId, ref hbMoney);

            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 获取单个 商家活动的详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage EventOne([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid proId = obj.proId;
            rs.Data = new
            {
                imgUrl = StaticHttpUrl,
                topicInfo = eCore.GetOneShowCase(proId)
            };
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 获取单个 用户话题的详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage TopicOne([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid proId = obj.proId;
            rs.Data = new
            {
                imgUrl = StaticHttpUrl,
                topicInfo = tCore.GetOneTopic(proId)
            };
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 删除话题（目前只能自己删除自己发布的话题）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Remove([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid proId = obj.proId;

            var info = tCore.LoadEntity(o => o.T_Id == proId);
            if (info.T_Status == 0 && info.T_UserId == uid)
            {
                info.T_Status = 1;

                if (tCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }

    }
}
