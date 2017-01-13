using System;
using System.Linq.Dynamic;
using JiGuangLinXin.App.Entities;
namespace JiGuangLinXin.App.Core
{
	public class  LikesCore:BaseRepository<Core_Likes>
	{

        /// <summary>
        /// �Ƿ��Ѿ����޹�
        /// </summary>
        /// <param name="proId">��Ŀ ID</param>
        /// <param name="uid">�û�id</param>
        /// <returns></returns>
	    public bool IsExist(Guid proId,Guid uid)
        {
            return base.LoadEntities(o => o.L_ProjectId == proId && uid == o.L_UserId).Any();
	    }
	}
}
