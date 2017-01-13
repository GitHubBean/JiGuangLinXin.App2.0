using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class MessageCenterCore:BaseRepository<Sys_MessageCenter>
    {
        
        /// <summary>
        /// 查询附件的人
        /// </summary>
        /// <param name="lng">经度</param>
        /// <param name="lat">纬度</param>
        /// <param name="radius">半径（距离）</param>
        /// <returns></returns>
        public List<NearbyLinyouViewModel> QueryNearbyLinyou(float lng, float lat, int radius)
        {

            //todo: 默认半径 9000000 米
            string sql = "exec proc_calc_nearby_linyou @lng,@lat, @radius";
            //string sql = "exec proc_calc_nearby_linyou '106.49914265705948','29.627831532645523', 2000";

            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

               return db.Database.SqlQuery<NearbyLinyouViewModel>(sql, new SqlParameter("@lng", lng), new SqlParameter("@lat", lat),
               new SqlParameter("@radius", 9000000)).ToList();

                //return db.Database.SqlQuery<NearbyLinyouViewModel>(sql).ToList();
                
            } 

            return null;
        }
    }
}
