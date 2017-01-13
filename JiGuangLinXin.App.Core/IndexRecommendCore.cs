using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JiGuangLinXin.App.Entities;
namespace JiGuangLinXin.App.Core
{
    public class IndexRecommendCore : BaseRepository<Sys_IndexRecommend>
    {

        /// <summary>
        /// 获得首页推荐的内容
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <param name="typeId">1社区服务 2便民购 3活动中心（社区活动、邻里团）</param>
        /// <param name="rows">多少条数据</param>
        /// <returns></returns>
        public List<Sys_IndexRecommend> GetProject(Guid buildingId, int typeId, int rows)
        {
            List<Sys_IndexRecommend> list = new List<Sys_IndexRecommend>();

            if (buildingId != Guid.Empty)
            {
                string sql = @"select top {0} a.* from Sys_IndexRecommend a
                                inner join Core_BusinessVillage b
                                on a.R_BusId = b.BV_BusinessId
                                inner join Core_Business c
                                on a.R_BusId=c.B_Id
                                where a.R_State=0  and c.B_Status=0 and a.R_Target=1 and b.BV_VillageId = '{1}' and a.R_Type ={2}
                                order by a.R_Rank ";
                list = base.ExecuteStoreQuery(string.Format(sql,rows,buildingId,typeId)).ToList();

                if (list.Count < rows)  //小区定向推荐数量不够，再查询全平台 推广
                {
                    var list2 =
                        base.LoadEntities(o => o.R_State == 0 && o.R_Target == 0 && o.R_Type == typeId)
                            .OrderBy(o => o.R_Rank)
                            .Take(rows - list.Count).ToList();
                    list = list.Concat(list2).ToList();  //链接两个集合
                }
            }
            return list;
        }




    }
}
