using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using JiguangLinXin.App.SpecialEvent.Models;

namespace JiguangLinXin.App.SpecialEvent.Core
{
    public class VillageCore : BaseRepository<Core_Village>
    {
        public dynamic GetBusinessServiceByBuildingId(Guid buildingId)
        {
            string sql = @"select a.B_Id as busId,a.B_ServiceImg as coverImg,a.B_NickName as nickname from Core_Business as a
                            inner join Core_BusinessVillage as b
                            on a.B_Id = b.BV_BusinessId
                            where b.BV_VillageId = '" + buildingId + "'";

            using (DbContext db = new SpecialEventEntities())
            {

                //var cc = db.Database.SqlQuery<object>(string.Format(sql, where, rows, (pn - 1) * rows)).ToList();
                var rs =
                    db.Database.SqlQuery<dynamic>(sql).Select(o => new
                    {
                        o.busId,
                        o.coverImg,
                        o.nickname
                    }).ToList();
                return rs;
            }
            return null;
        }
    }
}