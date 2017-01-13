using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using EntityFramework.Extensions;  
namespace JiGuangLinXin.App.Entities
{
    public partial class BaseRepository<T> where T : class
    {

        //获取当前线程内部的上下文实例
        private DbContext db = EFContextFactory.GetCurrentDbContext();

        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T AddEntity(T entity)
        {
            db.Entry<T>(entity).State = EntityState.Added;
            if (db.SaveChanges()>0)
            {
                return entity;   
            }
            return null;
        }

        /// <summary>
        /// 添加实体 不提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual T AddEntityNoSave(T entity)
        {
            db.Entry<T>(entity).State = EntityState.Added;
            return entity;
        }

        /// <summary>
        /// 同时增加多条数据到一张表
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool AddEntities(List<T> entitys)
        {
            foreach (var entity in entitys)
            {
                db.Entry<T>(entity).State = EntityState.Added;
            }
            // entitys.ForEach(c=>db.Entry<T>(c).State = EntityState.Added);
            return db.SaveChanges() > 0;
        }
        #endregion

        #region 修改
        //修改
        public bool UpdateEntity(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Modified;
            //db.Entry<T>(entity).State= EntityState.Unchanged;
            return db.SaveChanges() > 0;
            return true;
        }
        /// <summary>
        /// 修改实体 不提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateEntitiesNoSave(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Modified;
            //db.Entry<T>(entity).State= EntityState.Unchanged;
            //return db.SaveChanges() > 0;
            return true;
        }
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entitys">修改的所有实体对象集合</param>
        /// <returns></returns>
        public bool ModifyEntities(List<T> entitys)
        {
            entitys.ForEach(entity =>
            {
                db.Set<T>().Attach(entity);
                db.Entry<T>(entity).State = System.Data.Entity.EntityState.Modified;//将所有属性标记为修改状态
            });
            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// 修改一条数据,会修改指定列的值
        /// </summary>
        /// <param name="entity">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        public bool ModifyEntity(T entity, params string[] proNames)
        {

            DbEntityEntry<T> dbee = db.Entry<T>(entity);
            if (dbee.State == EntityState.Detached)
            {
                db.Set<T>().Attach(entity);
            }
            dbee.State = EntityState.Unchanged;//先将所有属性状态标记为未修改
            proNames.ToList().ForEach(c => dbee.Property(c).IsModified = true);//将要修改的属性状态标记为修改
            return db.SaveChanges() > 0;
        }
        #endregion

        #region 删除
        //删除
        public bool DeleteEntity(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Deleted;
            return db.SaveChanges() > 0;
            return true;
        }
        /// <summary>
        /// 删除实体 不提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool DeleteEntityNoSave(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Deleted;
            //return db.SaveChanges() > 0;
            return true;
        }

        /// <summary>
        /// 根据条件批量删除实体对象
        /// </summary>
        /// <param name="whereLambds"></param>
        /// <returns></returns>
        public bool DeleteEntityByWhere(Expression<Func<T, bool>> whereLambds)
        {
            var data = db.Set<T>().Where<T>(whereLambds).ToList();
            return DeleteEntitys(data);
        }
        /// <summary>
        /// 事务批量删除实体对象
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool DeleteEntitys(List<T> entitys)
        {
            foreach (var item in entitys)
            {
                db.Set<T>().Attach(item);
                db.Entry<T>(item).State = EntityState.Deleted;
            }
            return db.SaveChanges() > 0;
        }


        /// <summary>
        /// 批量物理删除数据,；拼接SQL删除语句（只适合Id列为int类型）
        /// </summary>
        /// <param name="ids">ids格式：1,3,2</param>
        /// <param name="idStr">自增长列，默认 id</param>
        /// <returns></returns>
        public bool DeleteBySql(string ids, string idStr = "id")
        {
            string tableName = typeof(T).Name;//获取表名
            if (ids.Contains(","))
            {
                ids = ids.Substring(1);
            }
            string sql = string.Format("delete from {0} where {2} in({1})", tableName, ids, idStr);
            return db.Database.ExecuteSqlCommand(sql) > 0;
        }
        #endregion

        #region 查询
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <param name="wherelambda"></param>
        /// <returns></returns>
        public IQueryable<T> LoadEntities(Expression<Func<T, bool>> wherelambda = null)
        {
            if (wherelambda == null)
            {
                wherelambda = x => true;
            }
            return db.Set<T>().Where<T>(wherelambda).AsQueryable();
        }
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="wherelambda">条件</param>
        /// <returns></returns>
        public T LoadEntity(Expression<Func<T, bool>> wherelambda = null)
        {
            if (wherelambda == null)
            {
                wherelambda = x => true;
            }
            return db.Set<T>().FirstOrDefault(wherelambda);
        }
        #endregion

