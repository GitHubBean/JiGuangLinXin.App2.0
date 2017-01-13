using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;

namespace JiGuangLinXin.App.Core
{
    public class LuckyGiftCore : BaseRepository<Core_LuckyGift>
    {

        #region 拆红包
        /// <summary>
        /// 拆红包
        /// </summary>
        /// <param name="money">总金额</param>
        /// <param name="count">多少个</param>
        /// <returns></returns>
        public double CalcGift(double money, int count)
        {
            //最少保证可以领到 0.01
            if (money - 0.01 * count > 0)
            {
                double cur = GetRandom(money - 0.01 * count, 0.01);
                while (cur >= (money / count) * 2)  //不能大于 平均数的两倍
                {
                    cur = GetRandom(money - 0.01 * count, 0.01);
                }
                return cur;
            }
            return 0.01;
        }
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public double GetRandom(double max, double min)
        {
            Random random = new Random();
            return Math.Round(random.NextDouble() * (max - min) + min, 2);
        }


        #endregion



        #region 红包


        /// <summary>
        /// 发群红包
        /// </summary>
        /// <param name="money">红包金额</param>
        /// <param name="count">拆分个数</param>
        /// <param name="uid">用户id</param>
        /// <param name="tips">红包祝福语</param>
        /// <param name="remark">备注：如，社区红包</param>
        /// <returns></returns>
        public ResultMessageViewModel SendGiftGroup(double money, int count, Guid uid, string remark, string tips, string enPaypwd)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();

            if (money <= 0)
            {
                result.State = 1;
                result.Msg = "红包金额必须大于0";
                return result;
            }

            if (money > 200)
            {
                result.State = 1;
                result.Msg = "红包金额不能超过200元";
                return result;
            }

            using (DbContext db = new LinXinApp20Entities())
            {
                //发红包的人
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "用户不存在";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.冻结)
                {
                    result.State = 1;
                    result.Msg = "用户已被冻结";
                    return result;

                }

                //判断用户的金额是否充足
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);

