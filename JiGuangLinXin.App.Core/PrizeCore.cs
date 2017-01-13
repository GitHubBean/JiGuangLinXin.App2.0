using System;
using System.Collections.Generic;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class PrizeCore : BaseRepository<Core_Prize>
    {
    }

    public class PrizeDetailCore : BaseRepository<Core_PrizeDetail>
    {
        /// <summary>
        /// ��ȡ �Ǽʴ��ص� ��ͳ����Ϣ
        /// </summary>
        /// <param name="buildingId">С��ID</param>
        /// <returns></returns>
        public int JoinCountByBuildingId(Guid buildingId)
        {

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                return
                    db.Core_PrizeDetail.Join(db.Core_User, a => a.PD_UId, b => b.U_Id,
                        (a, b) => new { bId = b.U_BuildingId }).Where(o => o.bId == buildingId).GroupBy(o => o.bId).Count();
            }
            return 0;
        }

        /// <summary>
        /// ��ȡ �Ǽʴ��� ÿһ���н�����
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public List<dynamic> JoinCountDetailByBuildingId(Guid buildingId)
        {

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var info = db.Core_PrizeDetail.Join(db.Core_User, a => a.PD_UId, b => b.U_Id,
                        (a, b) => new { bId = b.U_BuildingId, item = a.PD_Award }).Where(o => o.bId == buildingId).ToList().Select(o => new { item = o.item.Substring(0, 1) }).GroupBy(o => o.item).Select(o => new
                        {
                            item = o.Key,
                            count = o.Count()
                        });


                int i0 = 0;
                int i1 = 0;
                int i2 = 0;
                int i3 = 0;
                int i4 = 0;

                foreach (var temp in info)
                {
                    if (temp.item == "4" || temp.item == "7")
                    {
                        i0 += temp.count;
                    }

                    if (temp.item == "5")
                    {
                        i1 += temp.count;

                    }

                    if (temp.item == "0" || temp.item == "1" || temp.item == "6")
                    {
                        i2 += temp.count;
                    }

                    if (temp.item == "3")
                    {
                        i4 += temp.count;
                    }
                }
                List<dynamic> list = new List<dynamic>()
                {
                    new {item = 0,count =i0,title = "����н�����"},
                    new {item = 1,count = i1,title = "VR�۾��н�����"},
                    new {item = 2,count = i2,title = "ҵ�����н�����"},
                    new {item = 3,count = i3,title = "iPhone�н�����"},
                    new {item = 4,count = i4,title = "δ�н�����"}
                };
                return list;
            }
            return null;
        }




        //��Ѿ�������

        ///// <summary>
        ///// ҵ����¼�ɹ����������н���
        ///// </summary>
        ///// <param name="phone"></param>
        //public void BackAward(string phone)
        //{
        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        //����˵�
        //        var user = db.Core_User.FirstOrDefault(o => o.U_LoginPhone == phone);
        //        Guid uid = user.U_Id;
        //        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == uid);

        //        if (balance == null)
        //        {
        //            return;
        //        }
        //        var prList = db.Core_PrizeDetail.Where(o => o.PD_UPhone == phone && o.PD_Flag == 0 && (o.PD_LuckGift > 0 || o.PD_OwnerCard > 0));

        //        foreach (var info in prList)
        //        {
        //            //todo: ���Ƕҽ� ����

        //            if (info.PD_OwnerCard > 0)
        //            {
        //                Core_BillMember bill = new Core_BillMember()
        //                {
        //                    B_Flag = 0,
        //                    B_Module = (int)BillEnum.ƽ̨ҵ����,
        //                    B_Money = info.PD_OwnerCard,
        //                    B_OrderId = info.PD_Id,
        //                    B_Phone = info.PD_UPhone,
        //                    B_Remark = "���Ź��񡱴�齱�����ҵ����",
        //                    B_Status = 0,
        //                    B_Time = info.PD_Time,
        //                    B_Title = string.Format("�齱���ҵ����{0}Ԫ", info.PD_OwnerCard.ToString("N")),
        //                    B_UId = uid,
        //                    B_Type = (int)MemberRoleEnum.��Ա
        //                };
        //                db.Core_BillMember.Add(bill);
        //                balance.B_CouponMoney += info.PD_OwnerCard;  //�ۼ����


        //                info.PD_Flag = 1;  //����н�״̬
        //                info.PD_TimeAward = DateTime.Now;
        //                info.PD_UId = uid;
        //            }
        //            else if (info.PD_LuckGift > 0)
        //            {

        //                Core_BillMember bill = new Core_BillMember()
        //                {
        //                    B_Flag = 0,
        //                    B_Module = (int)BillEnum.���,
        //                    B_Money = info.PD_LuckGift,
        //                    B_OrderId = info.PD_Id,
        //                    B_Phone = info.PD_UPhone,
        //                    B_Remark = "���Ź��񡱴�齱,��ú��",
        //                    B_Status = 0,
        //                    B_Time = info.PD_Time,
        //                    B_Title = string.Format("�齱��ú��{0}Ԫ", info.PD_LuckGift.ToString("N")),
        //                    B_UId = uid,
        //                    B_Type = (int)MemberRoleEnum.��Ա
        //                };
        //                db.Core_BillMember.Add(bill);

        //                balance.B_Balance += info.PD_LuckGift;//�ۼ����


        //                info.PD_Flag = 1;  //����н�״̬
        //                info.PD_TimeAward = DateTime.Now;
        //                info.PD_UId = uid;
        //            }

        //            //info.PD_Id = Guid.NewGuid();
        //            //info.PD_UId = user.U_Id;
        //            //info.PD_UPhone = user.U_LoginPhone;
        //            //info.PD_Flag = 1;
        //            //info.PD_Time = DateTime.Now; 

        //            //db.Core_PrizeDetail.Add(info);  

        //            Sys_OperateLog log = new Sys_OperateLog() { L_Desc = "�μӹٷ����Ź��񡱳齱�������������", L_DriverType = 3, L_Flag = (int)ModuleEnum.�Ź���齱, L_Phone = info.PD_UPhone, L_UId = info.PD_UId, L_Url = "/Prize/Lottery", L_Status = 0, L_Time = DateTime.Now };

        //            db.Sys_OperateLog.Add(log);  //�����־

        //        }
        //        db.SaveChanges();
        //    }
        //}

        ///// <summary>
        ///// App�ڲ��齱
        ///// </summary>
        ///// <param name="uid">�û�ID</param>
        ///// <param name="info">������</param>
        ///// <returns></returns>
        //public ResultMessageViewModel TurnInside(Guid uid, Core_PrizeDetail info)
        //{
        //    ResultMessageViewModel rs = new ResultMessageViewModel();
        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid);
        //        if (user == null)
        //        {
        //            rs.Msg = "�û�������";
        //            return rs;
        //        }


        //        //info.PD_Id = Guid.NewGuid();

        //        //var prize = db.Core_Prize.FirstOrDefault(o => o.P_UId == uid);
        //        //if (prize == null)
        //        //{
        //        //    var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid);
        //        //    if (user == null)
        //        //    {
        //        //        rs.Msg = "�û�������";
        //        //        return rs;
        //        //    }


        //        //    prize = new Core_Prize()
        //        //    {
        //        //        P_Count = 1,
        //        //        P_Id = Guid.NewGuid(),
        //        //        P_RemainCount = 0,
        //        //        P_Time = DateTime.Now,
        //        //        P_TimeUseful = DateTime.Now.AddDays(60),
        //        //        P_TypeId = 1,
        //        //        P_UId = user.U_Id,
        //        //        P_UPhone = user.U_LoginPhone
        //        //    };

        //        //    db.Core_Prize.Add(prize);  //����н���¼i�������Ѿ���ȡ��Ʒ��
        //        //}
        //        //else
        //        //{
        //        //    prize.P_RemainCount -= 1; //����һ�γ齱������
        //        //}

        //        var pdInfo = db.Core_PrizeDetail.FirstOrDefault(o => o.PD_Id == info.PD_Id);
        //        if (pdInfo == null || pdInfo.PD_TimeUseful < DateTime.Now)
        //        {
        //            rs.Msg = "�ҽ�ʧ��";
        //            return rs;

        //        }

        //        //����˵�
        //        var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
        //        if (balance == null)
        //        {
        //            rs.Msg = "�û��˻�������";
        //            return rs;
        //        }
        //        if (info.PD_OwnerCard > 0)
        //        {
        //            Core_BillMember bill = new Core_BillMember()
        //            {
        //                B_Flag = 0,
        //                B_Module = (int)BillEnum.ƽ̨ҵ����,
        //                B_Money = info.PD_OwnerCard,
        //                B_OrderId = info.PD_Id,
        //                B_Phone = info.PD_UPhone,
        //                B_Remark = "���Ź��񡱴�齱�����ҵ����",
        //                B_Status = 0,
        //                B_Time = info.PD_Time,
        //                B_Title = string.Format("�齱���ҵ����{0}Ԫ", info.PD_OwnerCard.ToString("N")),
        //                B_UId = uid,
        //                B_Type = (int)MemberRoleEnum.��Ա
        //            };
        //            db.Core_BillMember.Add(bill);
        //            balance.B_CouponMoney += info.PD_OwnerCard;  //�ۼ����


        //            pdInfo.PD_Flag = 1;  //����н�״̬
        //            pdInfo.PD_TimeAward = DateTime.Now;
        //        }
        //        else if (info.PD_LuckGift > 0)
        //        {

        //            Core_BillMember bill = new Core_BillMember()
        //            {
        //                B_Flag = 0,
        //                B_Module = (int)BillEnum.���,
        //                B_Money = info.PD_LuckGift,
        //                B_OrderId = info.PD_Id,
        //                B_Phone = info.PD_UPhone,
        //                B_Remark = "���Ź��񡱴�齱,��ú��",
        //                B_Status = 0,
        //                B_Time = info.PD_Time,
        //                B_Title = string.Format("�齱��ú��{0}Ԫ", info.PD_LuckGift.ToString("N")),
        //                B_UId = uid,
        //                B_Type = (int)MemberRoleEnum.��Ա
        //            };
        //            db.Core_BillMember.Add(bill);

        //            balance.B_Balance += info.PD_LuckGift;//�ۼ����


        //            pdInfo.PD_Flag = 1;  //����н�״̬
        //            pdInfo.PD_TimeAward = DateTime.Now;
        //        }

        //        //info.PD_Id = Guid.NewGuid();
        //        //info.PD_UId = user.U_Id;
        //        //info.PD_UPhone = user.U_LoginPhone;
        //        //info.PD_Flag = 1;
        //        //info.PD_Time = DateTime.Now; 

        //        //db.Core_PrizeDetail.Add(info);  

        //        Sys_OperateLog log = new Sys_OperateLog() { L_Desc = "�μӹٷ����Ź��񡱳齱�", L_DriverType = 3, L_Flag = (int)ModuleEnum.�Ź���齱, L_Phone = info.PD_UPhone, L_UId = info.PD_UId, L_Url = "/Prize/Lottery", L_Status = 0, L_Time = DateTime.Now };

        //        db.Sys_OperateLog.Add(log);  //�����־



        //        if (db.SaveChanges() > 0)
        //        {
        //            rs.State = 0;
        //            rs.Data = new
        //            {
        //                awards = info.PD_Award,
        //                token = ""
        //            };
        //            rs.Msg = "ok";
        //        }
        //    }
        //    return rs;
        //}




        ///// <summary>
        ///// ������ȥ���û���д�ֻ������콱
        ///// </summary>
        ///// <param name="phone">�绰����</param>
        ///// <param name="info">�н�����</param>
        ///// <returns></returns>
        //public ResultMessageViewModel TurnOutside(string phone, Core_PrizeDetail info)
        //{
        //    ResultMessageViewModel rs = new ResultMessageViewModel();

        //    using (LinXinApp20Entities db = new LinXinApp20Entities())
        //    {
        //        //�û��Ѿ��μӹ�һ�Ρ�xx�����һ�س齱
        //        var prize = db.Core_PrizeDetail.FirstOrDefault(o => o.PD_UPhone == phone && o.PD_TypeId == 0 && o.PD_Round == 1);

        //        if (prize != null)
        //        {
        //            rs.Msg = "�˵绰�����Ѿ��μӹ����γ齱�������ظ��μ�";
        //            return rs;
        //        }
        //        var user = db.Core_User.FirstOrDefault(o => o.U_LoginPhone == phone);

        //        if (user != null)
        //        {
        //            rs.State = 1;
        //            rs.Msg = "���ʺ�������ҵ�������¼����APP�μӻ��";
        //            return rs;
        //        }

        //        //prize = new Core_Prize()
        //        //{
        //        //    P_Count = 1,
        //        //    P_Id = Guid.NewGuid(),
        //        //    P_RemainCount = 1,  //û��ע����û�
        //        //    P_Time = DateTime.Now,
        //        //    P_TimeUseful = DateTime.Now.AddDays(60),
        //        //    P_TypeId = 1,
        //        //    P_UId = Guid.Empty,
        //        //    P_UPhone = phone
        //        //};

        //        //db.Core_Prize.Add(prize);  //��ӳ齱����


        //        info.PD_Id = Guid.NewGuid();
        //        info.PD_UId = Guid.Empty;
        //        info.PD_UPhone = phone;
        //        info.PD_Flag = 0;  //δ��ȡ
        //        info.PD_Time = DateTime.Now;
        //        info.PD_Round = 1;//ֻ���ǵ�һ�س齱

        //        info.PD_TimeUseful = DateTime.Now.AddDays(60);

        //        db.Core_PrizeDetail.Add(info);  //����콱����
        //        if (db.SaveChanges() > 0)
        //        {
        //            rs.State = 0;
        //            rs.Msg = "ok";
        //        }
        //    }
        //    return rs;
        //}

        ///// <summary>
        ///// ���һ���齱��Ŀ
        ///// </summary>
        ///// <param name="uid">�н���ID</param>
        ///// <param name="phone">�绰</param>
        ///// <param name="round">�ڼ���</param>
        ///// <returns></returns>
        //public bool AddOne(Guid uid, string phone, int round)
        //{
        //    if (round > 2) //�����ֿ�ʼ
        //    {
        //        var info = base.LoadEntity(o => o.PD_UId == uid && o.PD_Round == round);
        //        var infoPre = base.LoadEntity(o => o.PD_UId == uid && o.PD_Round == round - 1);
        //        if (info == null && infoPre != null)  //��δ��ô��ֵĻ��ᣬ�����Ѿ�ȡ������һ�ֵ��ʸ�
        //        {

        //            var pd = new Core_PrizeDetail();
        //            pd.PD_Id = Guid.NewGuid();
        //            pd.PD_Flag = 0;
        //            pd.PD_Round = round;
        //            pd.PD_Time = DateTime.Now;
        //            pd.PD_TimeUseful = DateTime.Now.AddDays(60);
        //            pd.PD_UId = uid;
        //            pd.PD_UPhone = phone;

        //            if (base.AddEntity(pd) != null)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

    }
}
