using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;

namespace JiGuangLinXin.App.Core
{
    public class UserCore : BaseRepository<Core_User>
    {
        /// <summary>
        /// 会员注册
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Reg(Core_User obj, string cityName)
        {
            string chatId = "lx_" + obj.U_Id;
            if (HuanXin.CreateUser(chatId, obj.U_NickName))  //添加到环信用户  
            {
                obj.U_ChatID = chatId;
                using (DbContext db = new LinXinApp20Entities())
                {
                    db.Entry<Core_User>(obj).State = EntityState.Added;  //1添加用户基础信息

                    Core_Balance balance = new Core_Balance() { B_AccountId = obj.U_Id, B_Balance = 0, B_EncryptCode = "", B_ForzenMoney = 0, B_PayPwd = "", B_Role = (int)MemberRoleEnum.会员 };
                    db.Entry<Core_Balance>(balance).State = EntityState.Added;  //2添加用户帐号基本信息

                    Sys_OperateLog log = new Sys_OperateLog()
                    {
                        L_Desc = string.Format("新用户注册#注册城市：{2}，具体位置：{0},{1}", obj.U_CoordX, obj.U_CoordY, cityName),
                        L_DriverType = obj.U_RegisterSource,
                        L_Flag = (int)ModuleEnum.注册,
                        L_Phone = obj.U_LoginPhone,
                        L_Status = 0,
                        L_Time = DateTime.Now,
                        L_UId = obj.U_Id,
                        L_Url = "/User/Reg"
                    };
                    db.Entry<Sys_OperateLog>(log).State = EntityState.Added;  //3添加用户注册操作日志

                    return db.SaveChanges() > 0;
                }
            }
            return false;
        }




        /// <summary>
        /// 获得好(邻)友列表
        /// </summary>
        /// <param name="buildingId">社区ID</param>
        /// <param name="uid">用户ID</param>
        /// <returns></returns>
        public List<UserFriendViewModel> GetFriendList(string buildingId, Guid uid)
        {
            List<UserFriendViewModel> list = null;
            if (!string.IsNullOrEmpty(buildingId))  //有小区id 查询邻友
            {
                Guid bid = Guid.Parse(buildingId);
                list = base.LoadEntities(o => o.U_BuildingId == bid && o.U_Status != (int)UserStatusEnum.冻结).Select(o => new UserFriendViewModel()
                {
                    UId = o.U_Id,
                    Logo = o.U_Logo,
                    NikeName = o.U_NickName,
                    Phone = o.U_LoginPhone,
                    BuildingId = o.U_BuildingId,
                    BuildingName = o.U_BuildingName,
                    CityName = o.U_City,
                    Age = o.U_Age,
                    Sex = o.U_Sex,
                    Huanxin = o.U_ChatID,
                    State = o.U_Status,
                    ManagerFlag = o.U_AuditingManager
                }).ToList();
            }
            else  //查询好友
            {
                string sql =
                    @"select   a.U_Id as UId,a.U_Logo as Logo,a.U_NickName as NikeName,a.U_LoginPhone as Phone,a.U_BuildingId as BuildingId,a.U_BuildingName as BuildingName,a.U_City as CityName,a.U_Age as Age,a.U_Sex as Sex,a.U_ChatID as Huanxin,a.U_Status as State,a.U_AuditingManager as ManagerFlag
                                 from Core_User a
                                inner join (select F_UId,F_FriendId from Core_UserFriend where F_UId='{0}' or F_FriendId = '{0}') b
                                on a.U_Id = b.F_UId or a.U_Id = b.F_FriendId  where a.U_Id<>'{0}'";


                sql = string.Format(sql, uid);
                using (DbContext db = new LinXinApp20Entities())
                {
                    list = db.Database.SqlQuery<UserFriendViewModel>(sql).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        public bool ResetPwd(Guid uid, string newPwd)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var obj = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);
                if (obj != null)
                {
                    obj.U_LoginPwd = Md5Extensions.MD5Encrypt(newPwd + obj.U_PwdCode); ;
                }

                db.Set<Core_User>().Attach(obj);
                db.Entry(obj).State = EntityState.Modified;

                Sys_OperateLog log = new Sys_OperateLog()
                {
                    L_Desc = string.Format("用户:{0},{1}重置密码", obj.U_NickName, obj.U_LoginPhone),
                    L_DriverType = obj.U_RegisterSource,
                    L_Flag = (int)ModuleEnum.登录,
                    L_Phone = obj.U_LoginPhone,
                    L_Status = 0,
                    L_Time = DateTime.Now,
                    L_UId = obj.U_Id,
                    L_Url = "/User/ResetPwd"
                };
                db.Entry<Sys_OperateLog>(log).State = EntityState.Added;  //3添加用户操作日志

                return db.SaveChanges() > 0;
            }

            return false;
        }
        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="newPwd"></param>
        /// <param name="pla">平台</param>
        /// <returns></returns>
        public bool SetPayPwd(Guid uid, string newPwd, int pla)
        {
            using (DbContext db = new LinXinApp20Entities())
            {
                var obj = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                if (obj != null)
                {
                    string pwdCode = new CreateRandomStr().GetRandomString(4);//支付密码干扰码\
                    obj.B_EncryptCode = pwdCode;
                    obj.B_PayPwd = Md5Extensions.MD5Encrypt(newPwd + pwdCode); ;
                }

                db.Set<Core_Balance>().Attach(obj);
                db.Entry(obj).State = EntityState.Modified;

                Sys_OperateLog log = new Sys_OperateLog()
                {
                    L_Desc = string.Format("设置支付密码"),
                    L_DriverType = pla,
                    L_Flag = (int)ModuleEnum.个人信息,
                    L_Phone = "",
                    L_Status = 0,
                    L_Time = DateTime.Now,
                    L_UId = obj.B_AccountId,
                    L_Url = "/UserCenter/SetPayPwd"
                };
                db.Entry<Sys_OperateLog>(log).State = EntityState.Added;  //3添加用户操作日志

                return db.SaveChanges() > 0;
            }

            return false;
        }



        /// <summary>
        /// 获得申请小区验证的用户列表
        /// </summary>
        /// <returns></returns>
        public ResultMessageViewModel GetAuditingUserList(Guid? buildingId, int pn, int rows)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                if (buildingId.HasValue)
                {
                    rs.Data =
                        db.Set<Core_AuditingVillage>()
                            .Where(o => o.A_BuildingId == buildingId)
                            .Join(db.Set<Core_User>(), o => o.A_UId, u => u.U_Id, (o, u) => new
                            {
                                uId = u.U_Id,
                                uLogo = u.U_Logo,
                                uNickname = u.U_NickName,
                                uAge = u.U_Age,
                                uSex = u.U_Sex,
                                buildingName = o.A_BuildingName,
                                buildingId = o.A_BuildingId,
                                trueName = o.A_TrueName,
                                buildingImg = o.A_ImgBuilding,
                                buildingFee = o.A_ImgFee,
                                state = o.A_Status,
                                time = o.A_Time,
                                tips = o.A_Title,
                                aId = o.A_Id,
                                phone = o.A_UPhone
                            }).OrderByDescending(o => o.time).Skip(pn * rows).Take(rows).ToList();
                }
            }
            return rs;
        }

    }
}
