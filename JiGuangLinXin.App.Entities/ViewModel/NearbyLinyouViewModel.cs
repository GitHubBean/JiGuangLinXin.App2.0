using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiGuangLinXin.App.Entities.ViewModel
{

    /// <summary>
    /// 附近的人
    /// </summary>
    public class NearbyLinyouViewModel
    {
        public Guid userId { get; set; }
        public string cityName { get; set; }
        public string huanxinId { get; set; }
        public string nickname { get; set; }
        public string logo { get; set; }
        public int sex { get; set; }
        public int age { get; set; }
        public Guid buildingId { get; set; }
        public string buildingName { get; set; }
        public int auditingMgr { get; set; }
        public double distance { get; set; }
    }
}
