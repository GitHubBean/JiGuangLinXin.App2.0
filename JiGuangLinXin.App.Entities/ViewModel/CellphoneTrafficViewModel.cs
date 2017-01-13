using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{
    public class CellphoneTrafficViewModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int TraId { get; set; }
        /// <summary>
        /// 兑换的积分数量
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 对应的流量
        /// </summary>
        public string Traffic { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 每天限定数量
        /// </summary>
        public int LimitCount { get; set; }
    }
}
