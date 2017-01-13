using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ServicesModel
{
    /// <summary>
    /// 银联查询要素 实体
    /// </summary>
    public class ChinapayModel
    {
        /*-----账单查询----*/
        /// <summary>
        /// 渠道号（chinapay提供）
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// 出账机构号
        /// </summary>
        public string MerSysId { get; set; }

        /// <summary>
        /// 查询帐号
        /// </summary>
        public string QueryNo { get; set; }

        /// <summary>
        /// 账单类型:条形码,合同号,设备号00：合同号，01：设备号,02:条形码
        /// </summary>
        public string BillType { get; set; }


        /// <summary>
        /// 缴费状态【00】未缴费 【01】已缴费【02】全部
        /// </summary>
        public string MerBillStat { get; set; }

        /// <summary>
        /// 查询账期
        /// </summary>
        public string BillDateQuery { get; set; }

        /// <summary>
        /// 账单密码
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        /// 查询请求的特殊要求
        /// </summary>
        public string IndividualAreaQuery { get; set; }

        /// <summary>
        /// 销账类型主要区别是充值交易还是销账交易 00:充值,01:销账
        /// </summary>
        public string WriteOffType { get; set; }


        /*------请求返回数据，发起下单-----*/
        /// <summary>
        /// 请求方交易流水号
        /// </summary>
        public string ReqSeqIdTrans { get; set; }

        /// <summary>
        /// 请求方交易日期
        /// </summary>
        public string ReqTransDate { get; set; }

        /// <summary>
        /// 用户号，与branchId 一致
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 账单号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 账单金额
        /// </summary>
        public string BillAmt { get; set; }
        /// <summary>
        /// 账单日期
        /// </summary>
        public string BillDateOrder { get; set; }

        /// <summary>
        /// 账单超次
        /// </summary>
        public string RecordTimes { get; set; }

        /// <summary>
        /// 下单请求的特殊要求
        /// </summary>
        public string IndividualAreaOrder { get; set; }


        /*----下单返回数据，发起销帐申请-----*/
        /// <summary>
        /// 交易日期
        /// </summary>
        public string TransDate { get; set; }

        /// <summary>
        /// 交易id
        /// </summary>
        public string OrdId { get; set; }
        /// <summary>
        /// 响应码，具体参见响应码表
        /// </summary>
        public string RespCode { get; set; }

        /// <summary>
        /// 账单信息
        /// </summary>
        public string BillInfo { get; set; }

        /// <summary>
        /// 保留域（支付账单日期【Substring(6, 8)】+支付流水号【Substring(0, 6)】） 如：
        /// </summary>
        public string Resv { get; set; }

        /// <summary>
        /// MD5key  （银联提供）
        /// </summary>
        public string MD5Key { get; set; }

        /// <summary>
        /// 便民流水号
        /// </summary>
        public string SeqId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId{ get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// 订单状态 000：支付成功；002：支付失败；111：未支付221：待退款；222：已退款；991：退款失败
        /// </summary>
        public string OrderStat{get;set ;}

        /// <summary>
        /// 账单状态，便民平台账单状态0：未支付1：销账成功2：销账失败3：销账未知
        /// </summary>
        public string BillStat { get; set; }


    }
}
