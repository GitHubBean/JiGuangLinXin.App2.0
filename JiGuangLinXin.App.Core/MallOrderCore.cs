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
        /// ȷ���µ�
        /// </summary>
        /// <param name="uid">��ԱID</param>
        /// <param name="phone">��Ա�绰</param>
        /// <param name="ids">�����ύ��������ƷID�����ﳵ���ϣ����ŷָ�</param>
        /// <param name="flag">0ֱ�ӹ��� 1���ﳵ��ת</param>
        /// <param name="count">���������</param>
        /// <returns></returns>
        public ResultMessageViewModel OrderConfirm(Guid uid, string phone, string ids, int flag, int count)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();

            if (!string.IsNullOrEmpty(ids))
            {


                using (DbContext db = new LinXinApp20Entities())
                {
                    DateTime dt = DateTime.Now;
                    if (flag == 0)  //ֱ�ӹ���
                    {
                        //var list =mgCore.LoadEntities(o => ids.Contains(o.G_Id.ToString())).OrderByDescending(o => o.G_Time).ToList();

                        //if (list.Any(p => p.G_Status != 0))  //�й��ڵ���Ʒ
                        //{
                        //    rs.Msg = "ѡ�����Ʒ�е����¼ܣ�������ѡ��";
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

                        Guid gid = Guid.Parse(ids);  //ֻ��һ��ID����
                        var info = db.Set<Core_MallGoods>().FirstOrDefault(p => p.G_Id == gid);

                        if (info.G_Status != 0)  //���ڵ���Ʒ
                        {
                            rs.Msg = "ѡ�����Ʒ�е����¼ܣ�������ѡ��";
                            return rs;
                        }


                        #region �ύ����

                        Core_MallOrder order = new Core_MallOrder()
                        {
                            GO_BusId = info.G_BusId,
                            GO_BusName = info.G_BusName,
                            GO_BusPhone = info.G_BusPhone,
                            GO_GoodsCount = count,
                            GO_Id = Guid.NewGuid(),
                            GO_OrderMoney = info.G_Price * count,
                            GO_PayState = (int)PayStateEnum.δ����,
                            GO_OrderState = (int)OrderStateEnum.��ȷ��,
                            GO_Remark = info.G_ExtraFee > 0 ? "���ͷѣ�" + info.G_ExtraFee : "",
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

                        db.Entry<Core_MallOrder>(order).State = EntityState.Added;//��Ӷ���
                        db.Entry<Core_MallOrderDetail>(orderDetail).State = EntityState.Added;  //��Ӷ�������
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
                            rs.Msg = "�û���Ʒ���µ�ʧ��";
                        }
                    }
                    else   //���ﳵ�ύ
                    {
                        var idsArr = ids.Split(',');
                        var list = db.Set<view_shoppingCar>().Where(o => idsArr.Any(d => d == o.S_Id.ToString())).OrderByDescending(o => o.S_Time).ToList();

                        if (list.Any(p => p.G_Status != 0))  //�й��ڵ���Ʒ
                        {
                            rs.Msg = "ѡ�����Ʒ�е����¼ܣ�������ѡ��";
                            return rs;
                        }

                        #region �ύ����

                        var gpBus = list.GroupBy(o => new { o.S_BusId, o.B_NickName, o.B_Phone });

                        List<Core_MallOrder> oList = new List<Core_MallOrder>();
                        List<Core_MallOrderDetail> odList = new List<Core_MallOrderDetail>();
                        foreach (var item in gpBus)
                        {
                            //���ͷ�
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
                                GO_PayState = (int)PayStateEnum.δ����,
                                GO_OrderState = (int)OrderStateEnum.��ȷ��,
                                GO_Remark = exFee > 0 ? "���ͷѣ�" + exFee : "",
                                GO_Time = DateTime.Now,
                                GO_UId = uid,
                                GO_UPhone = phone
                            };

                            var goods = list.Where(o => o.S_BusId == item.Key.S_BusId);
                            foreach (var good in goods)   //��������
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
                                    OD_Remark = good.G_ExtraFee > 0 ? "���ͷѣ�" + exFee : ""
                                };
                                odList.Add(orderDetail);
                                db.Entry<Core_MallOrderDetail>(orderDetail).State = EntityState.Added;  //��Ӷ�������

                                var carGoods =
                                    db.Set<Core_MalShoppingCar>().FirstOrDefault(o => o.S_GoodsId == good.S_GoodsId && o.S_UId == good.S_UId);
                                db.Set<Core_MalShoppingCar>().Attach(carGoods);
                                db.Entry(carGoods).State = EntityState.Deleted;   //����Ʒ�ӹ��ﳵ���Ƴ�

                            }

                            oList.Add(od);
                            db.Entry<Core_MallOrder>(od).State = EntityState.Added;  //��Ӷ�������
                        }

                        #endregion

                        if (db.SaveChanges() > 0)  //�ύ�ɹ�
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
                            rs.Msg = "�û���Ʒ���µ�ʧ��";
                        }
                    }
                }



            }
            return rs;
        }


        /// <summary>
        /// ֧������
        /// </summary>
        /// <param name="orders">������</param> 
        /// <param name="ownerCardMoney">ҵ����������У�</param>
        /// <param name="addressId">���͵�ַ</param> 
        /// <returns></returns>
        public ResultMessageViewModel PayOrder(List<dynamic> orders, decimal ownerCardMoney, int addressId, Guid uid, string enPaypwd)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();


            if (orders.Any())  //��������
            {
                using (LinXinApp20Entities db = new LinXinApp20Entities())
                {
                    //���
                    var balance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                    if (string.IsNullOrEmpty(balance.B_PayPwd) || string.IsNullOrEmpty(balance.B_EncryptCode))
                    {
                        rs.State = 2;
                        rs.Msg = "����δ����֧������,������ǰȥ���ã�";
                        return rs;
                    }
                    else
                    {
                        string payPwd = DESProvider.DecryptString(enPaypwd);  //֧������
                        payPwd = Md5Extensions.MD5Encrypt(payPwd + balance.B_EncryptCode);// ����֧������
                        if (!balance.B_PayPwd.Equals(payPwd))
                        {
                            rs.Msg = "֧���������";
                            return rs;
                        }
                    }


                    //��ǰϵͳ���õ��̼ҵ��˱���
                    var rate = Convert.ToDecimal(db.Set<Sys_DiscountRate>().FirstOrDefault(o => o.R_State == 0).R_PayRate);

                    //�ջ���ַ
                    var address =
                        db.Set<Core_DeliveryAddress>().FirstOrDefault(o => o.A_Id == addressId && o.A_UserId == uid);
                    decimal orderFee = 0;//����Ӧ�����

                    string orderNo;
                    string businessCardId;
                    string remark;

                    if (ownerCardMoney > 0)  //ʹ��ҵ����
                    {
                        ownerCardMoney = balance.B_CouponMoney;
                    }
                    foreach (var order in orders)
                    {
                        orderNo = order.orderNo;  //������
                        businessCardId = order.businessCardId;  //�Ż�ȯ��Id������У�
                        remark = order.remark;//������ע
                        string cardRemark = ""; //�Żݿ�����;
                        Guid cardId;  //�Żݿ�

                        var od = db.Set<Core_MallOrder>().FirstOrDefault(o => o.GO_OrderNo == orderNo);  //����
                        if (od.GO_Time.AddDays(2) < DateTime.Now)  //��������û�и����������
                        {
                            od.GO_OrderState = (int)OrderStateEnum.��ȡ��;

                            db.Set<Core_MallOrder>().Attach(od);
                            db.Entry<Core_MallOrder>(od).State = EntityState.Modified;
                            rs.Msg = "����48Сʱ��δ֧����ϵͳ�ѽ�����ȡ����";
                            return rs;
                        }

                        //�ۼƶ����������Ʒ����
                        var goodsList = db.Core_MallOrderDetail.Where(t => t.OD_OrderId == od.GO_OrderNo);
                        foreach (var goods in goodsList)
                        {
                            var ginfo = db.Core_MallGoods.FirstOrDefault(o => o.G_Id == goods.OD_GoodsId);
                            ginfo.G_Sales += 1;
                        }


                        decimal payorderMoney = od.GO_OrderMoney; //����������Ӧ�ý��ɵĽ��

                        od.GO_Remark += remark;
                        if (!string.IsNullOrEmpty(businessCardId))  //���̼ҵ��Żݿ�
                        {
                            cardId = Guid.Parse(businessCardId);
                            //od.GO_VoucherCardId = 

                            var vcard = db.Set<Core_VoucherCardHistory>().FirstOrDefault(o => o.H_CardId == cardId && o.H_State == 0 && o.H_ETime > DateTime.Now && o.H_STime < DateTime.Now);
                            if (vcard != null)  //�����ڵ�
                            {
                                od.GO_VoucherCardId = vcard.H_Id;
                                od.GO_VoucherCardMoney = vcard.H_Money;

                                payorderMoney -= vcard.H_Money;

                                //��1����Ӷ�Ӧ�� �Żݿ����� ��¼
                                Core_BillMember bill = new Core_BillMember()
                                {
                                    B_Time = DateTime.Now,
                                    B_Title = string.Format("ʹ���̼�:{0}�����Żݿ�-{1}", od.GO_BusName, vcard.H_Title),
                                    B_Money = -vcard.H_Money,
                                    B_UId = od.GO_UId,
                                    B_Phone = od.GO_UPhone,
                                    B_Module = (int)BillEnum.�̼ҵ���ȯ,
                                    B_OrderId = od.GO_Id,
                                    B_Type = (int)MemberRoleEnum.��Ա,
                                    B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                                    B_Status = 0
                                };
                                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//����˵���ϸ ���ύ

                                cardRemark = string.Format("ʹ���̼��Żݿ����ѣ���{0}��,�ֿ۽�{1}", vcard.H_Title, vcard.H_Money);
                                //��2�������ȡ���Żݿ�״̬
                                vcard.H_State = (int)CardUseStateEnum.������;
                                db.Set<Core_VoucherCardHistory>().Attach(vcard);
                                db.Entry<Core_VoucherCardHistory>(vcard).State = EntityState.Modified;

                            }
                            else
                            {
                                rs.Msg = "�̼��Żݿ�����ȷ";
                                return rs;
                            }

                        }
                        //֧�������������Ӧ���̼��ʺ��ۼƶ����Ľ�1.����̼��Լ������Ż�ȯ���м����и��� 2.�̼ҵ��˽�Ϊ�������*��ǰ���ʣ�

                        //��7����� �̼����� ��¼
                        Core_BillMember billBus = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = string.Format("��Ʒ�����롣�����ţ���{0}�������˱��ʣ���{1}��", od.GO_OrderNo, rate),// "��Ʒ������,�����ţ�"+od.GO_OrderNo,
                            B_Money = Convert.ToDecimal(payorderMoney * rate),
                            B_UId = od.GO_BusId,
                            B_Phone = od.GO_BusPhone,
                            B_Module = (int)BillEnum.��Ʒ����,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.�̼�,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Status = 0,
                            B_Remark = string.IsNullOrEmpty(cardRemark) ? "" : cardRemark
                        };
                        db.Entry<Core_BillMember>(billBus).State = EntityState.Added;//����˵���ϸ ���ύ


                        // ��8�� ����̼ҿ������
                        var balanceBus = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == od.GO_BusId);
                        balanceBus.B_Balance += Convert.ToDecimal(payorderMoney * rate);


                        //��9����� ƽ̨���� ��¼
                        Sys_BillMaster billMaster = new Sys_BillMaster()
                        {
                            B_Time = DateTime.Now,
                            B_Title = string.Format("��Ʒ�����롣�����ţ���{0}�������˱��ʣ���{1}��", od.GO_BusId, rate),// "��Ʒ������,�����ţ�"+od.GO_OrderNo,
                            B_Money = payorderMoney - Convert.ToDecimal(payorderMoney * rate),
                            B_UId = od.GO_BusId,
                            B_Phone = od.GO_BusPhone,
                            B_Module = (int)BillEnum.��Ʒ����,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.�̼�,
                            B_Flag = (int)BillFlagEnum.ƽ̨��ˮ,
                            B_Status = 0,
                            B_Remark = string.IsNullOrEmpty(cardRemark) ? "" : cardRemark
                        };
                        db.Entry<Sys_BillMaster>(billMaster).State = EntityState.Added;//����˵���ϸ ���ύ



                        if (ownerCardMoney > 0)  //ʹ��ҵ����
                        {
                            //od.GO_OwnerCardMoney =  
                            if (payorderMoney > ownerCardMoney)  //ҵ�����ֿ�
                            {
                                od.GO_OwnerCardMoney = ownerCardMoney;// ҵ�����ֿ۵Ľ��
                                payorderMoney -= ownerCardMoney;
                                ownerCardMoney = 0;
                            }
                            else
                            {
                                od.GO_OwnerCardMoney = payorderMoney;
                                ownerCardMoney -= payorderMoney;  //����ҵ�������
                                payorderMoney = 0;//��ʾ ��������ҵ����ȫ��֧��
                            }


                            //��3����Ӷ�Ӧ�� ҵ�������� ��¼
                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = DateTime.Now,
                                B_Title = "ʹ��ƽ̨ҵ��������-��Ʒ��",
                                B_Money = -od.GO_OwnerCardMoney,
                                B_UId = od.GO_UId,
                                B_Phone = od.GO_UPhone,
                                B_Module = (int)BillEnum.ƽ̨ҵ����,
                                B_OrderId = od.GO_Id,
                                B_Type = (int)MemberRoleEnum.��Ա,
                                B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                                B_Status = 0
                            };
                            db.Entry<Core_BillMember>(bill).State = EntityState.Added;//����˵���ϸ ���ύ


                        }

                        //�޸Ķ������͵�ַ
                        od.GO_TargetAddress = address.A_Address;
                        od.GO_TargetName = address.A_UserName;
                        od.GO_TargetPhone = address.A_Phone;
                        od.GO_PayState = (int)PayStateEnum.�Ѹ���;
                        od.GO_OrderState = (int)OrderStateEnum.������;

                        //��4���޸Ķ�������Ϣ
                        db.Set<Core_MallOrder>().Attach(od);
                        db.Entry<Core_MallOrder>(od).State = EntityState.Modified;

                        orderFee += payorderMoney;  //�ۼƱ�����Ӧ��֧���Ľ��


                        //��5����Ӷ����� ���� ��¼
                        Core_BillMember billOrder = new Core_BillMember()
                        {
                            B_Time = DateTime.Now,
                            B_Title = "��Ʒ�㹺������",
                            B_Money = -payorderMoney,
                            B_UId = od.GO_UId,
                            B_Phone = od.GO_UPhone,
                            B_Module = (int)BillEnum.��Ʒ����,
                            B_OrderId = od.GO_Id,
                            B_Type = (int)MemberRoleEnum.��Ա,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Status = 0
                        };
                        db.Entry<Core_BillMember>(billOrder).State = EntityState.Added;//����˵���ϸ ���ύ
                    }
                    //���ж��� ���
                    if (balance.B_Balance > orderFee)
                    {
                        balance.B_Balance -= orderFee;  //�������
                        if (ownerCardMoney > -1)  //ʹ��ҵ����
                        {
                            balance.B_CouponMoney = ownerCardMoney;// ����ҵ�������
                        }


                        //��6���޸�������Ϣ
                        db.Set<Core_Balance>().Attach(balance);
                        db.Entry<Core_Balance>(balance).State = EntityState.Modified;


                        //��Ԫ�ύ
                        if (db.SaveChanges() > 0)
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                        }
                    }
                    else
                    {
                        rs.State = 3;
                        rs.Msg = "�û��ʺ�����,�뼰ʱ��ֵ��";
                    }
                }
            }
            return rs;
        }
    }

    public class MallOrderDetailCore : BaseRepository<Core_MallOrderDetail>
    {
        /// <summary>
        /// ��ȡ�û��Ķ��� ���� �б�
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
                        .Where(o => o.GO_UId == uid && o.GO_PayState == (int)PayStateEnum.�Ѹ���)
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
                    rs.Msg = "���޸�������";

                }
            }

            return rs;
        }
    }
}
