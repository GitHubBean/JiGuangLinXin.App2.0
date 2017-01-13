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
        /// ��ȡ����С�����б�
        /// </summary>
        /// <param name="cityName">������</param>
        /// <param name="villageName">С������ģ��</param>
        /// <param name="rows">Ĭ������</param>
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
