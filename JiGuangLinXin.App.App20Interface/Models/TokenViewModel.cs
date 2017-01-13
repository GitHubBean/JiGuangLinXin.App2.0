using System;

namespace JiGuangLinXin.App.App20Interface.Models
{
    /// <summary>
    /// 客户端请求，header 包含的token
    /// </summary>
    public class TokenViewModel
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 会员手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 平台标识
        /// </summary>
        public int Platform { get; set; }
        /// <summary>
        /// token 日期
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 区域码
        /// </summary>
        public string AreaCode { get; set; }

    }
}