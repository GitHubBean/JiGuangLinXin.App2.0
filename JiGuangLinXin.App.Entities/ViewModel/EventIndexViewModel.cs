using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 邻友圈 ，首页  活动列表
    /// </summary>
    public class EventIndexViewModel
    {
        public System.Guid S_Id { get; set; }
        public System.Guid S_BusId { get; set; }
        public string S_BusName { get; set; }
        public int S_BusRole { get; set; }
        public string S_Phone { get; set; }
        public string S_Title { get; set; }
        public string S_ImgTop { get; set; }
        public int S_Flag { get; set; }
        public string S_Video { get; set; }
        public string S_Img { get; set; }
        public string S_Content { get; set; }
        public string S_Tags { get; set; }
        public Nullable<System.DateTime> S_STime { get; set; }
        public Nullable<System.DateTime> S_ETime { get; set; }
        public string S_Remark { get; set; }
        public int S_Likes { get; set; }
        public int S_Comments { get; set; }
        public int S_Clicks { get; set; }
        public int S_Rank { get; set; }
        public int S_Recom { get; set; }
        public int S_Top { get; set; }
        public int S_Status { get; set; }
        public string S_Desc { get; set; }
        public string S_Address { get; set; }
        public string S_LinkPhone { get; set; }
        public System.DateTime S_Date { get; set; }
        public string S_TargetUrl { get; set; }
        public string S_GoodsName { get; set; }
        public Nullable<System.Guid> S_GoodsId { get; set; }
        public Nullable<System.Guid> S_BuildingId { get; set; }
        public string S_BuildingName { get; set; }
        public int S_Target { get; set; }
        public string S_BusLogo { get; set; }

        public Guid targetId { get; set; }

        /// <summary>
        /// 点赞列表
        /// </summary>
        public dynamic LikesList { get; set; }


        /// <summary>
        /// 评论列表
        /// </summary>
        public dynamic CommentsList { get; set; }

    }
}
