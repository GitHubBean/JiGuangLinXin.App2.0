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
        /// 小区实名审核
        /// </summary>
        /// <param name="id">审核记录的ID</param>
        /// <param name="ckName">审核人的帐号</param>
        /// <param name="ckId">审核人的ID</param>
        /// <param name="buildingId">小区ID</param>
        /// <param name="state">审核状态</param>
        /// <param name="remark">审核反馈</param>
        /// <param name="role">角色（0超管 ManagerRoleEnum）</param>
        /// <returns></returns>
        public bool BuildingAuditingCheck(Guid id, string ckName, Guid ckId, Guid? buildingId, int state, string remark, int role = 0)
        {
            //bool rs = false;

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                var aud =
                    db.Set<Core_AuditingVillage>()
                        .FirstOrDefault(
                            o => o.A_Id == id && o.A_Status == (int)AuditingEnum.未认证);
                 
                if (aud != null)  //认证有效
                {
                    //1修改认证记录的状态
                    aud.A_Status = state;
                    aud.A_CheckBack = remark;//string.Format("#审核人：{0},{1}#", ckId, ckName);  //记录审核人的ID，Name
                    //aud.A_Remark = remark;
                    aud.A_CheckTime = DateTime.Now;
                    aud.A_Role = role;
                     
                    Core_User user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == aud.A_UId);

                    //2如果是通过审核，把用户加入到环信群
                    if (state == (int)AuditingEnum.认证成功)
                    {
                        var building = db.Set<Core_Village>().FirstOrDefault(o => o.V_Id == buildingId);//入驻的小区
                        if (building == null)  //小区不存在
                        {
                            return false;
                        }
                        // 更新一下申请的小区群信息（有可能是 申请的是新小区）
                        aud.A_BuildingId = building.V_Id;
                        aud.A_BuildingName = building.V_BuildingName;

                        db.Set<Core_AuditingVillage>().Attach(aud);
                        db.Entry(aud).State = EntityState.Modified;


                        building.V_Number += 1;//累计用户人数


                        //重置一下用户的小区，有可能小区是新建的
                        user.U_BuildingId = building.V_Id;
                        user.U_BuildingName = building.V_BuildingName;


                        if (string.IsNullOrEmpty(building.V_ChatID))  //小区还没有环信群
                        {
                            //创建群
                            string chatid = HuanXin.CreateQun("group_" + building.V_Id);

                            building.V_ChatID = chatid;
                        }
                        db.Set<Core_Village>().Attach(building);
                        db.Entry(building).State = EntityState.Modified;//3更新 社区

                        //4 用户加入到环信群
                        HuanXin.AccountQunJoin(building.V_ChatID, user.U_ChatID);


                    }
                    //5会员修改认证状态
                    user.U_AuditingState = state;

                    db.Set<Core_User>().Attach(user);
                    db.Entry(user).State = EntityState.Modified;



                    //6 添加审核记录
                    db.Sys_CheckHistory.Add(new Sys_CheckHistory()
                    {
                        H_AdminId = ckId,
                        H_AdminName = ckName,
                        H_CheckState = state,
                        H_Flag = (int)CheckHistoryStateEnum.用户认证,
                        H_ProId = aud.A_Id.ToString(),
                        H_ProName = aud.A_TrueName,
                        H_Role = role,//0超级管理员 1商家管理员
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

            //创建群
            return HuanXin.CreateQun("group_" + gpName);
        }
    }
}