                if (string.IsNullOrEmpty(userBalance.B_PayPwd) || string.IsNullOrEmpty(userBalance.B_EncryptCode))
                {
                    result.State = 2;
                    result.Msg = "您还未设置支付密码,请立即前去设置！";
                    return result;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + userBalance.B_EncryptCode);// 加密支付密码
                    if (!userBalance.B_PayPwd.Equals(payPwd))
                    {
                        result.Msg = "支付密码错误！";
                        return result;
                    }
                }

                if (userBalance.B_Balance < Convert.ToDecimal(money))  //余额不足
                {
                    result.State = 1;
                    result.Msg = "余额不足";
                    return result;
                }

                /*****以上发红包资质验证结束*****/

                Guid gId = Guid.NewGuid();
                DateTime curTime = DateTime.Now;

                //【1】扣除余额,修改会员余额；
                userBalance.B_Balance = userBalance.B_Balance - Convert.ToDecimal(money);
                db.Set<Core_User>().Attach(user);
                db.Entry<Core_User>(user).State = EntityState.Modified;

                //【2】添加会员账单表
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = remark,
                    B_Money = Convert.ToDecimal(-money),
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.红包,
                    B_OrderId = gId,
                    B_Type = (int)MemberRoleEnum.会员,
                    B_Flag = (int)BillFlagEnum.普通流水,
                    B_Status = 0
                };
                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//添加账单详细 不提交



                //【3】记录发送的红包信息
                Core_LuckyGift gift = new Core_LuckyGift()
                {
                    LG_Id = gId,
                    LG_Title = tips,
                    LG_Tags = "",
                    LG_Type = (int)LuckGiftTypeEnum.群红包,
                    LG_UserId = uid,
                    LG_ProjectId = user.U_BuildingId,
                    L_ProjectTitle = user.U_BuildingName,
                    LG_Money = Convert.ToDecimal(money),
                    LG_RemainMoney = Convert.ToDecimal(money),
                    LG_Count = count,
                    LG_RemainCount = count,
                    LG_CreateTime = curTime,
                    LG_Flag = null,//user.U_BuildingId,
                    LG_Remark = remark,
                    LG_Status = 0,
                    LG_AreaCode = user.U_AreaCode,
                    LG_VillageId = user.U_BuildingId
                };
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Added;//添加红包 不提交

                #region 拆红包算法
                /*
                //int temp = 0;
                //for (int i = 1; i <= count - 1; i++)
                //{
                //    Random ran = new Random();
                //    var giftmoney = ran.Next(1, (money * 100 - temp) / (count + 1 - i));
                //    temp += giftmoney;

                //    LuckyGiftDetail detail = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(giftmoney * 0.01), LD_State = 0, LD_Uid = 0 };
                //    db.Entry<LuckyGiftDetail>(detail).State = EntityState.Added;//添加红包详细 不提交
                //}
                ////添加最后一个红包
                //LuckyGiftDetail detailLast = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal((money * 100 - temp) * 0.01), LD_State = 0, LD_Uid = 0 };



                try
                {

                    double min = 0.01;//每个人最少能收到0.01元  
                    double t_max = 0, t_min = 0;
                    double safe_total, moneyTemp;

                    Random rand = new Random(DateTime.Now.Millisecond);
                    for (int i = 1; i < count; i++)
                    {
                        safe_total = (money - (count - i) * min) / (count - i);//随机安全上限  

                        double ztfb_u = money / count;//正态分布期望

                        moneyTemp = rand.Next((int)(min * 100), (int)(safe_total * 100)) / 100.0d;
                        if (money > t_max) t_max = money;
                        if (i == 1) t_min = t_max;
                        if (money < t_min) t_min = money;

                        money = money - moneyTemp;
                        LuckyGiftDetail detail = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(moneyTemp.ToString("0.00")), LD_State = 0, LD_Uid = 0 };
                        db.Entry<LuckyGiftDetail>(detail).State = EntityState.Added;//添加红包详细 不提交
                    }

                    //添加最后一个红包
                    LuckyGiftDetail detailLast = new LuckyGiftDetail() { LD_GiftId = gId, LD_Id = Guid.NewGuid(), LD_Money = Convert.ToDecimal(money.ToString("0.00")), LD_State = 0, LD_Uid = 0 };
                    db.Entry<LuckyGiftDetail>(detailLast).State = EntityState.Added;//添加红包详细 不提交

                    db.SaveChanges();//单元操作，批量提交


                    result.State = 0;
                    result.Msg = "小区红包已发成功";
                    result.Data = new { giftId = gId };
                    return result;
                }
                catch (Exception ex)
                {
                    result.State = 1;
                    result.Msg = "每个红包的金额略少哦";
                    return result;
                }
                 */
                #endregion


                bool bl = db.SaveChanges() > 0;//单元操作，批量提交
                if (bl)
                {
                    result.State = 0;
                    result.Msg = "ok";
                    result.Data = new { giftId = gId };
                }
            }

            return result;
        }


        /// <summary>
        /// 单发红包
        /// </summary>
        /// <param name="money">红包金额</param>
        /// <param name="uid">发红包的Id</param>
        /// <param name="tUid">接红包的会员Id( 环信id)</param>
        /// <param name="tips">红包备注</param>
        /// <returns></returns>
        public ResultMessageViewModel SendGiftSingle(decimal money, Guid uid, Guid tUid, string tips, string remark, string enPaypwd)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            if (money <= 0)
            {
                result.State = 1;
                result.Msg = "红包金额必须大于0";
                return result;
            }
            if (money > 200)
            {
                result.State = 1;
                result.Msg = "红包金额不能超过200元";
                return result;
            }

            Guid gId = Guid.NewGuid();
            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid);
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "用户帐号不存在";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.冻结)
                {
                    result.State = 1;
                    result.Msg = "用户帐号已被冻结";
                    return result;

                }

                //接红包用户
                var targetUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == tUid && o.U_Status != (int)UserStatusEnum.冻结); //收红包的会员
                if (targetUser == null)
                {
                    result.State = 1;
                    result.Msg = "收红包的用户帐号不存在";
                    return result;

                }
                //判断用户的金额是否充足
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);


                if (string.IsNullOrEmpty(userBalance.B_PayPwd) || string.IsNullOrEmpty(userBalance.B_EncryptCode))
                {
                    result.State = 2;
                    result.Msg = "您还未设置支付密码,请立即前去设置！";
                    return result;
                }
                else
                {
                    string payPwd = DESProvider.DecryptString(enPaypwd);  //支付密码
                    payPwd = Md5Extensions.MD5Encrypt(payPwd + userBalance.B_EncryptCode);// 加密支付密码
                    if (!userBalance.B_PayPwd.Equals(payPwd))
                    {
                        result.Msg = "支付密码错误！";
                        return result;
                    }
                }


                if (userBalance.B_Balance < Convert.ToDecimal(money))  //余额不足
                {
                    result.State = 1;
                    result.Msg = "余额不足";
                    return result;
                }
                Guid targetUid = targetUser.U_Id;  //目标用户id
                /*****以上发红包资质验证结束*****/

                DateTime curTime = DateTime.Now;

                //【1】扣除余额,修改会员余额；
                userBalance.B_Balance -= money;
                db.Set<Core_User>().Attach(user);
                db.Entry<Core_User>(user).State = EntityState.Modified;

                //【2】添加会员账单表
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = remark,
                    B_Money = Convert.ToDecimal(-money),
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.红包,
                    B_OrderId = gId,
                    B_Type = (int)MemberRoleEnum.会员,
                    B_Flag = (int)BillFlagEnum.普通流水,
                    B_Status = 0
                };
                db.Entry<Core_BillMember>(bill).State = EntityState.Added; //添加账单详细 不提交


                //添加 红包 和 详情
                Core_LuckyGift gift = new Core_LuckyGift()
                {
                    LG_Id = gId,
                    LG_Title = tips,
                    LG_Tags = "",
                    LG_Type = (int)LuckGiftTypeEnum.单个红包,
                    LG_UserId = uid,
                    LG_ProjectId = user.U_BuildingId,
                    L_ProjectTitle = user.U_BuildingName,
                    LG_Money = Convert.ToDecimal(money),
                    LG_RemainMoney = Convert.ToDecimal(money),
                    LG_Count = 1,
                    LG_RemainCount = 1,
                    LG_CreateTime = curTime,
                    LG_Flag = targetUid,   //收红包的用户ID
                    LG_Remark = remark,
                    LG_Status = 0,
                    LG_AreaCode = user.U_AreaCode,
                    LG_VillageId = user.U_BuildingId,

                };
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Added;//添加红包 不提交

                var bl = db.SaveChanges() > 0;//单元操作，批量提交
                if (bl)
                {
                    result.State = 0;
                    result.Msg = "ok";
                    result.Data = new { giftId = gId, uid = uid };
                }

            }

            return result;

        }





        /// <summary>
        ///  会员抢红包(群里)
        /// </summary>
        /// <param name="uid">会员Id</param>
        /// <param name="groupId">社区Id</param>
        /// <param name="giftId">红包Id</param>
        /// <param name="giftTips">红包Tips</param>
        /// <returns></returns>
        public ResultMessageViewModel ReceiveGiftGroup(Guid uid, Guid groupId, Guid giftId, string giftTips)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid); //抢红包的会员
                //审核会员状态
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "用户不存在";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.冻结)
                {
                    result.State = 1;
                    result.Msg = "用户帐号已被冻结";
                    return result;

                }

                if (user.U_BuildingId != groupId)
                {
                    result.State = 1;
                    result.Msg = "用户帐号不在此社区";
                    return result;
                }

                //分抢的红包
                var gift = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_Id == giftId);
                var sendUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == gift.LG_UserId);

                if (gift.LG_Status == (int)LuckGiftStateEnum.已过期)
                {
                    result.State = 1;
                    result.Msg = "已过期";
                    result.Data = new { money = 0, tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };
                    return result;
                }
                if (user.U_BuildingId != gift.LG_VillageId)
                {
                    result.State = 1;
                    result.Msg = "该用户不在此社区";
                    return result;
                }

                if (gift.LG_RemainCount < 1)
                {
                    result.State = 1;
                    result.Msg = "红包已被领光";
                    result.Data = new { money = 0, tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };

                    return result;
                }

                //抢红包记录，查看是否已领取过
                var giftHistory = db.Set<Core_LuckyGiftHistory>().FirstOrDefault(o => o.LH_GiftId == giftId && o.LH_UserId == uid);
                if (giftHistory != null)
                {
                    result.State = 2;
                    result.Msg = "已领取过啦";
                    result.Data =
                        new
                        {
                            money = giftHistory.LH_Money.ToString("N"),
                            tips = gift.LG_Title,
                            headImg = sendUser.U_Logo,
                            nickName = sendUser.U_NickName
                        };
                    return result;
                }

                DateTime curTime = DateTime.Now;

                /******有抢红包的资格******/
                //随机拆开一个红包

                decimal receiveMoney = 0;
                if (gift.LG_RemainCount == 1)  //只有一个红包，就直接返回
                {
                    receiveMoney = gift.LG_RemainMoney;
                }
                else
                {
                    receiveMoney = Convert.ToDecimal(CalcGift(Convert.ToDouble(gift.LG_RemainMoney), gift.LG_RemainCount));
                }

                Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                {
                    LH_Id = Guid.NewGuid(),
                    LH_UserId = uid,
                    LH_UserNickName = user.U_NickName,
                    LH_UserPhone = user.U_LoginPhone,
                    LH_CreateTime = curTime,
                    LH_Flag = (int)LuckGiftTypeEnum.群红包,
                    LH_GiftDetailId = null,
                    LH_GiftId = gift.LG_Id,
                    LH_Money = receiveMoney,
                    LH_Remark = giftTips,
                    LH_Status = 0,
                    LH_UserLogo = user.U_Logo
                };
                db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;// 【1】添加领取记录 不提交

                //【2】追加用户余额
                var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                userBalance.B_Balance += receiveMoney;
                db.Set<Core_Balance>().Attach(userBalance);
                db.Entry<Core_Balance>(userBalance).State = EntityState.Modified;

                //【3】添加会员账单表
                Core_BillMember bill = new Core_BillMember()
                {
                    B_Time = curTime,
                    B_Title = "抢得一个社区群红包",
                    B_Money = receiveMoney,
                    B_UId = user.U_Id,
                    B_Phone = user.U_LoginPhone,
                    B_Module = (int)BillEnum.红包,
                    B_OrderId = gift.LG_Id,
                    B_Type = (int)MemberRoleEnum.会员,
                    B_Flag = (int)BillFlagEnum.普通流水,
                    B_Status = 0
                };

                db.Entry<Core_BillMember>(bill).State = EntityState.Added;//添加账单详细 不提交


                //【4】领取一个红包后，减少红包的剩余数量,和剩余余额
                gift.LG_RemainCount -= 1;
                gift.LG_RemainMoney -= receiveMoney;
                db.Set<Core_LuckyGift>().Attach(gift);
                db.Entry<Core_LuckyGift>(gift).State = EntityState.Modified;
                //var cc = db.GetValidationErrors();
                bool bl = db.SaveChanges() > 0;//单元操作，批量提交

                if (bl)//数据操作成功
                {
                    //返回发红包会员的信息
                    var dataStr =
                        new
                        {
                            money = receiveMoney.ToString("N"),
                            tips = gift.LG_Title,
                            headImg = sendUser.U_Logo,
                            nickname = sendUser.U_NickName
                        };
                    result.State = 0;
                    result.Msg = "红包领取成功";
                    result.Data = dataStr;
                }
            }

            return result;
        }



        /// <summary>
        ///  会员收红包(单对单)
        /// </summary>
        /// <param name="uid">收红包会员的Id</param>
        /// <param name="giftId">红包Id</param>
        /// <param name="giftTips">红包Tips</param>
        /// <returns></returns>
        public ResultMessageViewModel ReceiveGiftSingle(Guid uid, Guid giftId, string giftTips)
        {
            ResultMessageViewModel result = new ResultMessageViewModel();

            using (DbContext db = new LinXinApp20Entities())
            {
                var user = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == uid); //抢红包的会员
                //审核会员状态
                if (user == null)
                {
                    result.State = 1;
                    result.Msg = "发红包的用户不存在";
                    return result;

                }
                if (user.U_Status == (int)UserStatusEnum.冻结)
                {
                    result.State = 1;
                    result.Msg = "用户已被冻结";
                    return result;

                }

                //抢红包详情
                //var receiveGift = db.Set<LuckyGiftDetail>().FirstOrDefault(o => o.LD_GiftId == giftId);
                //if (receiveGift == null)
                //{
                //    result.Status = 1;
                //    result.Msg = "红包不存在";
                //    return result;
                //}
                //else if (receiveGift.LD_Status != (int)GiftStatusEnum.待领取)
                //{
                //    result.Status = 1;
                //    result.Msg = "红包已领取";
                //    result.Data = new { money = receiveGift.LD_Money, tips = gift.LG_Title };
                //    return result;
                //}

                //收红包
                var gift = db.Set<Core_LuckyGift>().FirstOrDefault(o => o.LG_Id == giftId); 
                //返回发红包会员的信息
                var sendUser = db.Set<Core_User>().FirstOrDefault(o => o.U_Id == gift.LG_UserId);

                if (gift != null && gift.LG_RemainCount > 0)
                {

                    if (gift.LG_UserId == uid)  //自己领自己的红包，提示“红包已发出”
                    {
                        result.State = 1;
                        result.Msg = "红包已发出";
                        result.Data =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        return result;

                    }

                    if ( gift.LG_Type != (int)LuckGiftTypeEnum.单个红包 || gift.LG_Flag != uid)
                    {
                        result.State = 1;
                        result.Msg = "红包已过期";//红包不存在，提示改成“已过期”
                        return result;
                    }

                    if (gift.LG_Status == (int)LuckGiftStateEnum.已过期)
                    {
                        result.State = 1;
                        result.Msg = "已过期";
                        result.Data =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        return result;
                    }

                    DateTime curTime = DateTime.Now;

                    /******有收红包的资格******/
                    Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                    {
                        LH_Id = Guid.NewGuid(),
                        LH_UserId = uid,
                        LH_UserNickName = user.U_NickName,
                        LH_UserPhone = user.U_LoginPhone,
                        LH_CreateTime = curTime,
                        LH_Flag = (int)LuckGiftTypeEnum.单个红包,
                        LH_GiftDetailId = null,
                        LH_GiftId = gift.LG_Id,
                        LH_Money = gift.LG_Money,
                        LH_Remark = giftTips,
                        LH_Status = 0,
                        LH_UserLogo = user.U_Logo
                    };


                    db.Entry<Core_LuckyGiftHistory>(history).State = EntityState.Added;// 【1】添加领取记录 不提交

                    //【2】追加用户余额
                    var userBalance = db.Set<Core_Balance>().FirstOrDefault(o => o.B_AccountId == uid);
                    userBalance.B_Balance += gift.LG_Money;
                    db.Set<Core_Balance>().Attach(userBalance);
                    db.Entry<Core_Balance>(userBalance).State = EntityState.Modified;

                    //【3】添加会员账单表
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = curTime,
                        B_Title = string.Format("收到邻友：{0} 的聊天红包", sendUser.U_NickName),
                        B_Money = gift.LG_Money,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.红包,
                        B_OrderId = gift.LG_Id,
                        B_Type = (int)MemberRoleEnum.会员,
                        B_Flag = (int)BillFlagEnum.普通流水,
                        B_Status = 0
                    };

                    db.Entry<Core_BillMember>(bill).State = EntityState.Added;//添加账单详细 不提交


                    //【4】领取一个红包后，减少红包的剩余数量和剩余余额
                    gift.LG_RemainCount = 0;
                    gift.LG_RemainMoney = 0;
                    db.Set<Core_LuckyGift>().Attach(gift);
                    db.Entry<Core_LuckyGift>(gift).State = EntityState.Modified;

                    bool bl = db.SaveChanges() > 0;//单元操作，批量提交

                    if (bl)//数据操作成功
                    {
                        //返回发红包会员的信息
                        var dataStr =
                            new
                            {
                                money = gift.LG_Money.ToString("N"),
                                tips = gift.LG_Title,
                                headImg = sendUser.U_Logo,
                                nickName = sendUser.U_NickName
                            };
                        result.State = 0;
                        result.Msg = "红包领取成功";
                        result.Data = dataStr;
                    }
                }
                else
                {
                    result.State = 1;
                    result.Msg = "红包已领取";
                    result.Data = new { money = gift.LG_Money.ToString("N"), tips = gift.LG_Title, headImg = sendUser.U_Logo, nickName = sendUser.U_NickName };
                    return result;
                }
            }

            return result;
        }

        #endregion

    }
}
