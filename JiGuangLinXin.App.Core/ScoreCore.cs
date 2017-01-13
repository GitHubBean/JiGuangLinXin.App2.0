using System;
using System.Collections.Generic;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.Core
{
    public class ScoreCore : BaseRepository<Core_Score>
    {
        /// <summary>
        /// 积分流量充值
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="score">消耗积分</param>
        /// <param name="title">充值标题</param>
        /// <param name="times">兑奖时间段，格式：2016-07-28 11:00,2016-07-28 11:01|2016-07-28 09：00,2016-07-28 09：01</param>
        /// <param name="limitCount">每日兑换份数</param>
        /// <returns></returns>
        public ResultMessageViewModel CellphoneTraffic(Guid uid, int score, string title, string times, int limitCount)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;

            bool flag = false;//是否可以兑奖
            //先看看是否是在可允许的时间段内兑奖
            DateTime now = DateTime.Now;
            var timeArr = times.Split('|'); //多个时间段可以兑奖
            foreach (var ti in timeArr)
            {
                var timestra = ti.Split(',');
                if (now > Convert.ToDateTime(timestra[0]) && now < Convert.ToDateTime(timestra[1]))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                rs.Msg = "暂未到兑奖时间";
                return rs;
            }


            int orderId = -1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //判断用户是否存在，以及积分是否足够
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 && o.U_AuditingState == (int)AuditingEnum.认证成功);
                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return rs;
                }
                DateTime dt = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                if (db.Core_ScoreExchange.Count(o => o.E_Module == (int)EventH5ModuleEnum.签到送流量 && o.E_Time > dt) >= limitCount)
                {
                    rs.Msg = "流量已被抢光";
                    return rs;
                }


                var scoreInfo = db.Core_Score.FirstOrDefault(o => o.S_AccountId == uid);
                if (scoreInfo != null && scoreInfo.S_Score >= score)  //积分足够
                {
                    //1扣除积分
                    scoreInfo.S_Score -= score;

                    //2 添加兑奖记录
                    var ex = new Core_ScoreExchange()
                    {
                        E_BuildingId = user.U_BuildingId,
                        E_BuildingName = user.U_BuildingName,
                        E_Id = 0,
                        E_Module = (int)EventH5ModuleEnum.签到送流量, //默认，兑流量，如果后期扩展，写成枚举即可
                        E_OrderNo = "",//真正充值成功，需要修改此订单号数据,
                        E_Phone = user.U_LoginPhone,
                        E_Role = (int)MemberRoleEnum.会员,
                        E_Score = score,
                        E_Status = -1,  //-1标识未销帐，0成功 ，1失败
                        E_Time = DateTime.Now,
                        E_Flag = (int)FilmFlagEnum.流量,
                        E_Title = title,
                        E_UId = uid
                    };

                    db.Core_ScoreExchange.Add(ex);

                    //3 添加账单记录
                    //Core_BillMember bill = new Core_BillMember()
                    //{
                    //    B_Flag = (int)BillFlagModuleEnum.官方平台,
                    //    B_Money = -score,
                    //    B_OrderId = order.O_Id,
                    //    B_Phone = order.O_Phone,
                    //    B_Remark = order.O_Remark,
                    //    B_Status = 0,
                    //    B_Time = order.O_Time,
                    //    B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type) + ":" + order.O_OrderNo,
                    //    B_UId = order.O_UId,
                    //    B_Type = (int)MemberRoleEnum.会员
                    //};

                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = JsonSerialize.Instance.ObjectToJson(new
                        {
                            phone = user.U_LoginPhone, //用户电话
                            billId = ex.E_Id //兑奖记录的ID
                        });
                    }

                }
                else
                {
                    rs.Msg = "签到点不足";
                }

            }
            return rs;
        }
    }

    public class ScoreExchangeCore : BaseRepository<Core_ScoreExchange>
    {
        /// <summary>
        /// 老虎机活动
        /// </summary>
        /// <param name="uid">业主ID</param>
        /// <returns></returns>
        public ResultMessageViewModel PlaySlot(Guid uid)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结);
                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return rs;
                }
                else if (user.U_AuditingState != (int)AuditingEnum.认证成功)
                {
                    rs.Msg = "请先通过实名认证";
                    return rs;
                }
                if (db.Core_ScoreExchange.Any(o => o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票 && o.E_UId == uid))
                {
                    rs.Msg = "您已参加过本次活动";
                    return rs;
                }

                var info = new Core_ScoreExchange()
                {
                    E_BuildingId = user.U_BuildingId,
                    E_BuildingName = user.U_BuildingName,
                    E_Id = 0,
                    E_Module = (int)EventH5ModuleEnum.老虎机送电影票,
                    E_OrderNo = "", //真正充值成功，需要修改此订单号数据,
                    E_Phone = user.U_LoginPhone,
                    E_Role = (int)MemberRoleEnum.会员,
                    E_Score = 0,
                    E_Status = 0,
                    E_Time = DateTime.Now,
                    E_UId = uid
                };

                #region 计算中奖概率

                //先计算得出已中的各个奖项的和

                //电影票
                var events =
                    db.Core_ScoreExchange.Where(o => o.E_Module == (int)EventH5ModuleEnum.老虎机送电影票);
                //.Count(o => o.E_Module == (int) EventH5ModuleEnum.老虎机送电影票 && o.E_Flag == 1);
                //每个小区多少张
                var tickets = events.Count(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 1); //电影票张数
                var cards = events.Count(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 2); //业主卡数量
                var giftInfo = events.Where(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 3); //红包总数
                int gift = 0;
                if (giftInfo.Any())
                {
                    gift = giftInfo.Sum(o => o.E_Score);
                }


                var rdm = new Random(Guid.NewGuid().GetHashCode());
                List<int> nums = new List<int>();

                //开始随机抓奖
                while (true)
                {

                    if (tickets > 100 && cards > 100 && gift > 1500)
                    {
                        rs.Msg = "活动太火爆，奖品已经抽完咯！";
                        return rs;
                    }
                    //1电影票 2业主卡 3红包
                    int rnum = rdm.Next(0, 100); //定个随机数，控制中奖几率
                    if (tickets <= 100 && rnum < 30)
                    {
                        nums.Add(1);
                    }
                    if (cards <= 100 && rnum >= 30 && rnum < 60)
                    {
                        nums.Add(2);
                    }
                    if (gift <= 1500 && rnum >= 60)
                    {
                        nums.Add(3);
                    }
                    if (nums.Count > 0)
                    {
                        break;
                    }

                }

                int tem = rdm.Next(0, nums.Count);
                int flag = nums[tem]; //第几个奖项

                //if (tem < 40)
                //{
                //    flag = 1;
                //    info.E_Title = "获得电影票一张";
                //    info.E_Flag = flag;
                //}
                //else if (tem < 80)
                //{
                //    flag = 2;
                //}
                //else
                //{
                //    flag = 3;
                //}


                info.E_Flag = flag;
                if (flag == 1)
                {
                    info.E_Title = "获得电影票一张";
                    info.E_Score = 30; //30元/张 电影票 ，这个字段反正闲着也是闲着

                    rs.Msg =
                        "恭喜您抽中电影票1张，您所在社区已领取电影票" + tickets + "张，共100张。赶紧邀请邻居也入驻邻信，凡抽取电影票达到80张，即可申请观看免费包场电影！#" + user.U_LoginPhone;
                }
                else if (flag == 2)
                {
                    info.E_Title = "获得业主卡一张";
                    info.E_Score = 30;

                    Guid bid = Guid.Parse("309FFB15-3CAA-4C92-9F1E-96EDAF5A81FD"); //排除东原D7小区
                    if (user.U_BuildingId != bid)
                    {
                        var ban = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                        if (ban != null)
                        {
                            //累计业主卡余额
                            ban.B_CouponMoney += 30;


                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = info.E_Time,
                                B_Title = string.Format("活动玩老虎机，获得{0}元业主卡", 30),
                                B_Money = 30,
                                B_UId = user.U_Id,
                                B_Phone = user.U_LoginPhone,
                                B_Module = (int)BillEnum.平台业主卡,
                                B_OrderId = null,
                                B_Type = (int)MemberRoleEnum.会员,
                                B_Flag = (int)BillFlagEnum.普通流水,
                                B_Status = 0
                            };
                            //添加账单
                            db.Core_BillMember.Add(bill);
                        }
                    }

                    rs.Msg = "恭喜您抽中30元现金业主卡1张，您所在社区已领取业主卡" + cards + "张，共100张。赶紧邀请邻居也入驻邻信，还有机会观看免费包场电影，赶紧行动起来吧！#" + user.U_LoginPhone;
                }
                else if (flag == 3)
                {
                    //得红包又的来一次随机金额 (最少1，最多5)
                    info.E_Score = rdm.Next(1, 3);
                    info.E_Title = "获得" + info.E_Score + "元红包";

                    //添加会员账单表
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = info.E_Time,
                        B_Title = string.Format("活动玩老虎机，获得{0}元红包", info.E_Score),
                        B_Money = info.E_Score,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.红包,
                        B_OrderId = null,
                        B_Type = (int)MemberRoleEnum.会员,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0

                    };
                    db.Core_BillMember.Add(bill);

                    //累计余额
                    var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                    balance.B_Balance += info.E_Score; // Convert.ToDecimal(info.E_Score);
                }

                #endregion

                db.Core_ScoreExchange.Add(info);
                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Data = new
                    {
                        flag,
                        title = info.E_Title,
                        phone = user.U_LoginPhone,
                        num = 80 - tickets - 1
                    }; //JsonSerialize.Instance.ObjectToJson();
                }

            }
            return rs;
        }

        /// <summary>
        /// 这是一个蛋疼的接口，给小区中奖的业主返业主卡金额到帐号（不通过实体卡兑换）
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ResultMessageViewModel Danteng()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                Guid bid = Guid.Parse("309FFB15-3CAA-4C92-9F1E-96EDAF5A81FD"); //排除东原D7小区
                var list = db.Core_ScoreExchange.Where(o => o.E_BuildingId != bid && o.E_Flag == (int)FilmFlagEnum.业主卡 && o.E_Status == 0);
                foreach (var item in list)
                {
                    var ban = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == item.E_UId);
                    if (ban != null)
                    {
                        //累计业主卡余额
                        ban.B_CouponMoney += item.E_Score;


                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = item.E_Time,
                            B_Title = string.Format("活动玩老虎机，获得{0}元业主卡", item.E_Score),
                            B_Money = item.E_Score,
                            B_UId = item.E_UId,
                            B_Phone = item.E_Phone,
                            B_Module = (int)BillEnum.平台业主卡,
                            B_OrderId = null,
                            B_Type = (int)MemberRoleEnum.会员,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0
                        };
                        //添加账单
                        db.Core_BillMember.Add(bill);

                        item.E_Status = 1;
                    }
                }
                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
                return rs;
            }
        }
    }

    public class ScoreHistoryCore : BaseRepository<Core_ScoreHistory>
    {
    }
}
