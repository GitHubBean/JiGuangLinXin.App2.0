using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers.Prize
{
    /// <summary>
    /// 星际大冲关
    /// </summary>
    public class LotteryController : ApiController
    {
        private PrizeDetailCore pCore = new PrizeDetailCore();
        private UserCore ucore = new UserCore();


        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// 业主抽奖机会
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ChanceList([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "活动已过期！", null);
            //ResultMessageViewModel rs = new ResultMessageViewModel(1, "当前获得第一关资格！", null);
            //dynamic obj = value;

            //string uid = obj.uid;

            //Guid gid;

            //if (Guid.TryParse(uid, out gid)) //APP内部抽奖
            //{
            //    var list = pCore.LoadEntities(o => o.PD_UId == gid).OrderBy(o => o.PD_Round).ToList();

            //    if (list.All(o => o.PD_Round != 1))  //没有第一关,默认添加第一关【出现场景：业主并没有抽第一关奖，直接点击分享，解锁第二关，此时第一关数据为null,，所以要默认添加第一关数据】
            //    {
            //        list.Add(new Core_PrizeDetail()
            //        {
            //            PD_Flag = 0,
            //            PD_Award = "",
            //            PD_Round = 1,
            //            PD_Time = DateTime.Now,
            //            PD_TimeAward = null,
            //            PD_TimeUseful = DateTime.Now.AddDays(60)
            //        });
            //    }
            //    if (list.Any())
            //    {
            //        rs.Data = list.OrderBy(o => o.PD_Round).Select(o => new
            //        {
            //            flag = o.PD_Flag,
            //            tips = o.PD_Award,
            //            round = o.PD_Round,
            //            acTime = o.PD_Time,
            //            awardTime = o.PD_TimeAward,
            //            userfulTime = o.PD_TimeUseful
            //        });
            //        rs.State = 0;
            //        rs.Msg = "ok";
            //    }
            //}
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Turn([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "活动已过期！", null);
            //ResultMessageViewModel rs = new ResultMessageViewModel(1, "您暂未取得本关抽奖资格！", null);
            //dynamic obj = value;
            //string uid = obj.uid;
            //int round = obj.round;//第几关

            //if (round < 1 || round > 5)
            //{
            //    rs.Msg = "参数错误";
            //    return WebApiJsonResult.ToJson(rs);
            //}

            //if (DateTime.Now > Convert.ToDateTime("2016-08-01"))
            //{
            //    rs.Msg = "活动已经结束";
            //    return WebApiJsonResult.ToJson(rs);
            //}
            //Guid gid;

            //if (Guid.TryParse(uid, out gid))//APP内部抽奖
            //{
            //    var user = ucore.LoadEntity(o => o.U_Id == gid);
            //    if (user == null)
            //    {
            //        rs.Msg = "用户不存在！";
            //        return WebApiJsonResult.ToJson(rs);
            //    }

            //    var pinfo = pCore.LoadEntity(o => o.PD_UId == gid && o.PD_Round == round);

            //    if (pinfo != null)  //之前中过奖
            //    {
            //        if (pinfo.PD_Flag == 1 && pinfo.PD_TimeUseful > DateTime.Now)  //还没有兑奖
            //        {
            //            rs.Msg = "您已参加过本关抽奖活动！";
            //            return WebApiJsonResult.ToJson(rs);
            //        }
            //        else  //可以抽奖
            //        {
            //            var pd = CalcRate(round);
            //            pinfo.PD_Award = pd.PD_Award;
            //            pinfo.PD_LuckGift = pd.PD_LuckGift;
            //            pinfo.PD_OwnerCard = pd.PD_OwnerCard;
            //            pinfo.PD_TimeAward = DateTime.Now;

            //            pCore.UpdateEntity(pinfo); //更新中奖项目

            //            rs = pCore.TurnInside(gid, pinfo);
            //        }
            //    }
            //    else  //如果没有抽过奖的，只有第一关，所有人有资格
            //    {
            //        if (round == 1) //第一关
            //        {
            //            var pd = CalcRate(round);

            //            pd.PD_UId = user.U_Id;
            //            pd.PD_UPhone = user.U_LoginPhone;

            //            pCore.AddEntity(pd); //添加中奖记录

            //            rs = pCore.TurnInside(gid, pd);
            //        }
            //    }
            //}
            //else  //分享的抽奖,不入库
            //{
            //    var pd = CalcRate(round);
            //    string jsonStr = Md5Extensions.MD5Encrypt(JsonSerialize.Instance.ObjectToJson(pd));

            //    rs.State = 0;
            //    rs.Msg = "ok";
            //    rs.Data = new
            //    {
            //        awards = pd.PD_Award,
            //        token = jsonStr
            //    };
            //}

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 中奖了，提交电话号码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage SubmitPhone([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "活动已过期！", null);

            //ResultMessageViewModel rs = new ResultMessageViewModel();
            //dynamic obj = value;
            //string phone = obj.phone;
            //string token = obj.token;
            //if (phone.IsMobilPhone())  //手机号码合法
            //{
            //    var info = JsonSerialize.Instance.JsonToObject<Core_PrizeDetail>(Md5Extensions.MD5Decrypt(token));
            //    rs = pCore.TurnOutside(phone, info);
            //}
            //else
            //{
            //    rs.Msg = "手机号码不合法";
            //}
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 获得第二关的抽奖资格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Round2Chance([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(1, "活动已过期！", null);

            //ResultMessageViewModel rs = new ResultMessageViewModel(1, "用户不存在！", null);
            //dynamic obj = value;
            //string uid = obj.uid;
            //Guid gid;
            //if (Guid.TryParse(uid, out gid)) //APP内部抽奖
            //{
            //    var user = ucore.LoadEntity(o => o.U_Id == gid);
            //    if (user == null)
            //    {
            //        rs.Msg = "用户不存在！";
            //        return WebApiJsonResult.ToJson(rs);
            //    }

            //    //当前用户是否取得第二关资格
            //    var pd = pCore.LoadEntity(o => o.PD_UId == gid && o.PD_Round == 2);

            //    if (pd == null)  //如果之前没有第二关资格
            //    {
            //        pd = new Core_PrizeDetail();
            //        pd.PD_Id = Guid.NewGuid();
            //        pd.PD_Flag = 0;
            //        pd.PD_Round = 2;
            //        pd.PD_Time = DateTime.Now;
            //        pd.PD_TimeUseful = DateTime.Now.AddDays(60);
            //        pd.PD_UId = user.U_Id;
            //        pd.PD_UPhone = user.U_LoginPhone;

            //        pCore.AddEntity(pd); //添加中奖记录

            //        if (user.U_AuditingState == (int)AuditingEnum.认证成功)  //如果实名认证了，获得第3次
            //        {

            //            pCore.AddOne(user.U_Id, user.U_LoginPhone, 3); //添加中奖记录 

            //            var topicCore = new TopicCore();
            //            if (topicCore.LoadEntity(o => o.T_UserId == user.U_Id && o.T_Status == 0) != null)  //发表个邻里圈，获得第4次
            //            {
            //                pCore.AddOne(user.U_Id, user.U_LoginPhone, 4); //添加中奖记录 

            //                if (new BillMemberCore().LoadEntity(o => o.B_UId == user.U_Id && o.B_Module == (int)BillEnum.充值) != null)//冲过值
            //                {
            //                    pCore.AddOne(user.U_Id, user.U_LoginPhone, 5); //添加中奖记录 
            //                }
            //            }
            //        }


            //        rs.Msg = "ok";
            //        rs.State = 0;
            //    }
            //    else
            //    {
            //        rs.Msg = "已取得第二关抽奖资格";
            //    }
            //}
            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 计算抽奖的各种概率
        /// </summary>
        /// <param name="count">第几次抽奖（每次抽奖的概率不同）</param>
        /// <returns></returns>
        private Core_PrizeDetail CalcRate(int count)
        {
            List<Core_PrizeDetail> list = new List<Core_PrizeDetail>();

            list.Add(new Core_PrizeDetail() { PD_Award = "0#3业主卡", PD_LuckGift = 0, PD_OwnerCard = 10, PD_Rate = 0.2 * count * 0.1 });
            list.Add(new Core_PrizeDetail() { PD_Award = "1#10业主卡", PD_LuckGift = 0, PD_OwnerCard = 5, PD_Rate = 0.2 * count * 0.2 });
            list.Add(new Core_PrizeDetail() { PD_Award = "2#iPhone6", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0 });
            list.Add(new Core_PrizeDetail() { PD_Award = "3#谢谢参与", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 1 - 0.2 * count });
            list.Add(new Core_PrizeDetail() { PD_Award = "4#业主红包", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.3 });
            list.Add(new Core_PrizeDetail() { PD_Award = "5#VR眼镜", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.1 });
            list.Add(new Core_PrizeDetail() { PD_Award = "6#100元业主卡", PD_LuckGift = 0, PD_OwnerCard = 3, PD_Rate = 0.2 * count * 0.2 });
            list.Add(new Core_PrizeDetail() { PD_Award = "7#业主红包", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.1 });
            //if (count == 1)  //20% 几率中奖
            //{
            //    list[1].PD_Rate = 0.2 * 0.1;
            //    list[2].PD_Rate = 0.2 * 0.2;
            //    list[3].PD_Rate = 0.2 * 0.3;
            //    list[4].PD_Rate = 0.2 * 0.1;
            //    list[5].PD_Rate = 0.2 * 0.2;
            //    list[6].PD_Rate = 0.2 * 0.1;
            //    list[7].PD_Rate = 0.8;
            //} 
            #region 随机抽奖
            /*
            Random rdm = new Random(Guid.NewGuid().GetHashCode());
            int temp = 0;
            int seek = rdm.Next(1, 101);  //取一个随机数
            for (int i = 0; i < list.Count; i++)
            {
                temp += Convert.ToInt32(list[i].PD_Rate * 100);
                if (temp >= seek)  //有可能=100
                {
                    return list[i];
                }
            }
             */
            #endregion

            #region 默认中奖

            switch (count)
            {
                case 1:
                    var pd = list[4];
                    
                        Random rd = new Random(Guid.NewGuid().GetHashCode());
                        pd.PD_Id = Guid.NewGuid();
                        pd.PD_Flag = 0;
                        pd.PD_Round = 1;
                        pd.PD_Time = DateTime.Now;
                        pd.PD_TimeUseful = DateTime.Now.AddDays(60);
                        pd.PD_TimeAward = DateTime.Now;
                        pd.PD_LuckGift = Convert.ToDecimal((rd.Next(100, 300) * 0.01).ToString("F2"));
                        pd.PD_Award += ":" + pd.PD_LuckGift + "元";
                        return pd;
                    break;
                case 2:

                    return list[1];
                    break;
                case 3:
                    return list[0];
                    break;
                case 4:

                    return list[3];
                    break;
                case 5:

                    return list[0];
                    break;
            }
            #endregion

            return null;
        }
    }
}
