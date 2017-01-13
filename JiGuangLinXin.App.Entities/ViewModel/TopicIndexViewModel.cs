using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 邻友圈，首页  话题列表
    /// </summary>
    public class TopicIndexViewModel
    {
        public long rownumber { get; set; }
        //public int topicTag { get; set; }
        public Guid T_Id { get; set; }
        public Guid T_UserId { get; set; }
        public string T_Title { get; set; }
        public int T_Typle { get; set; }
        public int T_Hongbao { get; set; }
        public int T_Ticket { get; set; }
        public string T_Img { get; set; }


        public int T_ImgAttaCount { get; set; }

        public string T_Tags { get; set; }
        public int T_Clicks { get; set; }

        public int T_Likes { get; set; }

        public int T_Comments { get; set; }

        public DateTime T_Date { get; set; }
        public string U_Logo { get; set; }
        public string U_NickName { get; set; }

        public int U_Sex { get; set; }

        public int U_Age { get; set; }
        public string U_City { get; set; }
        public string U_BuildingName { get; set; }

        /// <summary>
        /// 点赞列表
        /// </summary>
        public List<LikeIndexViewModel> LikesList { get; set; }

        /// <summary>
        /// 红包列表
        /// </summary>
        public List<HongbaoIndexViewModel> HongbaoList { get; set; }

        /// <summary>
        /// 评论列表
        /// </summary>
        public List<CommentsIndexViewModel> CommentsList { get; set; }



        /// <summary>
        /// 图片（附件）列表
        /// </summary>
        public dynamic attachmentList { get; set; }

    }

    public class LikeIndexViewModel
    {
        public Guid L_Id { get; set; }

        public Guid L_UserId { get; set; }
        public string U_Logo { get; set; }
        public string U_NickName { get; set; }
        public DateTime L_Time { get; set; }
        
    }
    public class HongbaoIndexViewModel
    {

        public Guid U_Id { get; set; }

        public string U_Logo { get; set; }
        public decimal LH_Money { get; set; }
    }
    public class CommentsIndexViewModel
    {
        public Guid C_Id { get; set; }

        public Guid C_UserId { get; set; }
        public string  C_UserName { get; set; }
        public Guid C_RefId { get; set; }
        public string C_RefName { get; set; }
        public string C_Content { get; set; }
    }

    public class attachmentIndexViewModel
    {
    }
}