        #region 分页
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="S">排序字段</typeparam>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="total">总数</param>
        /// <param name="whereLambda">条件</param>
        /// <param name="isAsc">排序方式</param>
        /// <param name="orderByLambda">排序筛选</param>
        /// <returns></returns>
        public IQueryable<T> LoadPagerEntities<S>(int pageSize, int pageIndex, out int total,
            Expression<Func<T, bool>> whereLambda, bool isAsc, Expression<Func<T, S>> orderByLambda)
        {
            var tempData = db.Set<T>().Where<T>(whereLambda);

            total = tempData.Count();

            //排序获取当前页的数据
            if (isAsc)
            {
                tempData = tempData.OrderBy<T, S>(orderByLambda).
                      Skip<T>(pageSize * (pageIndex - 1)).
                      Take<T>(pageSize).AsQueryable();
            }
            else
            {
                tempData = tempData.OrderByDescending<T, S>(orderByLambda).
                     Skip<T>(pageSize * (pageIndex - 1)).
                     Take<T>(pageSize).AsQueryable();
            }
            return tempData.AsQueryable();
        }

        /// <summary>
        /// 分页加载数据
        /// </summary>
        /// <param name="whereLambda">过滤条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <returns></returns>
        public virtual IQueryable<T> LoadPagerEntites(Expression<Func<T, bool>> whereLambda, int pageIndex, int pageSize, out int totalCount)
        {
            var tmp = db.Set<T>().Where<T>(whereLambda);
            totalCount = tmp.Count();

            return tmp.Skip<T>(pageSize * (pageIndex - 1))//跳过行数，最终生成的sql语句是Top(n)
                      .Take<T>(pageSize) //返回指定数量的行
                      .AsQueryable<T>();
        }
        #endregion


        #region 执行原生sql语句

        /// <summary>
        /// 执行数据源语句(如SQL)，返回影响的行数
        /// </summary>
        /// <param name="commandText">查询语句</param>
        /// <param name="parameter">参数(可选)</param>
        /// <returns></returns>
        public int ExecuteStoreCommand(string commandText, params Object[] paramete)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                return -1;
            }
            return db.Database.ExecuteSqlCommand(commandText, paramete);

        }

        /// <summary>
        /// 执行数据源查询语句(如SQL)，获得数据查询列表
        /// </summary>
        /// <param name="commandText">查询语句</param>
        /// <param name="parameter">参数(可选)</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteStoreQuery(string commandText, params Object[] paramete)
        {
            var result = db.Database.SqlQuery<T>(commandText, paramete);
            return result;

        }

        #endregion

        /// <summary>
        /// 批量提交，变更数据库
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()  //UintWork单元工作模式
        {
            //调用EF上下文的SaveChanges的方法
            return db.SaveChanges();

        }


        #region Entityframework.Extended 扩展 批量删除、修改
        /// <summary>
        /// 批量删除所有符合表达式条件的数据
        /// 
        /// eg:base.DeleteByExtended(o=>o.id==1)
        /// </summary>
        /// <param name="wherelambda">条件表达式</param>
        /// <returns></returns>
        public int DeleteByExtended(Expression<Func<T, bool>> wherelambda = null)
        {
            if (wherelambda == null)
            {
                wherelambda = x => true;
            }
            return db.Set<T>().Delete(wherelambda);
        }
        /// <summary>
        /// 批量修改符合条件的数据
        /// 
        /// eg:base.UpdateByExtended(o=>new obj{status=0},o=>o.price>5)
        /// 
        /// </summary>
        /// <param name="obj">修改的字段</param>
        /// <param name="wherelambda">条件表达式</param>
        /// <returns></returns>
        public int UpdateByExtended(Expression<Func<T, T>> obj, Expression<Func<T, bool>> wherelambda = null)
        {
            if (wherelambda == null)
            {
                wherelambda = x => true;
            }
            return db.Set<T>().Update(wherelambda, obj);
        }


        #endregion
    }
    public class EFContextFactory
    {
        /// <summary>
        /// 返回当前线程内的数据库上下文
        /// </summary>
        /// <returns></returns>
        public static DbContext GetCurrentDbContext()
        {
            DbContext dbcontext = CallContext.GetData("DbContext") as DbContext;

            //判断线程里面是否有数据
            if (dbcontext == null)
            {
                dbcontext = new LinXinApp20Entities();  //创建了一个EF上下文
                //存储指定对象
                CallContext.SetData("DbContext", dbcontext);
            }
            return dbcontext;
        }

    }
}
