using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace JiGuangLinXin.App.Entities
{
    /// <summary>
    /// 极光推送的实体
    /// </summary>
    public class JPushMsgModel
    {
        /// <summary>
        /// 推送到的位置编码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 推送的标题（列表中显示）
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 消息备注（内容详情中显示）
        /// </summary>
        public string tips { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string uid { get; set; }
       /// <summary>
       /// 用户LOGO
       /// </summary>
        public string logo { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 0默认  1.好友推送 （客户端接受到推送跳转到我的好友列表）  2 点击昵称跳转到用户详情 3点击昵称跳转到社区服务详情 4点击昵称跳转到精品汇详情
        /// </summary>
        public int proFlag { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public string proId { get; set; }
        /// <summary>
        /// 项目name
        /// </summary>
        public string proName { get; set; }
        /// <summary>
        /// 评论（公告）时间
        /// </summary>
        public string proTime { get; set; }
        /// <summary>
        /// 推送时间
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 标签  邻妹妹 标题"
        /// </summary>
        public string tags { get; set; }

        public JPushMsgModel()
        {
            code = 1006;
            title = "";
            tips = "";
            uid = "";
            logo = "";
            nickname = "";
            proFlag = 0;
            proId = "";
            proName = "";
            proTime = "";
            time = "";
            tags = "";
        }

    }
}
