using System;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;

namespace JiGuangLinXin.App.Core
{
    public class AuditingVillageCore : BaseRepository<Core_AuditingVillage>
    {
        //private VillageCore vCore = new VillageCore();
        //private UserCore uCore = new UserCore();
        /// <summary>
        /// С��ʵ�����
        /// </summary>
        /// <param name="id">��˼�¼��ID</param>
        /// <param name="ckName">����˵��ʺ�</param>
        /// <param name="ckId">����˵�ID</param>
        /// <param name="buildingId">С��ID</param>
        /// <param name="state">���״̬</param>
        /// <param name="remark">��˷���</param>
        /// <param name="role">��ɫ��0���� ManagerRoleEnum��</param>
        /// <returns></returns>
        public bool BuildingAuditingCheck(Guid id, string ckName, Guid ckId, Guid? buildingId, int state, string remark, int role = 0)
        {
            //bool rs = false;

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                var aud =
                    db.Set<Core_AuditingVillage>()
                        .FirstOrDefault(
                            o => o.A_Id == id && o.A_Status == (int)AuditingEnum.δ��֤);
                 
                if (aud != null)  //��֤��Ч
                {
                    //1�޸���֤��¼��״̬
                    aud.A_Status = state;
                    aud.A_CheckBack = remark;//string.Format("#����ˣ�{0},{1}#", ckId, ckName);  //��¼����˵�ID��Name
                    //aud.A_Remark = remark;
                    aud.A_CheckTime = DateTime.Now;
                    aud.A_Role = role;
                     
                    Core_User user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == aud.A_UId);

                    //2�����ͨ����ˣ����û����뵽����Ⱥ
                    if (state == (int)AuditingEnum.��֤�ɹ�)
                    {
                        var building = db.Set<Core_Village>().FirstOrDefault(o => o.V_Id == buildingId);//��פ��С��
                        if (building == null)  //С��������
                        {
                            return false;
                        }
                        // ����һ�������С��Ⱥ��Ϣ���п����� ���������С����
                        aud.A_BuildingId = building.V_Id;
                        aud.A_BuildingName = building.V_BuildingName;

                        db.Set<Core_AuditingVillage>().Attach(aud);
                        db.Entry(aud).State = EntityState.Modified;


                        building.V_Number += 1;//�ۼ��û�����


                        //����һ���û���С�����п���С�����½���
                        user.U_BuildingId = building.V_Id;
                        user.U_BuildingName = building.V_BuildingName;


                        if (string.IsNullOrEmpty(building.V_ChatID))  //С����û�л���Ⱥ
                        {
                            //����Ⱥ
                            string chatid = HuanXin.CreateQun("group_" + building.V_Id);

                            building.V_ChatID = chatid;
                        }
                        db.Set<Core_Village>().Attach(building);
                        db.Entry(building).State = EntityState.Modified;//3���� ����

                        //4 �û����뵽����Ⱥ
                        HuanXin.AccountQunJoin(building.V_ChatID, user.U_ChatID);


                    }
                    //5��Ա�޸���֤״̬
                    user.U_AuditingState = state;

                    db.Set<Core_User>().Attach(user);
                    db.Entry(user).State = EntityState.Modified;



                    //6 �����˼�¼
                    db.Sys_CheckHistory.Add(new Sys_CheckHistory()
                    {
                        H_AdminId = ckId,
                        H_AdminName = ckName,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.�û���֤,
                        H_ProId = aud.A_Id.ToString(),
                        H_ProName = aud.A_TrueName,
                        H_Role = role,//0��������Ա 1�̼ҹ���Ա
                        H_State = 0,
                        H_Time = DateTime.Now,
                        H_Tips = remark
                    });



                    var rs = db.SaveChanges() > 0;
                     
                    return rs; 
                }
            }


            return false;
        }

        public string CreateHuanxinGroup(string gpName)
        {
            return HuanXin.CreateUser(gpName).ToString();

            //����Ⱥ
            return HuanXin.CreateQun("group_" + gpName);
        }
    }
}
