using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class AuditingCashCore : BaseRepository<Core_AuditingCash>
    {

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool ApplyCash(Core_AuditingCash info)
        {

            using (DbContext db = new LinXinApp20Entities())
            {
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == info.M_UId);
                if (balance.B_Balance >= info.M_Money)  //����㹻
                {
                    //��1����������¼
                    db.Entry(info).State = EntityState.Added;

                    //��2�������������ֵĽ��
                    balance.B_Balance -= info.M_Money;
                    balance.B_ForzenMoney += info.M_Money;

                    db.Set<Core_Balance>().Attach(balance);
                    db.Entry(balance).State = EntityState.Modified;
                    //��3������˵���¼
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = "���յ��������룬�ȴ����",
                        B_Money = -info.M_Money,
                        B_UId = info.M_UId,
                        B_Phone = info.M_Phone,
                        B_Module = (int)BillEnum.����,
                        B_OrderId = info.M_Id,
                        B_Type = info.M_Role,
                        B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                        B_Status = 0
                    };
                    db.Entry(bill).State = EntityState.Added;


                    return db.SaveChanges() > 0;
                }
            }

            return false;
        }


        /// <summary>
        /// �������
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ResultMessageViewModel ApplyCashAuditing(Core_AuditingCash info)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                if (info.M_Status == (int)AuditingEnum.��֤�ɹ�)
                {
                    var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == info.M_UId);
                    if (balance.B_ForzenMoney > info.M_Money)  //����Ľ���㹻
                    {
                        //��2���ⶳ�������ֵĽ��
                        balance.B_ForzenMoney -= info.M_Money;
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry(balance).State = EntityState.Modified;

                        //��3������˵���¼
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "�������룬��ͨ����ˣ��ɹ�ת��",
                            B_Money = -info.M_Money,
                            B_UId = info.M_UId,
                            B_Phone = info.M_Phone,
                            B_Module = (int)BillEnum.����,
                            B_OrderId = info.M_Id,
                            B_Type = info.M_Role,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Status = 0,
                            B_Remark = info.M_CheckBack
                        };
                        db.Entry(bill).State = EntityState.Added;

                        //��4�����ƽ̨��ˮ��¼
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "�������룬��ͨ����ˣ��ɹ�ת��",
                            B_Money = -info.M_Money,
                            B_UId = info.M_UId,
                            B_Phone = info.M_Phone,
                            B_Module = (int)BillEnum.����,
                            B_OrderId = info.M_Id,
                            B_Type = info.M_Role,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Status = 0,

                            B_Remark = info.M_CheckBack
                        };
                        db.Entry(billMaster).State = EntityState.Added;

                    }
                }

                //��1��  ��˼�¼״̬���
                db.Set<Core_AuditingCash>().Attach(info);
                db.Entry(info).State = EntityState.Modified;

                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            return rs;

        }
    }
}
