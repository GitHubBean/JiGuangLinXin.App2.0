using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class GroupbuyCore : BaseRepository<Core_GroupBuy>
    {
        /// <summary>
        /// 获得用户的邻里团
        /// </summary>
        /// <param name="buidingId">小区ID</param>
        /// <param name="whereStr">where 筛选</param>
        /// <param name="odStr">排序</param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public List<GroupbuyViewModel> GetGroupbuyByVilliageId(Guid buidingId, int orderBy, int typeId, int pn, int rows)
        {
            string odStr = "";
            string whereStr = " and 1=1";
            if (orderBy == 1) //销量
            {
                odStr = ",a.GB_Sales desc";
            }
            else if (orderBy == 2) //价格
            {
                odStr = ",a.GB_Price desc";
            }
            if (typeId > 0)
            {
                whereStr = " and a.GB_TypeId=" + typeId;
            }

            string sql =
                string.Format(
                    @"select top {1} * from (
                            SELECT row_number() over(order by a.GB_Top desc {4}) as rownumber,  a.GB_Id,a.GB_Top,a.GB_PeopleCount,a.GB_Titlte,a.GB_CoverImg,a.GB_Price,a.GB_Sales,a.GB_PriceOld,a.GB_BusId,a.GB_BusName,a.GB_STime,a.GB_ETime,a.GB_Time from Core_GroupBuy  a
                            inner join Core_GroupBuyScope b
                            on a.GB_Id=b.GS_GId
                            where  a.GB_State=0 and a.GB_AuditingState=1 and a.GB_STime<GETDATE() and a.GB_ETime>GETDATE() and b.GS_BuildingId='{0}' {3} ) temp
                             where rownumber > {2}", buidingId, rows, pn * rows, whereStr, odStr);



            /*
             
             
             
            string sql =
                string.Format(
                    @"select top {1} * from (
                            SELECT row_number() over(order by a.GB_Top desc {4}) as rownumber,  a.GB_Id,a.GB_Top,a.GB_PeopleCount,a.GB_Titlte,a.GB_CoverImg,a.GB_Price,a.GB_PriceOld,a.GB_BusId,a.GB_BusName,a.GB_STime,a.GB_ETime,a.GB_Time from Core_GroupBuy  a
        inner join Core_BusinessVillage b
        on a.GB_BusId=b.BV_BusinessId
        where  a.GB_State=0 and a.GB_AuditingState=1 and a.GB_STime<GETDATE() and a.GB_ETime>GETDATE() and b.BV_VillageId='{0}' {3} ) temp
                             where rownumber > {2}", buidingId, rows, pn * rows, whereStr, odStr);
             
              
             */




            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var list1 = db.Database.SqlQuery<GroupbuyViewModel>(sql).ToList(); //发布到小区的邻里团

                var temp = //发布到全平台的邻里团
                    db.Core_GroupBuy.Where(o => o.GB_Target == 0 && o.GB_State == 0 && o.GB_STime < DateTime.Now && o.GB_ETime > DateTime.Now);


                if (typeId > 0)  //分类查询
                {
                    temp = temp.Where(o => o.GB_TypeId == typeId);
                }
                var list0 = temp.OrderByDescending(o => o.GB_Time).Skip(pn * rows).Take(rows)
                        .Select(o => new GroupbuyViewModel()
                        {
                            GB_BusId = o.GB_BusId,
                            GB_BusName = o.GB_BusName,
                            GB_CoverImg = o.GB_CoverImg,
                            GB_ETime = o.GB_ETime,
                            GB_Id = o.GB_Id,
                            GB_PeopleCount = o.GB_PeopleCount,
                            GB_Price = o.GB_Price,
                            GB_PriceOld = o.GB_PriceOld,
                            GB_STime = o.GB_STime,
                            GB_Time = o.GB_Time,
                            GB_Titlte = o.GB_Titlte,
                            GB_Top = o.GB_Top,
                            GB_Sales = o.GB_State
                        }).ToList();

                var rsList = list1.Union(list0).OrderBy(o => o.GB_Top).ThenBy(o => o.GB_Time).ToList();
                if (orderBy == 1) //销量
                {
                    rsList = rsList.OrderByDescending(o => o.GB_Sales).ToList();
                    //odStr = ",a.GB_Sales desc";
                }
                else if (orderBy == 2) //价格
                {
                    rsList = rsList.OrderBy(o => o.GB_Price).ToList();
                    //odStr = ",a.GB_Price desc";
                }

                return rsList;
            }
        }

        /// <summary>
        /// 查询社区 邻里团 数量
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public int GetGroupBuyCountByBuildingId(Guid buildingId)
        {
            string sql = @"SELECT COUNT(1) from Core_GroupBuy  a  
                           inner join Core_GroupBuyScope b
                            on a.GB_Id=b.GS_GId
                            where  a.GB_State=0 and a.GB_AuditingState=1 and a.GB_STime<GETDATE() and a.GB_ETime>GETDATE() and b.GS_BuildingId='{0}' ";

            /**
             
               string sql = @"SELECT COUNT(1) from Core_GroupBuy  a  
                            inner join Core_BusinessVillage b
                            on a.GB_BusId=b.BV_BusinessId
                            where  a.GB_State=0 and a.GB_AuditingState=1 and a.GB_STime<GETDATE() and a.GB_ETime>GETDATE() and b.GS_BuildingId='{0}' ";

             * **/

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                int count1 = db.Database.SqlQuery<int>(string.Format(sql, buildingId)).FirstOrDefault();
                int count0 = db.Core_GroupBuy.Count(o => o.GB_Target == 0 && o.GB_State == 0);
                return count1 + count0;
            }

            return 0;
        }

        /// <summary>
        /// 邻里团下单
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="proId">邻里团ID</param>
        /// <param name="ownerCardMoney">是否使用业主卡（-1 不实用 0使用）</param>
        /// <param name="addressId">收货地址</param>
        /// <param name="enPaypwd">支付密码（加密）</param>
        /// <param name="tips">订单备注</param>
        /// <returns></returns>
        public ResultMessageViewModel Order(Guid uid, Guid proId, decimal ownerCardMoney, int addressId, string enPaypwd, string tips)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结);  //用户信息

                if (user == null)
                {
                    rs.Msg = "用户不存在";
                    return rs;
                }
                var pro =
                    db.Core_GroupBuy.FirstOrDefault(
                        o =>
                            o.GB_Id == proId && o.GB_State == (int)GroupbuyStateEnum.团购中 && o.GB_AuditingState == (int)AuditingEnum.认证成功 && o.GB_STime < DateTime.Now &&
                            o.GB_ETime > DateTime.Now);  //邻里团信息

                //var busBuilding =
                //    db.view_businessService.FirstOrDefault(
                //        o => o.buildingId == user.U_BuildingId && o.busId == pro.GB_BusId);


                if (pro == null)
                {
                    rs.Msg = "信息不存在";
                    return rs;
                }
                //发布到社区
                if (pro.GB_Target == 1)
                {
                    //邻里团分区域
                    var busBuilding =
                        db.Core_GroupBuyScope.FirstOrDefault(
                            o => o.GS_BuildingId == user.U_BuildingId && o.GS_GId == pro.GB_Id);
                    if (busBuilding == null)
                    {

                        rs.Msg = "信息不存在";
                        return rs;
                    }
                }


                var proBuilding =
                    db.Core_GroupBuyVillage.FirstOrDefault(
                        o =>
                            o.VB_BuildingId == user.U_BuildingId && o.VB_GroupBuyId == pro.GB_Id);  //当前社区是否开团了

                if (proBuilding != null)  //开团了就看看自己是否已经参团、或者已经满团
                {
                    if (proBuilding.VB_JoinCount >= pro.GB_PeopleCount)
                    {
                        rs.Msg = "已满团";
                        return rs;
                    }
                    var isjoin =
                        db.Core_GroupBuyOrder.FirstOrDefault(o => o.BO_UId == user.U_Id && o.BO_BuildignId == user.U_BuildingId && o.BO_GroupBuyId == pro.GB_Id);
                    if (isjoin != null)
                    {
                        rs.Msg = "您已参团请勿重复提交";
                        return rs;
                    }
                }
                else  //没开团就添加自己小区的团购
                {
                    proBuilding = new Core_GroupBuyVillage()
                    {
                        VB_BuildignName = user.U_BuildingName,
                        VB_BuildingId = user.U_BuildingId,
                        VB_GroupBuyId = pro.GB_Id,
                        VB_Id = Guid.NewGuid(),
                        VB_JoinCount = 0,
                        VB_PeopleCount = pro.GB_PeopleCount,
                        VB_State = (int)GroupbuyStateEnum.团购中,
                        VB_ETime = pro.GB_ETime,
                        VB_STime = pro.GB_STime,
                        VB_Time = DateTime.Now
                    };
                    //添加社区团购信息
                    db.Core_GroupBuyVillage.Add(proBuilding);
                }

                var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                {
                    rs.State = 2;
                    rs.Msg = "您还未设置支付密码,请立即前去设置！";
                    return rs;
                }
                if (balance.B_PayPwd.Equals(Md5Extensions.MD5Encrypt(DESProvider.DecryptString(enPaypwd) + balance.B_EncryptCode)))
                {

                    decimal payMoney = pro.GB_Price + pro.GB_ExtraFee;//实际应付金额
                    Guid orderId = Guid.NewGuid();
                    if (ownerCardMoney > -1)  //使用业主卡
                    {
                        if (balance.B_CouponMoney >= pro.GB_Price)  //全部用业主卡支付
                        {
                            ownerCardMoney = payMoney;  //实际支出的业主卡金额
                            balance.B_CouponMoney -= payMoney;
                            payMoney = 0;
                        }
                        else
                        {
                            ownerCardMoney = balance.B_CouponMoney;
                            payMoney = payMoney - balance.B_CouponMoney;  //使用业主卡部分支付
                            balance.B_CouponMoney = 0;
                        }

                        // 添加对应的 业主卡消费 记录
                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "使用平台业主卡消费-邻里团",
                            B_Money = -ownerCardMoney,
                            B_UId = user.U_Id,
                            B_Phone = user.U_LoginPhone,
                            B_Module = (int)BillEnum.平台业主卡,
                            B_OrderId = orderId,
                            B_Type = (int)MemberRoleEnum.会员,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0
                        };
                        db.Core_BillMember.Add(bill);

                    }
                    else//不使用业主卡
                    {
                        ownerCardMoney = 0;
                    }
                    if (balance.B_Balance < payMoney)
                    {
                        rs.State = 3;
                        rs.Msg = "用户帐号余额不足,请及时充值！";
                        return rs;
                    }

                    //减少用户的可用余额
                    balance.B_Balance -= payMoney;

                    //添加用户订单的 消费 记录
                    Core_BillMember billOrder = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = "邻里团购物消费",
                        B_Money = -payMoney,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.邻里团,
                        B_OrderId = orderId,
                        B_Type = (int)MemberRoleEnum.会员,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0
                    };

                    db.Core_BillMember.Add(billOrder);

                    //收货地址
                    var address =
                        db.Set<Core_DeliveryAddress>().FirstOrDefault(o => o.A_Id == addressId && o.A_UserId == uid);

                    Core_GroupBuyOrder order = new Core_GroupBuyOrder()
                    {
                        BO_BuildignId = user.U_BuildingId,
                        BO_BusId = pro.GB_BusId,
                        BO_BusName = pro.GB_BusName,
                        BO_BusPhone = pro.GB_BusPhone,
                        BO_CoverImg = pro.GB_CoverImg,
                        BO_ETime = pro.GB_ETime,
                        BO_GoodsCount = 1,
                        BO_GroupBuyId = pro.GB_Id,
                        BO_Id = orderId,
                        BO_OrderNo = DateTime.Now.ToString("yyyyMMddHHmmssfff"),


                        BO_OrderState = (int)GroupBuyOrderStateEnum.待发货,
                        BO_PayState = (int)PayStateEnum.已付款,

                        BO_OwnerCardMoney = ownerCardMoney,
                        BO_PeopleCount = pro.GB_PeopleCount,
                        BO_Price = pro.GB_Price + pro.GB_ExtraFee,
                        BO_Remark = tips,
                        BO_STime = pro.GB_STime,
                        BO_TargetAddress = address.A_Address,
                        BO_TargetName = address.A_UserName,
                        BO_TargetPhone = address.A_Phone,
                        BO_Time = DateTime.Now,
                        BO_Titlte = pro.GB_Titlte,
                        BO_UId = user.U_Id,
                        BO_ULogo = user.U_Logo,
                        BO_UPhone = user.U_LoginPhone
                    };
                    //添加Order
                    db.Core_GroupBuyOrder.Add(order);

                    //添加 商家收入 记录
                    Core_BillMember billBus = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = string.Format("邻里团收入。订单号：【{0}】；", order.BO_OrderNo),// 
                        B_Money = pro.GB_Price + pro.GB_ExtraFee,
                        B_UId = order.BO_BusId,
                        B_Phone = order.BO_BusPhone,
                        B_Module = (int)BillEnum.邻里团,
                        B_OrderId = order.BO_Id,
                        B_Type = (int)MemberRoleEnum.商家,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0,
                        B_Remark = tips
                    };
                    db.Core_BillMember.Add(billBus); //添加商家账单

                    // 增加商家可用余额
                    var balanceBus = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == pro.GB_BusId);
                    balanceBus.B_Balance += payMoney;

                    //累计邻里团的销量
                    pro.GB_Sales += 1;

                    //累计本社区参团人数
                    proBuilding.VB_JoinCount += 1;

                    //单元提交
                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new
                        {
                            orderId = order.BO_Id,
                            orderNo = order.BO_OrderNo,
                            orderPrice = order.BO_Price.ToString("F2")
                        };
                    }
                }
                else
                {
                    rs.Msg = "支付密码错误";
                    return rs;
                }

            }
            return rs;
        }



        /// <summary>
        /// 编辑邻里团
        /// </summary>
        /// <param name="info">邻里团信息</param>
        /// <param name="images">邻里团图片</param>
        /// <param name="buildings">邻里团发布的小区</param>
        /// <param name="isAdd">添加/修改</param>
        /// <returns></returns>
        public ResultMessageViewModel Edit(Core_GroupBuy info, IEnumerable<dynamic> images, IEnumerable<dynamic> buildings, bool isAdd = true)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {


                if (buildings != null && buildings.Any())
                {
                    foreach (var building in buildings)
                    {
                        Core_GroupBuyScope bv = new Core_GroupBuyScope()
                        {
                            GS_Id = Guid.NewGuid(),
                            GS_BuildingName = building.BV_BuildingName,
                            GS_GId = info.GB_Id,
                            GS_CityName = building.BV_CityName,
                            GS_DistrictName = building.BV_DistrictName,
                            GS_BuildingId = building.BV_VillageId,
                            GS_Time = info.GB_Time,
                        };

                        db.Core_GroupBuyScope.Add(bv);
                    }
                }

                if (isAdd)
                {
                    db.Core_GroupBuy.Add(info);  //添加邻里团 基本信息
                }
                else  //修改
                {
                    db.Set<Core_GroupBuy>().Attach(info);
                    db.Entry(info).State = EntityState.Modified;  // 修改邻里团基本信息

                    if (images != null)
                    {
                        db.Sys_Attachment.Delete(o => o.A_PId == info.GB_Id);  //批量删除，物理
                    }
                    if (buildings != null)
                    {
                        db.Core_GroupBuyScope.Delete(o => o.GS_GId == info.GB_Id);  //删除之前分配的社区
                    }
                }

                if (images != null && images.Any())
                {
                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = info.GB_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = info.GB_Time;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);  //添加附件
                    }
                }

                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            return rs;
        }
    }

    public class GroupbuyOrderCore : BaseRepository<Core_GroupBuyOrder>
    {
        /// <summary>
        /// 批量发货
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public ResultMessageViewModel Deliver(Guid proId, Guid busId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //看看邻里团是否开团成功
                var buyVill = db.Core_GroupBuyVillage.FirstOrDefault(o => o.VB_Id == proId);

                var buy = db.Core_GroupBuy.FirstOrDefault(o => o.GB_Id == buyVill.VB_GroupBuyId && busId == o.GB_BusId);

                var buyOrder =
                    db.Core_GroupBuyOrder.Where(
                        o => o.BO_GroupBuyId == buy.GB_Id && o.BO_BuildignId == buyVill.VB_BuildingId);

                //社区参团的订单与邻里团设置的数量相等，标识开团成功
                if (buy.GB_PeopleCount == buyOrder.Count())
                {
                    buyVill.VB_State = (int)GroupbuyStateEnum.团购成功;

                    foreach (var order in buyOrder)
                    {
                        order.BO_OrderState = (int)GroupBuyOrderStateEnum.已完成; //(int)OrderStateEnum.已发货;  商家发货标识结束，跳过用户确认收货
                    }

                    //单元提交
                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }

            }
            return rs;
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="proId">退款的社区记录ID</param>
        /// <param name="busId"></param>
        /// <returns></returns>
        public ResultMessageViewModel BackMoney(Guid proId, Guid busId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //看看邻里团是否开团成功
                var buyVill = db.Core_GroupBuyVillage.FirstOrDefault(o => o.VB_Id == proId);

                var buy = db.Core_GroupBuy.FirstOrDefault(o => o.GB_Id == buyVill.VB_GroupBuyId && busId == o.GB_BusId);

                var buyOrder =
                    db.Core_GroupBuyOrder.Where(
                        o => o.BO_GroupBuyId == buy.GB_Id && o.BO_BuildignId == buyVill.VB_BuildingId);

                //社区参团的订单如果已经过期并且，状态为 团购中（意思就是还没有发货、或者还没有满团）才有资格退款
                if (buy.GB_ETime < DateTime.Now && buyVill.VB_State == (int)GroupbuyStateEnum.团购中)
                {
                    buyVill.VB_State = (int)GroupbuyStateEnum.团购失败;

                    decimal backMoney = 0;
                    foreach (var order in buyOrder)
                    {
                        order.BO_OrderState = (int)GroupBuyOrderStateEnum.已退款;
                        backMoney += buy.GB_Price + buy.GB_ExtraFee;  //应该退金额


                        //添加 用户退款收入 记录
                        //Core_BillMember billUser = new Core_BillMember()
                        //{
                        //    B_Time = DateTime.Now,
                        //    B_Title = string.Format("邻里团组团失败，商家退款。订单号：【{0}】；", order.BO_OrderNo),// 
                        //    B_Money = order.BO_Price,// buy.GB_Price + buy.GB_ExtraFee,
                        //    B_UId = order.BO_UId,
                        //    B_Phone = order.BO_BusPhone,
                        //    B_Module = (int)BillEnum.邻里团,
                        //    B_OrderId = order.BO_Id,
                        //    B_Type = (int)MemberRoleEnum.会员,
                        //    B_Flag = (int)BillFlagEnum.普通流水,
                        //    B_Status = 0,
                        //    B_Remark = string.Format("业主卡：{}")
                        //};

                        var banlance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == order.BO_UId);
                        if (order.BO_OwnerCardMoney > 0)  //退业主卡
                        {
                            banlance.B_CouponMoney += order.BO_OwnerCardMoney;

                            //添加 用户退款收入 记录
                            Core_BillMember billUser = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Title = string.Format("邻里团组团失败，商家退业主卡。订单号：【{0}】；", order.BO_OrderNo),// 
                                B_Money = order.BO_OwnerCardMoney,// buy.GB_Price + buy.GB_ExtraFee,
                                B_UId = order.BO_UId,
                                B_Phone = order.BO_BusPhone,
                                B_Module = (int)BillEnum.平台业主卡,
                                B_OrderId = order.BO_Id,
                                B_Type = (int)MemberRoleEnum.会员,
                                B_Flag = (int)BillFlagEnum.普通流水,
                                B_Status = 0
                            };
                            db.Core_BillMember.Add(billUser);

                        }
                        var banPrice = order.BO_Price - order.BO_OwnerCardMoney;
                        if (banPrice > 0)
                        {
                            //退余额
                            banlance.B_Balance += banPrice;
                            //添加 用户退款收入 记录
                            Core_BillMember billUser1 = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Title = string.Format("邻里团组团失败，商家退余额。订单号：【{0}】；", order.BO_OrderNo),// 
                                B_Money = banPrice,// buy.GB_Price + buy.GB_ExtraFee,
                                B_UId = order.BO_UId,
                                B_Phone = order.BO_BusPhone,
                                B_Module = (int)BillEnum.邻里团退款,
                                B_OrderId = order.BO_Id,
                                B_Type = (int)MemberRoleEnum.会员,
                                B_Flag = (int)BillFlagEnum.普通流水,
                                B_Status = 0
                            };
                            db.Core_BillMember.Add(billUser1);
                        }

                    }
                    //添加 商家 退款支出 记录
                    Core_BillMember billBus = new Core_BillMember()
                    {
                        B_Time = DateTime.Now,
                        B_Title = string.Format("邻里团组团失败，商家退款。社区：【{0}】；", buyVill.VB_BuildignName),// 
                        B_Money = -backMoney,
                        B_UId = buy.GB_BusId,
                        B_Phone = buy.GB_BusName,
                        B_Module = (int)BillEnum.邻里团,
                        B_OrderId = buyVill.VB_Id,
                        B_Type = (int)MemberRoleEnum.商家,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0,
                    };
                    db.Core_BillMember.Add(billBus);


                    //单元提交
                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new
                        {
                            money = backMoney
                        };
                    }
                }
                else
                {
                    rs.Msg = "不符合退款条件";
                }

            }
            return rs;
        }
    }
    public class GroupbuyBuildingCore : BaseRepository<Core_GroupBuyVillage>
    {
    }
    public class GroupbuyTypeCore : BaseRepository<Core_GroupBuyType>
    {
    }
    public class GroupbuyScopeCore : BaseRepository<Core_GroupBuyScope>
    {
    }
}
