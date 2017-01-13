using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ServicesModel;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class MallGoodsCore : BaseRepository<Core_MallGoods>
    {

        /// <summary>
        /// 精品购，首页
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <param name="cateType">分类ID</param>
        /// <param name="orderByFlag">排序字段</param>
        /// <returns></returns>
        public List<MallGoodsViewModel> QueryGoods(Guid buildingId, int pn=0, int rows = 10, int cateType = -1, string orderByFlag = "")
        {
            string orderBy = "c.G_Top desc ";
            string cateTypeStr = " 1=1 ";
            string sql = @"select top {1} * from (
                              select row_number() over(order by {4},c.G_Time desc) as rownumber,a.B_Id, a.B_NickName, c.G_Id, c.G_Name, c.G_Img, d.T_Title, c.G_Tags
                               FROM dbo.Core_MallGoods c
                            inner JOIN dbo.Core_MallType d
                            on c.G_TypeId  = d.T_Id
                            inner JOIN dbo.Core_Business a
                            on a.B_Id=c.G_BusId
                            inner   JOIN dbo.Core_BusinessVillage b
                            on a.B_Id = b.BV_BusinessId 
                            where  c.G_Status=0 and  c.G_AuditingState=1 and a.B_Status =0 and b.BV_VillageId='{0}' and {3}
                            ) C
                            where rownumber > {2}";
            if (!string.IsNullOrEmpty(orderByFlag)) //按照销量排序
            {
                orderBy = orderByFlag; //"c.G_Sales desc ";
            }
            if (cateType > 0)
            {
                cateTypeStr = " c.G_TypeId=" + cateType;
            }

            using (DbContext db = new LinXinApp20Entities())
            {

                //db.Set<Core_Business>()
                //    .Join(db.Set<Core_BusinessVillage>(), b => b.B_Id, v => v.BV_BusinessId, (b, v) => new
                //    {
                //        b.B_Id,
                //        b.B_NickName,
                //        b.B_Category,
                //        v.BV_VillageId
                //    }).Join(db.Set<Core_MallType>(),a=>a.B_Category,b=>b.T_Id,(a,b)=>new
                //    {

                //    }).Where(o => o.BV_VillageId == Guid.NewGuid()).ToList();



                var rs = db.Database.SqlQuery<MallGoodsViewModel>(string.Format(sql, buildingId, rows, pn * rows, cateTypeStr, orderBy)).ToList();

                if (rs.Any())
                {
                    AttachmentCore attCore = new AttachmentCore();
                    foreach (var item in rs)
                    {
                        //查询附件
                        item.attachmentList =
                                attCore.LoadEntities(p => p.A_PId == item.G_Id).OrderBy(o => o.A_Rank).ToList().Select(o => new
                                {
                                    imgUrl = string.Format("{0}/{1}", o.A_Folder, o.A_FileName.Replace("_2","_3"))  //附件里面存储的是中尺寸的图片，需要调整成最小尺寸输出
                                });
                    }
                    return rs;
                }
            }
            return new List<MallGoodsViewModel>();
        }


        /// <summary>
        /// 查询精品购数量
        /// </summary>
        /// <param name="buildingId">小区ID</param>
        /// <param name="businessId">商家ID</param>
        /// <returns></returns>
        public int CountGoods(Guid buildingId, Guid businessId)
        {
            string sql = @" select COUNT(1)   FROM dbo.Core_MallGoods c
inner JOIN dbo.Core_MallType d
on c.G_TypeId  = d.T_Id
inner JOIN dbo.Core_Business a
on a.B_Id=c.G_BusId
inner   JOIN dbo.Core_BusinessVillage b
on a.B_Id = b.BV_BusinessId 
where  c.G_Status=0 and c.G_AuditingState=1 and a.B_Status =0 {0}";

            using (DbContext db = new LinXinApp20Entities())
            {
                if (businessId != Guid.Empty)
                {
                    sql = string.Format(sql, "and b.BV_BusinessId = @businessId");
                    return db.Database.SqlQuery<int>(sql, new SqlParameter("@businessId", businessId)).FirstOrDefault();
                }
                if (buildingId != Guid.Empty)
                {
                    sql = string.Format(sql, "and b.BV_VillageId = @buildingId");
                    return db.Database.SqlQuery<int>(sql, new SqlParameter("@buildingId", buildingId)).FirstOrDefault();
                }

                sql = string.Format(sql, " and 1=1");
                return db.Database.SqlQuery<int>(sql).FirstOrDefault();

            }

            //return    base.ExecuteStoreCommand(sql, new SqlParameter("@buildingId", buildingId)); 


            //Guid b2 = Guid.Parse("4d191025-f7b3-4e42-912c-b4cc9cf69069");

            return 0;
        }


        #region 商家后台管理系统
        /// <summary>
        /// 添加一个商品
        /// </summary>
        /// <param name="goods">商品信息</param>
        /// <param name="images">商品图片</param>
        /// <returns></returns>
        public bool AddOneGoods(Core_MallGoods goods,IEnumerable<dynamic> images )
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                db.Core_MallGoods.Add(goods);
                if (images != null && images.Any())
                {
                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = goods.G_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = goods.G_Time;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }
        /// <summary>
        /// 编辑精品汇 
        /// </summary>
        /// <param name="et">商品信息</param>
        /// <param name="images">图片</param>
        /// <returns></returns>
        public bool EditOneGoods(Core_MallGoods et, IEnumerable<dynamic> images)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                db.Set<Core_MallGoods>().Attach(et);
                db.Entry(et).State = EntityState.Modified;  //1修改活动

                 
                //2活动图片附件，如果修改，必须删除之前配置的活动图片
                if (images != null && images.Any())
                {
                    db.Sys_Attachment.Delete(o => o.A_PId == et.G_Id);  //批量删除，物理

                    foreach (var img in images)
                    {
                        Sys_Attachment am = new Sys_Attachment();

                        am.A_Id = Guid.NewGuid();
                        am.A_PId = et.G_Id;
                        am.A_Type = (int)AttachmentTypeEnum.图片;
                        am.A_Time = et.G_Time;

                        am.A_FileNameOld = img.A_FileNameOld;
                        am.A_FileName = img.A_FileName;
                        am.A_Size = img.A_Size;
                        am.A_Folder = img.A_Folder;
                        am.A_Rank = img.A_Rank;

                        //添加项目附件
                        db.Sys_Attachment.Add(am);
                    }
                }
                return db.SaveChanges() > 0;
            }
            return false;
        }

        /// <summary>
        /// 商家的所有的商品
        /// </summary>
        /// <param name="busId">商家ID</param>
        /// <param name="audState">认证状态</param>
        /// <param name="state">商品状态</param>
        /// <returns></returns>
        public List<dynamic> AllGoodsByBusId(Guid busId,int audState,int state)
        {

            using (DbContext db = new LinXinApp20Entities())
            {

                var list  = db.Set<Core_MallGoods>()
                    .Join(db.Set<Core_MallType>(), a => a.G_TypeId, b => b.T_Id, (a, b) => new
                    {
                        a.G_Name,
                        a.G_Sales,
                        a.G_Price,
                        a.G_ExtraFee,
                        a.G_Status,
                        a.G_AuditingState,
                        b.T_Title,
                        a.G_Time

                    });
                if (audState>0)
                {
                    list = list.Where(o => o.G_AuditingState == audState);
                }
                if (state>0)
                {
                    list = list.Where(o => o.G_Status == state);
                }

            }
            return null;
        }

        #endregion
    }
}
