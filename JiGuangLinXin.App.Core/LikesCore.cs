using System;
using System.Linq.Dynamic;
using JiGuangLinXin.App.Entities;
namespace JiGuangLinXin.App.Core
{
	public class  LikesCore:BaseRepository<Core_Likes>
	{

        /// <summary>
        /// 是否已经点赞过
        /// </summary>
        /// <param name="proId">项目 ID</param>
        /// <param name="uid">用户id</param>
        /// <returns></returns>
	    public bool IsExist(Guid proId,Guid uid)
        {
            return base.LoadEntities(o => o.L_ProjectId == proId && uid == o.L_UserId).Any();
	    }
	}
}
