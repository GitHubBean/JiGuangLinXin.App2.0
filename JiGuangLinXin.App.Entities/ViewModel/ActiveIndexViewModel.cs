using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 邻友圈 ，首页  活动列表
    /// </summary>
    public class ActiveIndexViewModel
    {
        public long rownumber { get; set; }
        //public int topicTag { get; set; }
        public Guid I_Id { get; set; }
        public Guid I_UserId { get; set; }
        public string I_Title { get; set; }
        public string I_Remark { get; set; }

        public int I_Flag { get; set; }
        public string I_Img { get; set; }

        public int I_Hongbao { get; set; }
        public DateTime I_Date { get; set; }

        public int I_Likes { get; set; }
        public int I_Comments { get; set; }


        public int I_Type { get; set; }
        public string I_Tags { get; set; }
        public string U_NickName { get; set; }
        public string U_Logo { get; set; }
        public int U_Sex { get; set; }

        /// <summary>
        /// 点赞列表
        /// </summary>
        public dynamic LikesList { get; set; }


        /// <summary>
        /// 评论列表
        /// </summary>
        public dynamic CommentsList { get; set; }

        /// <summary>
        /// 红包列表
        /// </summary>
        public dynamic HongbaoList { get; set; }

    }
}
