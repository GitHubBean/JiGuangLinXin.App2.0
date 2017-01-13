using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    /// <summary>
    /// 手机流量充值
    /// </summary>
    public class CellphoneTrafficCore
    {
        /// <summary> 
        /// 匹配移动手机号 
        /// </summary> 
        public const string PATTERN_CMCMOBILENUM = @"^1(3[4-9]|5[012789]|8[23478])\d{8}$";
        /// <summary> 
        /// 匹配电信手机号 
        /// </summary> 
        public const string PATTERN_CTCMOBILENUM = @"^1([35]3|8[019])\d{8}$";//@"^18[09]\d{8}$"
        /// <summary> 
        /// 匹配联通手机号 
        /// </summary> 
        public const string PATTERN_CUTMOBILENUM = @"^1(3[0-2]|5[56]|8[56])\d{8}$";

        /// <summary> 
        /// 匹配CDMA手机号 
        /// </summary> 
        //public const string PATTERN_CDMAMOBILENUM = @"^1[35]3d{8}$";


        /// <summary>
        /// 根据手机号码判断运营商
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public int GetPhoneType(string phone)
        {
            if (Regex.IsMatch(phone, PATTERN_CUTMOBILENUM))
            {
                return 1;
            }

            if (Regex.IsMatch(phone, PATTERN_CMCMOBILENUM))
            {
                return 2;
            }

            if (Regex.IsMatch(phone, PATTERN_CTCMOBILENUM))
            {
                return 3;
            }
            return -1;
        }

        /// <summary>
        /// 聚合充值失败退积分
        /// </summary>
        /// <param name="uid">业主ID</param>
        /// <param name="score">所退积分</param>
        /// <param name="orderno">订单号</param>
        /// <returns></returns>
        public ResultMessageViewModel Refund(Guid uid,int score,string orderno)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            return rs;
        }

        /// <summary>
        /// 业主签到
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <returns></returns>
        public ResultMessageViewModel SingIn(Guid uid)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //1.业主是否存在
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结);
                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return rs;
                }

                //2.业主今天是否已经签到过了
                DateTime st = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                DateTime et = Convert.ToDateTime(DateTime.Now.AddDays(1).ToShortDateString());
                var his = db.Core_ScoreHistory.Where(o => o.H_UId == uid).OrderByDescending(o => o.H_Time).FirstOrDefault(o => o.H_Time > st && o.H_Time < et);
                if (his != null)
                {
                    rs.Msg = "今天您已签到，请明天继续";
                    return rs;
                }

                //3。可以签到
                var scoreInfo = db.Core_Score.FirstOrDefault(o => o.S_AccountId == uid);
                if (scoreInfo == null)  //第一次签到
                {
                    scoreInfo = new Core_Score()
                    {
                        S_AccountId = uid,
                        S_EncryptCode = "",
                        S_ForzenScore = 0,
                        S_PayPwd = "",
                        S_Remark = "",
                        S_Role = (int)MemberRoleEnum.会员,
                        S_Score = 1
                    };
                    db.Core_Score.Add(scoreInfo);
                }
                else
                {
                    scoreInfo.S_Score += 1;  //累计积分
                }

                //添加签到记录
                var hisInfo = new Core_ScoreHistory()
                {
                    H_Phone = user.U_LoginPhone,
                    H_Role = (int)MemberRoleEnum.会员,
                    H_Score = 1,
                    H_Time = DateTime.Now,
                    H_TimeUseful = DateTime.Now,
                    H_Title = "签到获得1个签到点",
                    H_TypeId = 1,
                    H_UId = uid
                };
                db.Core_ScoreHistory.Add(hisInfo);

                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "获得1个签到点，赶快兑换流量吧！";
                    rs.Data = new
                    {
                        score = 1
                    };

                }
            }
            return rs;
        }

        /// <summary>
        /// 所有流量列表
        /// </summary>
        /// <returns></returns>
        public List<CellphoneTrafficViewModel> GetCellphoneTrafficList()
        {
            List<CellphoneTrafficViewModel> list = new List<CellphoneTrafficViewModel>();
            list.Add(new CellphoneTrafficViewModel() { TraId = 1, Score = 3, Traffic = "20M", Remark = "", LimitCount = 300 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 2, Score = 8, Traffic = "50M", Remark = "", LimitCount = 300 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 3, Score = 19, Traffic = "100M", Remark = "", LimitCount = 150 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 4, Score = 32, Traffic = "150M", Remark = "", LimitCount = 150 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 5, Score = 50, Traffic = "200M", Remark = "", LimitCount = 100 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 6, Score = 78, Traffic = "500M", Remark = "", LimitCount = 50 });
            list.Add(new CellphoneTrafficViewModel() { TraId = 7, Score = 100, Traffic = "1G", Remark = "", LimitCount = 30 });

            return list;
        }


    }

}
