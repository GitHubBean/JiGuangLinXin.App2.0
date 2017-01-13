using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.Core
{
    public class CheckHistoryCore : BaseRepository<Sys_CheckHistory>
    {
    }

    /// <summary>
    /// 审核退款
    /// </summary>
    public class RefundCore
    {
        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="billNo">关联的账单编号</param>
        /// <param name="uid">业主编号</param>
        /// <param name="uFlag">标识</param>
        /// <param name="money">金额</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        public string Back(int billNo, Guid uid, int uFlag, decimal money, string remark, Guid adminId, string adminName)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                string proName = "无效";
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_AuditingState == 1 && o.U_Status == 0);
                if (user == null) return "业主不存在";
                decimal billMoney = -1;
                Core_BillMember billInfo = null;
                Core_ScoreExchange scoreBill = null;
                if (uFlag == 0)
                {
                    var bill = db.Core_BillMember.FirstOrDefault(o => o.B_Id == billNo && o.B_UId == uid);
                    billMoney = Math.Abs(bill.B_Money);
                    proName = "【平台退返余额】-" + bill.B_Title;

                    billInfo = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = "业主充值失败,官方退款",
                        B_Money = billMoney,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.便民退款,
                        B_Type = (int)MemberRoleEnum.会员,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0,
                        B_Remark =  proName

                    };

                }
                else if (uFlag == 1)
                {

                    var bill = db.Core_ScoreExchange.FirstOrDefault(o => o.E_Id == billNo && o.E_UId == uid);
                    billMoney = Math.Abs(bill.E_Score);
                    proName = string.Format("{0}-{1}个积分,{2}", "【平台退返积分】", billMoney, bill.E_Title);


                    scoreBill = new Core_ScoreExchange()
                     {
                         E_BuildingId = user.U_BuildingId,
                         E_BuildingName = user.U_BuildingName,
                         E_Id = 0,
                         E_Module = (int)EventH5ModuleEnum.退返积分,
                         E_Phone = user.U_LoginPhone,
                         E_Role = (int)MemberRoleEnum.会员,
                         E_Score = Convert.ToInt32(billMoney),
                         E_Status = 0,
                         E_Time = DateTime.Now,
                         E_Flag = (int)FilmFlagEnum.积分,
                         E_Title = proName,
                         E_UId = uid
                     };

                }
                else
                {
                    return "数据处理异常";
                }



                if (billMoney != -1 && money == billMoney) //合法的订单退款
                {
                    string bNo = billNo.ToString();
                    var hisExist = db.Sys_CheckHistory.Any(o => o.H_Flag == (int)CheckHistoryStateEnum.用户退款 && o.H_ProId == bNo && o.H_Role == uFlag);
                    if (hisExist)
                    {
                        return "已退款";
                    }
                    var his = new Sys_CheckHistory()
                    {
                        H_AdminId = adminId,
                        H_AdminName = adminName,
                        H_CheckState = 1,
                        H_Flag = (int)CheckHistoryStateEnum.用户退款,
                        H_ProId = billNo.ToString(),
                        H_ProName = proName,
                        H_Role = uFlag, //0余额 1积分
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = string.Format("备注：{0} 【数额：{1}】", remark,money)
                    };
                    db.Sys_CheckHistory.Add(his);  //1.退款记录

                    if (billInfo != null)
                    {
                        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == uid);
                        balance.B_Balance += billInfo.B_Money;  //2.累计金额
                        db.Core_BillMember.Add(billInfo);  //3.添加账单
                    }
                    if (scoreBill != null)
                    {
                        var scoreInfo = db.Core_Score.FirstOrDefault(o => o.S_AccountId == uid);
                        scoreInfo.S_Score += scoreBill.E_Score;    //2.累计积分

                        db.Core_ScoreExchange.Add(scoreBill);  //3.添加积分记录
                    }

                    if (db.SaveChanges() > 0)
                    {
                        return "ok";
                    }
                }
                return "数据处理异常";
            }
        }
    }
}
