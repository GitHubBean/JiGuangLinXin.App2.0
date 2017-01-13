using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using JiguangLinXin.App.SpecialEvent.Models;

namespace JiguangLinXin.App.SpecialEvent.Core
{
    public class VillageVoteCore : BaseRepository<Core_VillageVote>
    {
        /// <summary>
        /// 发起投票
        /// </summary>
        /// <param name="buildingId">投票的小区ID</param>
        /// <param name="ip">投票人的IP</param>
        /// <returns></returns>
        public bool Send(Guid buildingId, string ip)
        {


            using (DbContext db = new SpecialEventEntities())
            {
                var vill = db.Set<Core_Village>().FirstOrDefault(p => p.V_Id == buildingId);
                if (vill != null)
                {
                    vill.V_Votes += 1;
                    db.Set<Core_Village>().Attach(vill);
                    db.Entry(vill).State = EntityState.Modified;  //1累计投票


                    Core_VillageVote obj = new Core_VillageVote()
                    {
                        C_BuildingId = buildingId,
                        C_Id = Guid.NewGuid(),
                        C_Ip = ip,
                        C_Source  = 0,
                        C_Time  = DateTime.Now
                    };

                    db.Entry(obj).State = EntityState.Added;   //2新增记录




                    var vs = db.Set<Core_Statistics>().FirstOrDefault();
                    vs.S_Votes += 1;

                    db.Set<Core_Statistics>().Attach(vs);
                    db.Entry(vs).State = EntityState.Modified;  //3累计投票

                    return db.SaveChanges() > 0;

                }
            }

            return false;
        }
    }
}