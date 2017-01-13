using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Services;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 精品购
    /// </summary>
    public class BoutiqueMallController : BaseController
    {

        private VoucherCardCore vcCore = new VoucherCardCore();
        private MallGoodsCore mgCore = new MallGoodsCore();
        private VillageCore vCore = new VillageCore();
        private MallTypeCore mtCore = new MallTypeCore();
        private AttachmentCore attCore = new AttachmentCore();
        private MalShoppingCarCore msCore = new MalShoppingCarCore();
        private ShoppingCarViewCore scCoew = new ShoppingCarViewCore();
        private DeliveryAddressCore daCore = new DeliveryAddressCore();
        private VoucherCardHistoryCore chCore = new VoucherCardHistoryCore();

        private MallOrderCore orderCore = new MallOrderCore();
        private MallOrderDetailCore orderDetailCore = new MallOrderDetailCore();

        private BalanceCore balanceCore = new BalanceCore();
        /// <summary>
        /// 精品购首页
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage Main([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            //int pn = obj.pn;
            //int rows = obj.rows;
            //pn = pn - 1;

            var villInfo = vCore.LoadEntity(o => o.V_Id == buildingId);
            var categoryList = mtCore.LoadEntities(o=>o.T_State == 0).OrderBy(o => o.T_Rank);
            int cateId = categoryList.First().T_Id;
            //  var goodsList = mgCore.QueryGoods(buildingId, pn, rows, cateId);

            rs.Data = new
            {
                staticImgUrl = StaticHttpUrl,
                buildingLogo = StaticHttpUrl + villInfo.V_Img,
                buildingName = villInfo.V_BuildingName,
                categoryList = categoryList.Select(c => new
                {
                    cid = c.T_Id,
                    title = c.T_Title,
                    remark = c.T_Remark
                }),
                //goodsList,
                cateId
            };

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 查询所有的精品汇分类
        /// </summary> 
        /// <returns></returns>
        public HttpResponseMessage QueryAllGoodsCategory()
        {

            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            rs.Data = mtCore.LoadEntities(o=>o.T_State==0).OrderBy(o => o.T_Rank).Select(c => new
                {
                    cid = c.T_Id,
                    title = c.T_Title,
                    remark = c.T_Remark
                });

            return WebApiJsonResult.ToJson(rs);
        }




        /// <summary>
        /// 根据分类查询列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage QueryByCateId([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            int pn = obj.pn;
            int rows = obj.rows;
            pn = pn - 1;
            int cateId = obj.cateId;
            int orderBy = obj.orderBy;
            string orderByStr = "";
            if (orderBy != 0)
            {
                orderByStr = " c.G_Sales desc ";
            }

            var goodsList = mgCore.QueryGoods(buildingId, pn, rows, cateId, orderByStr);

            rs.Data = new
            {
                staticImgUrl = StaticHttpUrl,
                goodsList,
                cateId
            };

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 精品购详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage GoodsShow([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid goodsId = obj.goodsId;


            var goods = mgCore.LoadEntity(o => o.G_Id == goodsId);

            goods.G_Clicks += 1;  //累计人气
            mgCore.UpdateEntity(goods);

            var att = attCore.LoadEntities(o => o.A_PId == goodsId); //附件图片

            rs.Data = new
            {
                gid = goodsId,
                coverImg = StaticHttpUrl + goods.G_Img.Replace("_2", "_1"),  //存储的是中尺寸图片，输出需要调整成最大尺寸
                attList = att.Select(o => new
                {
                    imgUrl = StaticHttpUrl + o.A_Folder + "/" + o.A_FileName.Replace("_2", "_1")  // 存储的是中尺寸图片，输出需要调整成最大尺寸
                }),
                title = goods.G_Name,
                busId = goods.G_BusId,
                busName = goods.G_BusName,
                price = goods.G_Price,
                oldPrice = goods.G_PriceOld,
                fee = goods.G_ExtraFee,
                sales = goods.G_Sales,
                clicks = goods.G_Clicks,
                desc = goods.G_Desc,
                tags = goods.G_Tags
            };
            return WebApiJsonResult.ToJson(rs);
        }


        #region 购物车
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ShoppingCarAdd([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid goodsId = obj.goodsId;
            int goodsCount = obj.goodsCount;

            var spc = msCore.LoadEntity(o => o.S_UId == uid && o.S_GoodsId == goodsId);
            if (spc != null)  //购物车已经存在
            {
                spc.S_GoodsCount += 1;//累计数量即可

                if (!msCore.UpdateEntity(spc))
                {
                    rs.State = 1;
                    rs.Msg = "加入购物车失败";
                }
                return WebApiJsonResult.ToJson(rs);
            }


            var goods = mgCore.LoadEntity(p => p.G_Id == goodsId);

            Core_MalShoppingCar car = new Core_MalShoppingCar()
            {
                S_GodosImg = goods.G_Img,
                S_GoodsCount = goodsCount,
                S_GoodsId = goodsId,
                S_GoodsName = goods.G_Name,
                S_Price = goods.G_Price,
                S_BusName = goods.G_BusName,
                S_BusId = goods.G_BusId,
                S_Remark = "",
                S_Time = DateTime.Now,
                S_UId = uid,
                S_ExtraFee = goods.G_ExtraFee
            };

            if (msCore.AddEntity(car) == null)
            {
                rs.State = 1;
                rs.Msg = "加入购物车失败";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 移除购物车
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ShoppingCarRemove([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            int carId = obj.carId;

            //移除失败
            if (!msCore.DeleteEntity(msCore.LoadEntity(o => o.S_Id == carId)))
            {
                rs.State = 1;
                rs.Msg = "移除购物车失败";
            }

            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 我的购物车
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage ShoppingCar()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());


            //var list = msCore.LoadEntities(o => o.S_UId == uid).OrderByDescending(o=>o.S_Time).ToList();
            var list = scCoew.LoadEntities(o => o.S_UId == uid).OrderByDescending(o => o.S_Time).ToList();


            //按照商家分组查询
            var gp = list.GroupBy(o => new { o.S_BusId, o.B_NickName }).Select(o => new
            {
                busId = o.Key.S_BusId,
                busName = o.Key.B_NickName,
                busList = list.Where(p => p.S_BusId == o.Key.S_BusId).Select(s => new
                {
                    busId = s.S_BusId,
                    busName = s.B_NickName,
                    imgUrl = StaticHttpUrl + s.G_Img,
                    carId = s.S_Id,
                    goodsId = s.S_GoodsId,
                    goodsName = s.G_Name,
                    goodsCount = s.S_GoodsCount,
                    price = s.G_Price,
                    fee = s.G_ExtraFee,
                    state = s.G_Status,
                    clientFlag = "0"
                }),
                extraFee = o.Max(c => c.G_ExtraFee)  //以当前商家邮费最多的商品为准
            });

            rs.Data = gp;

            return WebApiJsonResult.ToJson(rs);
        }


        #endregion


        #region 订单

        /// <summary>
        /// 确认提交订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OrderConfrim([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            string phone = Request.Headers.GetValues("phone").FirstOrDefault();
            string ids = obj.ids;  //所有提交过来的商品ID、购物车集合，逗号分割
            int flag = obj.flag;  //0直接购买 1购物车跳转
            int count = obj.count;  //购买的数量



            if (!string.IsNullOrEmpty(ids))
            {
                var address = daCore.LoadEntity(p => p.A_UserId == uid && p.A_State == 0 && p.A_Default == 1);

                rs = orderCore.OrderConfirm(uid, phone, ids, flag, count);

                if (rs.State == 0) //成功返回
                {
                    //     dynamic tempRs = rs.Data;
                    rs.Data = new
                    {
                        addressId = address == null ? 0 : address.A_Id,
                        addressName = address == null ? "" : address.A_UserName,
                        addressPhone = address == null ? "" : address.A_Phone,
                        address = address == null ? "" : address.A_Address,
                        addressRemark = address == null ? "" : address.A_Remark,
                        cardId = "0",
                        baseImgUrl = StaticHttpUrl,
                        orderResult = rs.Data
                    };
                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 支付订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OrderPay([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            string odersStr = obj.orders;
            List<dynamic> orders = JsonSerialize.Instance.JsonToObject<List<dynamic>>(odersStr);
            decimal ownerCardMoney = obj.ownerCardMoney;
            int addressId = obj.addressId;
            string enPayPwd = obj.payPwd; //支付密码

            rs = orderCore.PayOrder(orders, ownerCardMoney, addressId, uid, enPayPwd);
            if (rs.State == 0)
            {

                #region 消息推送

                JPushMsgModel jm = new JPushMsgModel()
                {
                    code = (int)MessageCenterModuleEnum.邻妹妹,
                    proFlag = (int)PushMessageEnum.默认,
                    proTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tags = "精品汇",
                    title = "您购买的精品汇商品已成功下单",
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tips = " 您购买的精品汇商品已成功下单，社区商家会尽快与您联系，请保持联系方式畅通，我们将竭诚为您服务。",
                };

                Tuisong.PushMessage((int)PushPlatformEnum.Alias, jm.title, jm.title, JsonSerialize.Instance.ObjectToJson(jm), uid.ToString("N").ToLower());
                #endregion

            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 订单可用的商家抵用券
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OrderBusinessCard([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid busId = obj.busId;

            var cardHistory = chCore.LoadEntities(o => o.H_UserId == uid && o.H_BusId == busId && o.H_State == 0 && o.H_ETime > DateTime.Now);

            if (cardHistory.Any())  //有券
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = cardHistory.Select(o => new
                {
                    historyId = o.H_Id,
                    title = o.H_Title,
                    money = o.H_Money,
                    sTime = o.H_STime,
                    eTime = o.H_ETime,
                    busName = o.H_Nickname,
                    busId = o.H_BusId,
                    cardId = o.H_CardId,
                    flag = "0"
                });
            }
            else
            {
                rs.Msg = "暂无可用抵用券";
            }
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 订单可用的用户消费卡
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage OrderOwnerCard()
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            var carBalance = balanceCore.LoadEntity(o => o.B_AccountId == uid);
            if (carBalance.B_CouponMoney > 0)  //有卡有余额
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    cardMoney = carBalance.B_CouponMoney
                };
            }
            else
            {
                rs.Msg = "业主卡暂无余额";
            }
            return WebApiJsonResult.ToJson(rs);
        }






        #endregion



        #region 收获地址
        /// <summary>
        /// 用户收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddressMgr()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            var address = daCore.LoadEntities(p => p.A_UserId == uid && p.A_State == 0).OrderByDescending(o => o.A_Default).Select(o => new
            {
                aid = o.A_Id,
                userName = o.A_UserName,
                phone = o.A_Phone,
                address = o.A_Address,
                defualt = o.A_Default,
                flag = "0"
            });

            rs.Data = address;

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 编辑收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddressEdit([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            string uname = obj.userName;
            string phone = obj.phone;
            string addre = obj.address;
            string rem = obj.remark;
            int aid = obj.aid;

            if (aid <= 1)  //新增
            {
                Core_DeliveryAddress info = new Core_DeliveryAddress()
                {
                    A_Address = addre,
                    A_Default = 0,
                    A_Phone = phone,
                    A_Remark = rem,
                    A_State = 0,
                    A_UserId = uid,
                    A_UserName = uname
                };
                if (daCore.AddEntity(info) != null)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {
                Core_DeliveryAddress info = daCore.LoadEntity(p => p.A_Id == aid);
                info.A_UserName = uname;
                info.A_Phone = phone;
                info.A_Remark = rem;
                info.A_State = 0;
                info.A_Address = addre;

                if (daCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddressRemove([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            int aid = obj.aid;


            Core_DeliveryAddress info = daCore.LoadEntity(p => p.A_Id == aid);
            info.A_State = 1;
            daCore.UpdateEntity(info);
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 设置收货地址默认
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage AddressDefault([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            int aid = obj.aid;
            var up = daCore.UpdateByExtended(o => new Core_DeliveryAddress() { A_Default = 0 }, o => o.A_UserId == uid);  //批量修改该用户的收货地址w
            if (up > 0)  //修改成功
            {
                Core_DeliveryAddress info = daCore.LoadEntity(p => p.A_Id == aid);
                info.A_Default = 1;
                var ds = daCore.UpdateEntity(info);
                if (ds)  // 默认成功
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return WebApiJsonResult.ToJson(rs);
        }




        #endregion
    }
}



