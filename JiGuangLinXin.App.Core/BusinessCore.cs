using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class BusinessCore : BaseRepository<Core_Business>
    {
        /// <summary>
        /// 获取服务商家
        /// </summary>
        /// <param name="buildingId">所在社区</param>
        /// <param name="category">分类</param>
        /// <returns></returns>
        public dynamic GetBusinessServiceByBuildingId(Guid buildingId, int category = -1)
        {
            //            string sql = @"select a.B_Id as busId,a.B_ServiceImg as coverImg,a.B_NickName as nickname from Core_Business as a
            //                            inner join Core_BusinessVillage as b
            //                            on a.B_Id = b.BV_BusinessId
            //                            where b.BV_VillageId = '"+buildingId+"'";

            using (DbContext db = new LinXinApp20Entities())
            {

                Expression<Func<Core_Business, bool>> exp = o => true;

                if (category > 0)
                {
                    exp = o => o.B_Category == category;
                }
                var list2 =
                    db.Set<Core_Business>()
                        .Where(o => o.B_AuditingState == (int)AuditingEnum.认证成功 && o.B_Status == 0).Where(exp)
                        .Join(db.Set<Core_BusinessVillage>(), b => b.B_Id, v => v.BV_BusinessId, (b, v) => new
                        {
                            busId = b.B_Id,
                            nickname = b.B_NickName,
                            coverImg = b.B_ServiceImg,
                            buildingId = v.BV_VillageId
                        }).Where(o => o.buildingId == buildingId).ToList();

                if (list2.Any())
                {
                    return list2;
                }

                //var cc = db.Database.SqlQuery<object>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();
                //  var rs =db.Database.SqlQuery<dynamic>(sql).ToList();
            }
            return null;
        }

        /// <summary>
        /// 获取服务商家 个数
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetBusinessServiceCountByBuildingId(Guid buildingId, int category = -1)
        {
            //            string sql = @"select a.B_Id as busId,a.B_ServiceImg as coverImg,a.B_NickName as nickname from Core_Business as a
            //                            inner join Core_BusinessVillage as b
            //                            on a.B_Id = b.BV_BusinessId
            //                            where b.BV_VillageId = '"+buildingId+"'";

            using (DbContext db = new LinXinApp20Entities())
            {

                Expression<Func<Core_Business, bool>> exp = o => true;

                if (category > 0)
                {
                    exp = o => o.B_Category == category;
                }
                return
                    db.Set<Core_Business>()
                        .Where(o => o.B_AuditingState == (int)AuditingEnum.认证成功 && o.B_Status == 0).Where(exp)
                        .Join(db.Set<Core_BusinessVillage>(), b => b.B_Id, v => v.BV_BusinessId, (b, v) => new { }).Count();

                //var cc = db.Database.SqlQuery<object>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();
                //  var rs =db.Database.SqlQuery<dynamic>(sql).ToList();
            }
            return 0;
        }


        #region 商家管理系统后台
        /// <summary>
        /// 编辑商家信息
        /// </summary>
        /// <param name="goods">商家信息</param>
        /// <param name="images"></param>
        /// <returns></returns>
        public bool EditBusInfo(Core_Business et, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Set<Core_Business>().Attach(et);
                db.Entry(et).State = EntityState.Modified;  //1修改商家基本信息


                //2 图片附件，如果修改，必须删除之前配置的图片
                if (images != null && images.Any())
                {
                    db.Sys_Attachment.Delete(o => o.A_PId == et.B_Id);  //批量删除，物理

                    DateTime dt = DateTime.Now;

                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.B_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = dt;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }


        /// <summary>
        /// 审核商家入驻
        /// </summary>
        /// <param name="proId"> 审核ID</param>
        /// <param name="category">商家社区服务类型</param>
        /// <param name="state">状态</param>
        /// <param name="tips">备注</param>
        /// <param name="buildings">分配的小区</param>
        /// <param name="busRemark">商家小区统计</param>
        /// <returns></returns>
        public ResultMessageViewModel CheckBusApplly(int proId, int category, int state, string tips, IEnumerable<dynamic> buildings, string busRemark)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var audApply = db.Core_BusinessEnteringApply.FirstOrDefault(o => o.BA_Id == proId);

                var bus =
                    db.Core_Business.FirstOrDefault(
                        o => o.B_Id == audApply.BA_BusId && o.B_Status == 0 && o.B_AuditingState == (int)AuditingEnum.认证中);

                if (bus != null && audApply != null)
                {

                    DateTime curTime = DateTime.Now;
                    if (state != (int)AuditingEnum.认证成功)   //审核失败
                    {
                        bus.B_AuditingState = (int)AuditingEnum.认证失败;  //审核失败
                        audApply.BA_CheckTips = tips;
                        audApply.BA_CheckTime = curTime;
                        audApply.BA_State = (int)AuditingEnum.认证失败;
                    }
                    else //审核成功
                    {
                        if (buildings != null && buildings.Any())
                        {
                            bus.B_VillageCount = buildings.Count();
                            bus.B_AuditingState = (int)AuditingEnum.认证成功;
                            bus.B_Category = category;

                            audApply.BA_CheckTips = tips;
                            audApply.BA_CheckTime = curTime;
                            audApply.BA_State = (int)AuditingEnum.认证成功;

                            //添加商家个人帐号
                            Core_Balance ban = new Core_Balance()
                            {
                                B_AccountId = bus.B_Id,
                                B_Balance = 0,
                                B_CouponMoney = 0,
                                B_ForzenMoney = 0,
                                B_EncryptCode = "",
                                B_PayPwd = "",
                                B_Role = (int)MemberRoleEnum.商家
                            };

                            db.Core_Balance.Add(ban);

                            //添加商家服务的社区 
                            var gp = buildings.GroupBy(o => o.BV_CityName).Select(o => new
                            {
                                cityname = o.Key,
                                count = o.Count()
                            });
                            //  string remark = "";

                            //foreach (var item in gp)
                            //{
                            //    remark += string.Format("{0}[{1}] ", item.cityname, item.count);
                            //}
                            bus.B_Remark = busRemark;  //商家分配的社区统计

                            foreach (var building in buildings)
                            {
                                Core_BusinessVillage bv = new Core_BusinessVillage()
                                {
                                    BV_BuildingName = building.BV_BuildingName,
                                    BV_BusinessId = bus.B_Id,
                                    BV_CityName = building.BV_CityName,
                                    BV_DistrictName = building.BV_DistrictName,
                                    BV_VillageId = building.BV_VillageId,
                                    BV_Date = curTime,
                                };

                                db.Core_BusinessVillage.Add(bv);
                            }


                        }
                        else
                        {
                            rs.Msg = "请选择商家服务的社区";
                            return rs;
                        }
                    }

                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new { busId = bus.B_Id };
                    }
                }
                else
                {
                    rs.Msg = "记录不存在";
                }
            }
            return rs;
        }


        /// <summary>
        /// 重新分配商家的服务社区
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="category"></param>
        /// <param name="buildings"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public bool EditBusBuilding(Guid busId, int category, IEnumerable<dynamic> buildings, string remark)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                var bus =
                    db.Core_Business.FirstOrDefault(
                        o => o.B_Id == busId);
                if (bus != null && buildings.Any())
                {
                    DateTime curTime = DateTime.Now;
                    if (!string.IsNullOrEmpty(remark) && buildings != null)
                    {
                        db.Core_BusinessVillage.Delete(o => o.BV_BusinessId == busId);  //删除商家之前分配的社区
                        //重新分配社区
                        foreach (var building in buildings)
                        {
                            Core_BusinessVillage bv = new Core_BusinessVillage()
                            {
                                BV_BuildingName = building.BV_BuildingName,
                                BV_BusinessId = busId,
                                BV_CityName = building.BV_CityName,
                                BV_DistrictName = building.BV_DistrictName,
                                BV_VillageId = building.BV_VillageId,
                                BV_Date = curTime,
                            };


                            db.Core_BusinessVillage.Add(bv);
                        }

                        bus.B_Remark = remark;  //商家分配的社区统计
                    }
                    if (category > 0)
                    {
                        bus.B_Category = category;
                    }
                    return db.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        #endregion
    }
}
