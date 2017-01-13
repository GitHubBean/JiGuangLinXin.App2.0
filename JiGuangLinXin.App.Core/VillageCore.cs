using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Linq.Dynamic;
using JiGuangLinXin.App.Entities;
namespace JiGuangLinXin.App.Core
{
	public class  VillageCore:BaseRepository<Core_Village>
	{

        /// <summary>
        /// 获取热门小区的列表
        /// </summary>
        /// <param name="cityName">城市名</param>
        /// <param name="villageName">小区名，模糊</param>
        /// <param name="rows">默认条数</param>
        /// <returns></returns>
	    public List<Core_Village> GetHotList(string cityName , string villageName,int rows =10)
        {

            var list =
                base.LoadEntities(o => o.V_CityName.Contains(cityName) && o.V_BuildingName.Contains(villageName))
                    .OrderByDescending(o => o.V_Hot).ThenByDescending(o=>o.V_Number)
                    .Take(rows).ToList();



	        return list;
	    }

        
	}
}
