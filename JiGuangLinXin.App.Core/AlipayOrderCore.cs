using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;

namespace JiGuangLinXin.App.Core
{
    public class AlipayOrderCore : BaseRepository<Core_AlipayOrder>
    {


        /// <summary>
        /// ��������ӿ�֧�������֧��������
        /// </summary>
        /// <param name="obj">��ӵĶ���</param>
        /// <returns>0�ɹ� 1�����Ѿ����� 2�����Ѿ�����</returns>
        public int ChinapayAlipayOrder(Core_AlipayOrder obj)
        {
            var od = LoadEntity(o => o.A_OrderNo == obj.A_OrderNo);
            if (od != null && od.A_Status == 0)
            {
                return 2;
            }
            if (od == null)
            {

                using (DbContext db = new LinXinApp20Entities())
                {
                    //1���֧��������
                    db.Entry<Core_AlipayOrder>(obj).State = EntityState.Added;
                    //2��Ӹ����˵�
                    Core_BillMember bill = new Core_BillMember() { B_Flag = (int)BillFlagEnum.ƽ̨��ˮ, B_Module = (int)BillEnum.����ɷ�, B_Money = obj.A_Money, B_OrderId = obj.A_Id, B_Phone = obj.A_Phone, B_Remark = "", B_Status = 0, B_Time = obj.A_Time, B_Title = obj.A_Remark, B_UId = obj.A_UId, B_Type = (int)MemberRoleEnum.��Ա };
                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;


                    db.SaveChanges();//��Ԫ�����������ύ
                    return 0;
                }

            }
            //else if (od.A_Status ==1)  //�Ѿ����ڣ�δ����
            //{

            //} 
            return 1;
        }


        /// <summary>
        /// ����ɷ�
        /// </summary>
        /// <param name="aliOrder">֧��������</param> 
        /// <returns></returns>
        public bool Pay(Core_AlipayOrder aliOrder)
        {

           
            using (DbContext db = new LinXinApp20Entities())
            {
                var od = db.Set<Core_AlipayOrder>().FirstOrDefault(o => o.A_OrderNo == aliOrder.A_OrderNo);

                if (od != null)  //�˵��Ѿ�����
                {
                    return false;
                }


                //�û���ˮ
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                    B_Module = (int)BillEnum.��ֵ,
                    B_Money = aliOrder.A_Money,
                    B_OrderId = aliOrder.A_Id,
                    B_Phone = aliOrder.A_Phone,
                    B_Remark = "",
                    B_Status = 0,
                    B_Time = aliOrder.A_Time,
                    B_Title = aliOrder.A_Remark,
                    B_UId = aliOrder.A_UId,
                    B_Type = (int)MemberRoleEnum.��Ա
                };

                //ƽ̨��ˮ
                Sys_BillMaster billMaster = new Sys_BillMaster()
                {
                    B_Flag = (int)BillFlagEnum.ƽ̨��ˮ,
                    B_Module = (int)BillEnum.��ֵ,
                    B_Money = aliOrder.A_Money,
                    B_OrderId = aliOrder.A_Id,
                    B_Phone = aliOrder.A_Phone,
                    B_Remark = "",
                    B_Status = 0,
                    B_Time = aliOrder.A_Time,
                    B_Title = aliOrder.A_Remark,
                    B_UId = aliOrder.A_UId,
                    B_Type = (int)MemberRoleEnum.��Ա
                };


                //1���֧��������
                db.Entry<Core_AlipayOrder>(aliOrder).State = EntityState.Added;
                //2��Ӹ����˵�
                db.Entry<Core_BillMember>(bill).State = EntityState.Added;
                //3.���ƽ̨�˵�
                db.Entry<Sys_BillMaster>(billMaster).State = EntityState.Added;

                //4������
                var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == aliOrder.A_UId);
                balance.B_Balance += aliOrder.A_Money;
                db.Set<Core_Balance>().Attach(balance);
                db.Entry<Core_Balance>(balance).State = EntityState.Modified;


                return db.SaveChanges() > 0;//��Ԫ�����������ύ

            }


            return false;
        }
    }
}
