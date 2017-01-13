using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Microsoft.Win32.SafeHandles;

namespace JiGuangLinXin.App.Core
{
    public class BuildingCore : BaseRepository<Core_Building>
    {

        /// <summary>
        /// 新增楼盘
        /// </summary>
        /// <param name="info"></param>
        /// <param name="images"></param>
        /// <param name="cube"></param>
        /// <returns></returns>
        public bool Add(Core_Building info, IEnumerable<dynamic> images, IEnumerable<dynamic> cube)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //1添加全景记录
                foreach (var o in cube)
                {
                    Core_BuidingCube cb = new Core_BuidingCube();
                    cb.BC_BuildingId = info.B_Id;
                    cb.BC_CoverImg = o.coverImg;
                    cb.BC_Rank = o.rank;
                    cb.BC_Status = 0;
                    cb.BC_Title = o.title;
                    cb.BC_Time = info.B_Time;
                    cb.BC_Id = Guid.NewGuid();
                    db.Core_BuidingCube.Add(cb);  //添加户型

                    //户型图片
                    IEnumerable<dynamic> temp = o.images;
                    if (temp.Count() == 6)
                    {
                        foreach (var img in o.images)
                        {
                            Sys_Attachment att = new Sys_Attachment();
                            att.A_Id = Guid.NewGuid();
                            att.A_PId = cb.BC_Id;
                            att.A_FileName = img.A_FileName;
                            att.A_FileNameOld = img.A_FileNameOld;
                            att.A_Folder = img.A_Folder;
                            att.A_Size = img.A_Size;
                            att.A_Rank = img.A_Rank;
                            att.A_Time = cb.BC_Time;
                            db.Sys_Attachment.Add(att);  //添加户型的图片
                        }
                    }
                }


                //2添加楼盘图片
                foreach (var img in images)
                {
                    Sys_Attachment att = new Sys_Attachment();
                    att.A_Id = Guid.NewGuid();
                    att.A_PId = info.B_Id;
                    att.A_FileName = img.A_FileName;
                    att.A_FileNameOld = img.A_FileNameOld;
                    att.A_Folder = img.A_Folder;
                    att.A_Size = img.A_Size;
                    att.A_Rank = img.A_Rank;
                    att.A_Time = info.B_Time;

                    db.Sys_Attachment.Add(att);  //添加楼盘的图片
                }

                //3添加楼盘信息
                db.Core_Building.Add(info);

                //4由于发了楼盘红包，需要添加平台账单
                if (info.B_HongbaoCount > 0 && info.B_HongbaoMoney > 0)
                {

                    //5平台流水
                    Sys_BillMaster billMaster = new Sys_BillMaster()
                    {
                        B_Flag = (int)BillFlagEnum.平台流水,
                        B_Module = (int)BillEnum.楼盘红包,
                        B_Money = info.B_HongbaoCount * info.B_HongbaoCount,
                        B_OrderId = info.B_Id,
                        B_Phone = info.B_BusPhone,
                        B_Remark = "",
                        B_Status = 0,
                        B_Time = info.B_Time,
                        B_Title = info.B_HongbaoTips,
                        B_UId = info.B_AdminId,
                        B_Type = (int)MemberRoleEnum.商家
                    };
                    db.Sys_BillMaster.Add(billMaster);
                }



