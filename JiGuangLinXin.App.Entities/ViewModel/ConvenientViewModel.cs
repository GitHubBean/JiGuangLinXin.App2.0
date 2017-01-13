using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{
    /// <summary>
    /// 便民服务结果
    /// </summary>
    public class ConvenientViewModel
    {
        /// <summary>
        /// 用户号（缴费帐号）
        /// </summary>
        public string AccountNo { get; set; }
        /// <summary>
        /// 收款（收款方）
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public string OrderDate { get; set; }

        //支付方式
        public string PayWay { get; set; }
    }
}
