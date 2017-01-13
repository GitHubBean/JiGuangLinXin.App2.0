using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using JiGuangLinXin.App.Entities;
namespace JiGuangLinXin.App.Core
{
    /// <summary>
    /// 购物车
    /// </summary>
    public class MalShoppingCarCore : BaseRepository<Core_MalShoppingCar>
    {
        /// <summary>
        /// /查询用户的购物车
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public dynamic ShoppingCarByUserId(Guid uid)
        {

            //using (DbContext db = new LinXinApp20Entities())
            //{
            //    db.Set<Core_MalShoppingCar>().Where(o => o.S_UId == uid).Join(db.Set<Core_Business>(), a => a.S_BusId, b => b.B_Id, (a, b) => new
            //    {
            //        a.S_BusId,
            //        a.S_BusName,
            //        a.S_GodosImg,
            //        a.S_Id,
            //        a.S_GoodsId,
            //        a.S_GoodsName,
            //        a.S_GoodsCount,
            //        a.S_Price,
            //        a.S_Time,
            //        a.S_Remark,
            //        b.B_

            //    });
            //}

            return null;
        }
    }
}
