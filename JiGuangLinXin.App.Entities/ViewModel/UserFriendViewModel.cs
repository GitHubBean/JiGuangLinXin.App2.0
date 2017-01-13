using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 用户好友（邻友）
    /// </summary>
    public class UserFriendViewModel
    {

        public Guid UId { get; set; }
        public string Logo { get; set; }

        public string NikeName { get; set; }
        public string Phone { get; set; }
        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }
        public string CityName { get; set; }

        public int Age { get; set; }

        public int Sex { get; set; }
        public string Huanxin { get; set; }


        public int State { get; set; }

        public int ManagerFlag { get; set; }
    }
}
