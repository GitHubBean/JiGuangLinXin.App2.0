using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Dynamic;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;

using System.Linq;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.StringHelper;

namespace JiGuangLinXin.App.Core
{
    public class MallOrderCore : BaseRepository<Core_MallOrder>
    {


        /// <summary>
        /// 确认下单
        /// </summary>
        /// <param name="uid">会员ID</param>
        /// <param name="phone">会员电话</param>
        /// <param name="ids">所有提交过来的商品ID、购物车集合，逗号分割</param>
        /// <param name="flag">0直接购买 1购物车跳转</param>
        /// <param name="count">购买的数量</param>
        /// <returns></returns>
        public ResultMessageViewModel OrderConfirm(Guid uid, string phone, string ids, int flag, int count)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            if (!string.IsNullOrEmpty(ids))
            {


                using (DbContext db = new LinXinApp20Entities())
                {
                    DateTime dt = DateTime.Now;
                    if (flag == 0)  //直接购买
                    {
                        //var list =mgCore.LoadEntities(o => ids.Contains(o.G_Id.ToString())).OrderByDescending(o => o.G_Time).ToList();

                        //if (list.Any(p => p.G_Status != 0))  //有过期的商品
                        //{
                        //    rs.Msg = "选择的商品有的已下架，请重新选择";
                        //    return WebApiJsonResult.ToJson(rs);
                        //}
                        //var gp = list.Select(s => new
                        //{
                        //    busId = s.G_BusId,
                        //    busName = s.G_BusName,
                        //    busList = new
                        //    {
                        //        busId = s.G_BusId,
                        //        busName = s.G_BusName,
                        //        imgUrl = StaticHttpUrl + s.G_Img,
                        //        goodsId = s.G_Id,
                        //        goodsName = s.G_Name,
                        //        goodsCount = count,
                        //        price = s.G_Price,
                        //        fee = s.G_ExtraFee,
                        //        state = s.G_Status
                        //    }
                        //});

                        Guid gid = Guid.Parse(ids);  //只传一个ID过来
                        var info = db.Set<Core_MallGoods>().FirstOrDefault(p => p.G_Id == gid);

                        if (info.G_Status != 0)  //过期的商品
                        {
                            rs.Msg = "选择的商品有的已下架，请重新选择";
                            return rs;
                        }


                        #region 提交订单

                        Core_MallOrder order = new Core_MallOrder()
                        {
                            GO_BusId = info.G_BusId,
                            GO_BusName = info.G_BusName,
                            GO_BusPhone = info.G_BusPhone,
                            GO_GoodsCount = count,
                            GO_Id = Guid.NewGuid(),
                            GO_OrderMoney = info.G_Price * count,
                            GO_PayState = (int)PayStateEnum.未付款,
                            GO_OrderState = (int)OrderStateEnum.待确认,
                            GO_Remark = info.G_ExtraFee > 0 ? "配送费：" + info.G_ExtraFee : "",
                            GO_Time = dt,
                            GO_OrderNo = dt.ToString("yyyyMMddHHmmssfff") + new CreateRandomStr().GetRandomString(6), //;Guid.NewGuid().ToString("N"),
                            GO_UId = uid,
                            GO_UPhone = phone
                        };

                        Core_MallOrderDetail orderDetail = new Core_MallOrderDetail()
                        {
                            OD_BusId = order.GO_BusId,
                            OD_BusName = order.GO_BusName,
                            OD_BusPhone = order.GO_BusPhone,
                            OD_Count = count,
                            OD_GoodsId = info.G_Id,
                            OD_Id = Guid.NewGuid(),
                            OD_Img = info.G_Img,
                            OD_Name = info.G_Name,
                            OD_OrderId = order.GO_OrderNo,
                            OD_Price = info.G_Price,
                            OD_Time = order.GO_Time,
                            OD_UId = uid,
                            OD_UPhone = phone
                        };

                        db.Entry<Core_MallOrder>(order).State = EntityState.Added;//添加订单
                        db.Entry<Core_MallOrderDetail>(orderDetail).State = EntityState.Added;  //添加订单详情
                        #endregion

                        if (db.SaveChanges() > 0)
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                            rs.Data = new
                            {
                                orderGoods = new List<dynamic>()
                                {
                                    new
                                    {
                                        busId = info.G_BusId,
                                        busName = info.G_BusName,
                                        busList = new List<dynamic>()
                                        {
                                            new
                                            {
                                                busId = info.G_BusId,
                                                busName = info.G_BusName,
                                                imgUrl = info.G_Img,
                                                goodsId = info.G_Id,
                                                goodsName = info.G_Name,
                                                goodsCount = count,
                                                price = info.G_Price,
                                                fee = info.G_ExtraFee,
                                                state = info.G_Status
                                            }
                                        },
                                        orderId = order.GO_Id,
                                        orderNo = order.GO_OrderNo,

                                        expFee = info.G_ExtraFee,
                                        deliver = "",
                                        card = ""
                                    }
                                },

                            };
                        }
                        else
                        {
                            rs.Msg = "用户精品汇下单失败";
                        }
                    }
                    else   //购物车提交
                    {
                        var idsArr = ids.Split(',');
                        var list = db.Set<view_shoppingCar>().Where(o => idsArr.Any(d => d == o.S_Id.ToString())).OrderByDescending(o => o.S_Time).ToList();

                        if (list.Any(p => p.G_Status != 0))  //有过期的商品
                        {
                            rs.Msg = "选择的商品有的已下架，请重新选择";
                            return rs;
                        }

                        #region 提交订单

                        var gpBus = list.GroupBy(o => new { o.S_BusId, o.B_NickName, o.B_Phone });

                        List<Core_MallOrder> oList = new List<Core_MallOrder>();
                        List<Core_MallOrderDetail> odList = new List<Core_MallOrderDetail>();
                        foreach (var item in gpBus)
                        {
                            //配送费
                            decimal exFee =
                                list.Where(o => o.S_BusId == item.Key.S_BusId).Max(o => o.G_ExtraFee);

                            Core_MallOrder od = new Core_MallOrder()
                            {

                                GO_BusId = item.Key.S_BusId,
                                GO_BusName = item.Key.B_NickName,
                                GO_BusPhone = item.Key.B_Phone,
                                GO_GoodsCount = list.Where(o => o.S_BusId == item.Key.S_BusId).Sum(o => o.S_GoodsCount),
                                GO_Id = Guid.NewGuid(),
                                GO_OrderMoney = list.Where(o => o.S_BusId == item.Key.S_BusId).Sum(o => o.S_GoodsCount * o.G_Price) + exFee,
                                GO_OrderNo = dt.ToString("yyyyMMddHHmmssfff") + new CreateRandomStr().GetRandomString(6),
                                GO_PayState = (int)PayStateEnum.未付款,
                                GO_OrderState = (int)OrderStateEnum.待确认,
                                GO_Remark = exFee > 0 ? "配送费：" + exFee : "",
                                GO_Time = DateTime.Now,
                                GO_UId = uid,
                                GO_UPhone = phone
                            };

                            var goods = list.Where(o => o.S_BusId == item.Key.S_BusId);
                            foreach (var good in goods)   //订单详情
                            {
                                Core_MallOrderDetail orderDetail = new Core_MallOrderDetail()
                                {
                                    OD_BusId = od.GO_BusId,
                                    OD_BusName = od.GO_BusName,
                                    OD_BusPhone = od.GO_BusPhone,
                                    OD_Count = good.S_GoodsCount,
                                    OD_GoodsId = good.S_GoodsId,
                                    OD_Id = Guid.NewGuid(),
                                    OD_Img = good.G_Img,
                                    OD_Name = good.G_Name,
                                    OD_OrderId = od.GO_OrderNo,
                                    OD_Price = good.G_Price,
                                    OD_Time = od.GO_Time,
                                    OD_UId = uid,
                                    OD_UPhone = phone,
                                    OD_Remark = good.G_ExtraFee > 0 ? "配送费：" + exFee : ""
                                };
                                odList.Add(orderDetail);
                                db.Entry<Core_MallOrderDetail>(orderDetail).State = EntityState.Added;  //添加订单详情

                                var carGoods =
                                    db.Set<Core_MalShoppingCar>().FirstOrDefault(o => o.S_GoodsId == good.S_GoodsId && o.S_UId == good.S_UId);
                                db.Set<Core_MalShoppingCar>().Attach(carGoods);
                                db.Entry(carGoods).State = EntityState.Deleted;   //将商品从购物车中移除

                            }

                            oList.Add(od);
                            db.Entry<Core_MallOrder>(od).State = EntityState.Added;  //添加订单详情
                        }

                        #endregion

                        if (db.SaveChanges() > 0)  //提交成功
                        {
                            rs.State = 0;
                            rs.Msg = "ok";

                            rs.Data = new
                            {
                                orderGoods = list.GroupBy(o => new { o.S_BusId, o.B_NickName }).Select(o => new
                                {
                                    busId = o.Key.S_BusId,
                                    busName = o.Key.B_NickName,
                                    busList = list.Where(p => p.S_BusId == o.Key.S_BusId).Select(s => new
                                    {
                                        busId = s.S_BusId,
                                        busName = s.B_NickName,
                                        imgUrl = s.G_Img,
                                        goodsId = s.S_GoodsId,
                                        goodsName = s.G_Name,
                                        goodsCount = s.S_GoodsCount,
                                        price = s.G_Price,
                                        fee = s.G_ExtraFee,
                                        state = s.G_Status
                                    }
                                    ),
                                    expFee = list.Max(n => n.G_ExtraFee),
                                    deliver = "",
                                    card = "",

                                    orderId = oList.FirstOrDefault(od => od.GO_BusId == o.Key.S_BusId).GO_Id,
                                    orderNo = oList.FirstOrDefault(od => od.GO_BusId == o.Key.S_BusId).GO_OrderNo
                                })
                            };
                        }
                        else
                        {
                            rs.Msg = "用户精品汇下单失败";
                        }
                    }
                }



            }
            return rs;
        }


        /// <summary>
        /// 支付订单
        /// </summary>
        /// <param name="orders">订单号</param> 
        /// <param name="ownerCardMoney">业主卡（如果有）</param>
        /// <param name="addressId">配送地址</param> 
        /// <returns></returns>
        public ResultMessageViewModel PayOrder(List<dynamic> orders, decimal ownerCardMoney, int addressId, Guid uid, string enPaypwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();


            if (orders.Any())  //订单存在
            {
                using (LinXinApp20Entities db = new LinXinApp20Entities())
                {
                    //余额
                    var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                    if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                    {
                        rs.State = 2;
                        rs.Msg = "您还未设置支付密码,请立即前去设置！";
                        return rs;
                    }
                    else
                    {
                        string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                        payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// 加密支付密码
                        if (!balance.B_PayPwd.Equals(payPwd))
                        {
                            rs.Msg = "支付密码错误！";
                            return rs;
                        }
                    }


                    //当前系统设置的商家到账比率
                    var rate = Convert.ToDecimal(db.Set<Sys_DiscountRate>().FirstOrDefault(o => o.R_State == 0).R_PayRate);

                    //收货地址
                    var address =
                        db.Set<Core_DeliveryAddress>().FirstOrDefault(o => o.A_Id == addressId && o.A_UserId == uid);
                    decimal orderFee = 0;//订单应付金额

                    string orderNo;
                    string businessCardId;
                    string remark;

                    if (ownerCardMoney > 0)  //使用业主卡
                    {
                        ownerCardMoney = balance.B_CouponMoney;
                    }
                    foreach (var order in orders)
                    {
                        orderNo = order.orderNo;  //订单号
                        businessCardId = order.businessCardId;  //优惠券卡Id（如果有）
                        remark = order.remark;//订单备注
                        string cardRemark = ""; //优惠卡描述;
                        Guid cardId;  //优惠卡

                        var od = db.Set<Core_MallOrder>().FirstOrDefault(o => o.GO_OrderNo == orderNo);  //订单
                        if (od.GO_Time.AddDays(2) < DateTime.Now)  //超过两天没有付款，订单作废
                        {
                            od.GO_OrderState = (int)OrderStateEnum.已取消;

                            db.Set<Core_MallOrder>().Attach(od);
                            db.Entry<Core_MallOrder>(od).State = EntityState.Modified;
                            rs.Msg = "订单48小时内未支付，系统已将订单取消！";
                            return rs;
                        }

                        //累计订单里面的商品销量
                        var goodsList = db.Core_MallOrderDetail.Where(t => t.OD_OrderId == od.GO_OrderNo);
                        foreach (var goods in goodsList)
                        {
                            var ginfo = db.Core_MallGoods.FirstOrDefault(o => o.G_Id == goods.OD_GoodsId);
                            ginfo.G_Sales += 1;
                        }


                        decimal payorderMoney = od.GO_OrderMoney; //单个订单，应该缴纳的金额

                        od.GO_Remark += remark;
                        if (!string.IsNullOrEmpty(businessCardId))  //有商家的优惠卡
                        {
                            cardId = Guid.Parse(businessCardId);
                            //od.GO_VoucherCardId = 

                            var vcard = db.Set<Core_VoucherCardHistory>().FirstOrDefault(o => o.H_CardId == cardId && o.H_State == 0 && o.H_ETime > DateTime.Now && o.H_STime < DateTime.Now);
                            if (vcard != null)  //卡存在的
                            {
                                od.GO_VoucherCardId = vcard.H_Id;
                                od.GO_VoucherCardMoney = vcard.H_Money;

                                payorderMoney -= vcard.H_Money;

                                //【1】添加对应的 优惠卡消费 记录
                                Core_BillMember bill = new Core_BillMember()
                                {
                                    B_Time = DateTime.Now,
                                    B_Title = string.Format("使用商家:{0}，的优惠卡-{1}", od.GO_BusName, vcard.H_Title),
                                    B_Money = -vcard.H_Money,
                                    B_UId = od.GO_UId,
                                    B_Phone = od.GO_UPhone,
                                    B_Module = (int)BillEnum.商家抵用券,
                                    B_OrderId = od.GO_Id,
                                    B_Type = (int)MemberRoleEnum.会员,
                                    B_Flag = (int)BillFlagEnum.普通流水,
                                    B_Status = 0
                                };
                                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//添加账单详细 不提交

                                cardRemark = string.Format("使用商家优惠卡消费：【{0}】,抵扣金额，{1}", vcard.H_Title, vcard.H_Money);
                                //【2】变更领取的优惠卡状态
                                vcard.H_State = (int)CardUseStateEnum.已消费;
                                db.Set<Core_VoucherCardHistory>().Attach(vcard);
                                db.Entry<Core_VoucherCardHistory>(vcard).State = EntityState.Modified;

                            }
                            else
                            {
                                rs.Msg = "商家优惠卡不正确";
                                return rs;
                            }

                        }
                        //支付订单后，需给对应的商家帐号累计订单的金额（1.如果商家自己发的优惠券，行家自行负责 2.商家到账金额，为订单金额*当前比率）

                        //【7】添加 商家收入 记录
                        Core_BillMember billBus = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = string.Format("精品购收入。订单号：【{0}】；到账比率：【{1}】", od.GO_OrderNo, rate),// "精品购收入,订单号："+od.GO_OrderNo,
                            B_Money = Convert.ToDecimal(payorderMoney * rate),
                            B_UId = od.GO_BusId,
                            B_Phone = od.GO_BusPhone,
                            B_Module = (int)BillEnum.商品购买,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.商家,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0,
                            B_Remark = string.IsNullOrEmpty(cardRemark) ? "" : cardRemark
                        };
                        db.Entry<Core_BillMember>(billBus).State = EntityState.Added;//添加账单详细 不提交


                        // 【8】 变更商家可用余额
                        var balanceBus = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == od.GO_BusId);
                        balanceBus.B_Balance += Convert.ToDecimal(payorderMoney * rate);


                        //【9】添加 平台收入 记录
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Time = DateTime.Now,
                            B_Title = string.Format("精品购收入。订单号：【{0}】；到账比率：【{1}】", od.GO_BusId, rate),// "精品购收入,订单号："+od.GO_OrderNo,
                            B_Money = payorderMoney - Convert.ToDecimal(payorderMoney * rate),
                            B_UId = od.GO_BusId,
                            B_Phone = od.GO_BusPhone,
                            B_Module = (int)BillEnum.商品购买,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.商家,
                            B_Flag = (int)BillFlagEnum.平台流水,
                            B_Status = 0,
                            B_Remark = string.IsNullOrEmpty(cardRemark) ? "" : cardRemark
                        };
                        db.Entry<Sys_BillMaster>(billMaster).State = EntityState.Added;//添加账单详细 不提交



                        if (ownerCardMoney > 0)  //使用业主卡
                        {
                            //od.GO_OwnerCardMoney =  
                            if (payorderMoney > ownerCardMoney)  //业主卡抵扣
                            {
                                od.GO_OwnerCardMoney = ownerCardMoney;// 业主卡抵扣的金额
                                payorderMoney -= ownerCardMoney;
                                ownerCardMoney = 0;
                            }
                            else
                            {
                                od.GO_OwnerCardMoney = payorderMoney;
                                ownerCardMoney -= payorderMoney;  //减少业主卡金额
                                payorderMoney = 0;//标示 本订单，业主卡全额支付
                            }


                            //【3】添加对应的 业主卡消费 记录
                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Title = "使用平台业主卡消费-精品汇",
                                B_Money = -od.GO_OwnerCardMoney,
                                B_UId = od.GO_UId,
                                B_Phone = od.GO_UPhone,
                                B_Module = (int)BillEnum.平台业主卡,
                                B_OrderId = od.GO_Id,
                                B_Type = (int)MemberRoleEnum.会员,
                                B_Flag = (int)BillFlagEnum.普通流水,
                                B_Status = 0
                            };
                            db.Entry<Core_BillMember>(bill).State = EntityState.Added;//添加账单详细 不提交


                        }

                        //修改订单配送地址
                        od.GO_TargetAddress = address.A_Address;
                        od.GO_TargetName = address.A_UserName;
                        od.GO_TargetPhone = address.A_Phone;
                        od.GO_PayState = (int)PayStateEnum.已付款;
                        od.GO_OrderState = (int)OrderStateEnum.待发货;

                        //【4】修改订单的信息
                        db.Set<Core_MallOrder>().Attach(od);
                        db.Entry<Core_MallOrder>(od).State = EntityState.Modified;

                        orderFee += payorderMoney;  //累计本订单应该支付的金额


                        //【5】添加订单的 消费 记录
                        Core_BillMember billOrder = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "精品汇购物消费",
                            B_Money = -payorderMoney,
                            B_UId = od.GO_UId,
                            B_Phone = od.GO_UPhone,
                            B_Module = (int)BillEnum.商品购买,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.会员,
                            B_Flag = (int)BillFlagEnum.普通流水,
                            B_Status = 0
                        };
                        db.Entry<Core_BillMember>(billOrder).State = EntityState.Added;//添加账单详细 不提交
                    }
                    //所有订单 金额
                    if (balance.B_Balance > orderFee)
                    {
                        balance.B_Balance -= orderFee;  //减少余额
                        if (ownerCardMoney > -1)  //使用业主卡
                        {
                            balance.B_CouponMoney = ownerCardMoney;// 减少业主卡金额
                        }


                        //【6】修改余额的信息
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry<Core_Balance>(balance).State = EntityState.Modified;


                        //单元提交
                        if (db.SaveChanges() > 0)
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                        }
                    }
                    else
                    {
                        rs.State = 3;
                        rs.Msg = "用户帐号余额不足,请及时充值！";
                    }
                }
            }
            return rs;
        }
    }

    public class MallOrderDetailCore : BaseRepository<Core_MallOrderDetail>
    {
        /// <summary>
        /// 获取用户的订单 详情 列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ResultMessageViewModel GetUserOrderDetailList(Guid uid, int pn, int rows, string imgBaseUrl = "")
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                var list  =
                    db.Set<Core_MallOrder>()
                        .Where(o => o.GO_UId == uid && o.GO_PayState == (int)PayStateEnum.已付款)
                        .Join(db.Set<Core_MallOrderDetail>(), a => a.GO_OrderNo, o => o.OD_OrderId, (a, o) => new
                        {

                            id = o.OD_Id,
                            uid = o.OD_UId,
                            imgUrl = imgBaseUrl + o.OD_Img,
                            goodsName = o.OD_Name,
                            goodsCount = o.OD_Count,
                            goodsId = o.OD_GoodsId,
                            busName = o.OD_BusName,
                            busId = o.OD_BusId,
                            linkPhone = o.OD_BusPhone,
                            price = o.OD_Price,
                            time = o.OD_Time,
                            //  orderNo = o.OD_OrderId
                            orderNo = o.OD_OrderId,
                            //state = Enum.GetName(typeof(OrderStateEnum), a.GO_OrderState)
                            state = a.GO_OrderState
                            //payState = o.GO_PayState,
                            //orderState = o.GO_OrderState
                        }).OrderByDescending(o=>o.time).Skip(pn * rows).Take(rows).ToList();
                if (list.Any())
                {
                    rs.Data = list;
                }
                else
                {
                    rs.State = 1;
                    rs.Msg = "暂无更多数据";

                }
            }

            return rs;
        }
    }
}