                return db.SaveChanges() > 0;//批量提交
            }
            return false;
        }

        /// <summary>
        /// 用户报名申请
        /// </summary>
        /// <param name="bId">楼盘ID</param>
        /// <param name="uId">用户ID</param>
        /// <returns></returns>
        public ResultMessageViewModel Apply(Guid bId, Guid uId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //用户是否存在
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uId && o.U_Status != (int)UserStatusEnum.冻结);
                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return rs;
                }
                //楼盘是否存在
                var building = db.Core_Building.FirstOrDefault(o => o.B_Id == bId && o.B_Status == 0);
                if (building == null)
                {
                    rs.Msg = "楼盘活动不存在";
                    return rs;
                }
                if (building.B_BTime < DateTime.Now && building.B_ETime > DateTime.Now)
                {
                    rs.Msg = "楼盘活动报名已经结束";
                    return rs;
                }
                var his = db.Core_ActivityApply.Where(o => o.AA_UId == user.U_Id && o.AA_ActivityId == building.B_Id);
                if (his.Any())
                {
                    rs.Msg = "您已经参加，请勿重复报名";
                    return rs;
                }


                Core_ActivityApply apply = new Core_ActivityApply()
                {
                    AA_ActivityId = building.B_Id,
                    AA_Status = 0,
                    AA_Time = DateTime.Now,
                    AA_UId = user.U_Id,
                    AA_UPhone = user.U_LoginPhone,
                    AA_Remark = user.U_TrueName + "#" + user.U_BuildingName
                };
                db.Core_ActivityApply.Add(apply);
                if (db.SaveChanges() > 0)
                {
                    rs.Msg = "ok";
                    rs.State = 0;
                }

            }
            return rs;
        }


        /// <summary>
        /// 楼盘活动，有机会拆红包哦
        /// </summary>
        /// <param name="bId"></param>
        /// <param name="uId"></param>
        /// <returns></returns>
        public ResultMessageViewModel Activity(Guid bId, Guid uId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                //楼盘是否存在
                var building = db.Core_Building.FirstOrDefault(o => o.B_Id == bId && o.B_Status == 0);
                if (building == null)
                {
                    rs.Msg = "楼盘活动不存在";
                    return rs;
                }
                //用户是否存在
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uId && o.U_Status != (int)UserStatusEnum.冻结);
                if (user == null)
                {

                    rs.Msg = "ok";
                    rs.State = 0;


                    rs.Data = new
                    {
                        bId = building.B_Id,
                        title = building.B_ActivityTitle,
                        stime = building.B_BTime,
                        etime = building.B_ETime,
                        coverImg = building.B_ActivityImg,
                        address = building.B_Address,
                        content = building.B_ActivityContent,
                        buildingName = building.B_Name,
                        logo = building.B_Logo,

                        hbFlag = 0,//0未领取红包 1本次领取红包 2之前领取到红包
                        hbMoney = 0,//红包金额,

                        uid = "",
                        uphone = "",
                        unickname = ""
                    };

                    return rs;
                }
                //if (building.B_BTime < DateTime.Now && building.B_ETime > DateTime.Now)
                //{
                //    rs.Msg = "楼盘活动报名已经结束";
                //    return rs;
                //}
                //var his = db.Core_ActivityApply.Where(o => o.AA_UId == user.U_Id && o.AA_ActivityId == building.B_Id);
                //if (his.Any())
                //{
                //    rs.Msg = "您已经参加，请勿重复报名";
                //    return rs;
                //}

                DateTime dt = DateTime.Now;
                decimal hbMoney = 0;
                int hbFlag = 0;

                //查看是否已经领取
                var isReceive =
                    db.Core_LuckyGiftHistory.FirstOrDefault(o => o.LH_UserId == user.U_Id && o.LH_GiftId == building.B_Id);
                if (isReceive != null)  //已领取
                {
                    hbMoney = isReceive.LH_Money;
                    hbFlag = 2;
                }
                else if (building.B_HongbaoRemain > 0) //没有领取，还有红包未被领取
                {

                    building.B_HongbaoRemain -= 1;  //红包数量递减

                    //添加用户领红包记录
                    Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                    {
                        LH_CreateTime = dt,
                        LH_GiftDetailId = null,
                        LH_GiftId = building.B_Id,
                        LH_Id = Guid.NewGuid(),
                        LH_Money = building.B_HongbaoMoney,
                        LH_Remark = string.Format("领取楼盘【{0}-{1}】-“{2}” 元活动红包", building.B_Name, building.B_ActivityTitle, building.B_HongbaoMoney.ToString("N")),
                        LH_Status = 0,
                        LH_UserId = user.U_Id,
                        LH_UserNickName = user.U_NickName,
                        LH_UserPhone = user.U_LoginPhone,
                        LH_Flag = (int)LuckGiftTypeEnum.新家推荐红包
                    };
                    db.Core_LuckyGiftHistory.Add(history);

                    //添加账单记录
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Module = (int)BillEnum.楼盘红包,
                        B_Money = building.B_HongbaoMoney,
                        B_OrderId = history.LH_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Remark = history.LH_Remark,
                        B_Status = 0,
                        B_Time = dt,
                        B_Title = building.B_HongbaoTips,
                        B_UId = user.U_Id,
                        B_Type = (int)MemberRoleEnum.会员
                    };
                    db.Core_BillMember.Add(bill);

                    //累计用户余额
                    var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                    balance.B_Balance += building.B_HongbaoMoney;


                    hbMoney = building.B_HongbaoMoney;
                    hbFlag = 1;
                }
                building.B_Clicks += 1;  //累计浏览量
                if (db.SaveChanges() > 0)
                {

                    rs.Msg = "ok";
                    rs.State = hbFlag;


                    rs.Data = new
                    {
                        bId = building.B_Id,
                        title = building.B_ActivityTitle,
                        stime = building.B_BTime,
                        etime = building.B_ETime,
                        coverImg = building.B_ActivityImg,
                        address = building.B_Address,
                        content = building.B_ActivityContent,
                        buildingName = building.B_Name,
                        logo = building.B_Logo,

                        hbFlag,//0未领取红包 1本次领取红包 2之前领取到红包
                        hbMoney,//红包金额,

                        uid = user.U_Id,
                        uphone = user.U_LoginPhone,
                        unickname = user.U_NickName
                    };
                }

            }
            return rs;
        }

    }
}
