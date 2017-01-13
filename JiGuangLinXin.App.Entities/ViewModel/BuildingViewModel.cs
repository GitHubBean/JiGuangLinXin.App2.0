using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 新家推荐 视图模型
    /// </summary>
    public class BuildingViewModel
    {
        /// <summary>
        /// 景观图片
        /// </summary>
        public string landscape { get; set; }

        /// <summary>
        /// 区位展示
        /// </summary>
        public string location { get; set; }

        /// <summary>
        /// 物业配套
        /// </summary>
        public string property { get; set; }

        /// <summary>
        /// 建筑规划
        /// </summary>
        public string planning { get; set; }
    }
}
