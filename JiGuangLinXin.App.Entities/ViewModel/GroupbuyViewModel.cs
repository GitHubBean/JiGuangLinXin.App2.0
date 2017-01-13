using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{
    /// <summary>
    /// 自家社区的邻里团
    /// </summary>
    public class GroupbuyViewModel
    {
        public long rownumber { get; set; }
        public Guid GB_Id { get; set; }
        public int GB_Top { get; set; }
        public int GB_Sales { get; set; }
        public int GB_PeopleCount { get; set; }
        public string GB_Titlte { get; set; }
        public string GB_CoverImg { get; set; }
        public decimal GB_Price { get; set; }
        public decimal GB_PriceOld { get; set; }
        public Guid GB_BusId { get; set; }
        public string GB_BusName { get; set; }
        public DateTime GB_STime { get; set; }
        public DateTime GB_ETime { get; set; }
        public DateTime GB_Time { get; set; }

    }
}
