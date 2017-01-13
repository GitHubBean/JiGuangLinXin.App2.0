using System;

namespace JiGuangLinXin.App.Entities.ViewModel
{
    public class MallGoodsViewModel
    {
        public long rownumber { get; set; }

        public Guid B_Id { get; set; }

        public string B_NickName { get; set; }

        public Guid G_Id { get; set; }

        public string G_Name { get; set; }

        public string G_Img { get; set; }


        public string T_Title { get; set; }


        public string G_Tags { get; set; }



        /// <summary>
        /// 图片（附件）列表
        /// </summary>
        public dynamic attachmentList { get; set; }
    }
}
