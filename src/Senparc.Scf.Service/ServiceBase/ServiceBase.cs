﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Senparc.CO2NET;
using Senparc.Scf.Core.Enums;
using Senparc.Scf.Core.Models;
using Senparc.Scf.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Senparc.Scf.Service
{
    public class ServiceBase<T> : ServiceDataBase, IServiceBase<T> where T : class, IEntityBase// global::System.Data.Objects.DataClasses.EntityObject, new()
    {
        public IMapper Mapper { get; set; } //TODO: add in to Wapper

        public IRepositoryBase<T> RepositoryBase { get; set; }

        public ServiceBase(IRepositoryBase<T> repo, IMapper mapper = null)
            : base(repo)
        {
            RepositoryBase = repo;
            Mapper = mapper == null ? SenparcDI.GetService<IMapper>() : mapper;//确保 Mapper 中有值
        }

        public virtual bool IsInsert(T obj)
        {
            return RepositoryBase.IsInsert(obj);
        }

        public T GetObject(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return RepositoryBase.GetFirstOrDefaultObject(where, includes);
        }

        public T GetObject<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return RepositoryBase.GetFirstOrDefaultObject(where, orderBy, orderingType, includes);
        }

        public PagedList<T> GetFullList<TK>(Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return this.GetObjectList(0, 0, where, orderBy, orderingType, includes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderingType"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual PagedList<T> GetObjectList<TK>(int pageIndex, int pageCount, Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return RepositoryBase.GetObjectList(where, orderBy, orderingType, pageIndex, pageCount, includes);
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageCount">每页数量</param>
        /// <param name="where">条件</param>
        /// <param name="orderBy">排序字段 eg.(xxx desc, bbb aec),默认升序</param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual async Task<PagedList<T>> GetObjectListAsync(int pageIndex, int pageCount, Expression<Func<T, bool>> where, string orderBy, string[] includes = null)
        {
            return await RepositoryBase.GetObjectListAsync(where, orderBy, pageIndex, pageCount, includes);
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageCount">每页数量</param>
        /// <param name="where">条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="orderingType">正序|倒叙</param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual async Task<PagedList<T>> GetObjectListAsync<TK>(int pageIndex, int pageCount, Expression<Func<T, bool>> where, Expression<Func<T, TK>> orderBy, OrderingType orderingType, string[] includes = null)
        {
            return await RepositoryBase.GetObjectListAsync(where, orderBy, orderingType, pageIndex, pageCount, includes);
        }

        public virtual int GetCount(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return RepositoryBase.ObjectCount(where, includes);
        }

        public virtual decimal GetSum(Expression<Func<T, bool>> where, Func<T, decimal> sum, string[] includes = null)
        {
            return RepositoryBase.GetSum(where, sum, includes);
        }

        /// <summary>
        /// 强制将实体设置为Modified状态
        /// </summary>
        /// <param name="obj"></param>
        public virtual void TryDetectChange(T obj)
        {
            if (!IsInsert(obj))
            {
                RepositoryBase.BaseDB.BaseDataContext.Entry(obj).State = EntityState.Modified;
            }
        }

        public virtual void SaveObject(T obj)
        {
            if (RepositoryBase.BaseDB.ManualDetectChangeObject)
            {
                TryDetectChange(obj);
            }
            RepositoryBase.Save(obj);
        }

        public virtual void DeleteObject(Expression<Func<T, bool>> predicate)
        {
            T obj = GetObject(predicate);
            DeleteObject(obj);
        }

        public virtual void DeleteObject(T obj)
        {
            RepositoryBase.Delete(obj);
        }

        public virtual void DeleteAll(IEnumerable<T> objects)
        {
            var list = objects.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                DeleteObject(list[i]);
            }
        }

        //TODO: 提供异步版本
        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<T> GetObjectAsync(Expression<Func<T, bool>> where, string[] includes = null)
        {
            return await RepositoryBase.GetFirstOrDefaultObjectAsync(where, includes);
        }

        public async Task SaveObjectAsync(T obj)
        {
            if (RepositoryBase.BaseDB.ManualDetectChangeObject)
            {
                TryDetectChange(obj);
            }
            await RepositoryBase.SaveAsync(obj);
        }

        public virtual async Task DeleteObjectAsync(Expression<Func<T, bool>> predicate)
        {
            T obj = await GetObjectAsync(predicate);
            await DeleteObjectAsync(obj);
        }

        public virtual async Task DeleteObjectAsync(T obj)
        {
            await RepositoryBase.DeleteAsync(obj, true);
        }

        public virtual async Task DeleteAllAsync(Expression<Func<T, bool>> where, bool softDelete = false)
        {
            var list = await GetFullListAsync(where);
            await RepositoryBase.DeleteAllAsync(list, softDelete);
        }

        public virtual async Task DeleteAllAsync(IEnumerable<T> objects, bool softDelete = false)
        {
            await RepositoryBase.DeleteAllAsync(objects, softDelete);
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderField">xxx desc, yyy asc</param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<PagedList<T>> GetFullListAsync(Expression<Func<T, bool>> where, string orderField = null, string[] includes = null)
        {
            return await RepositoryBase.GetObjectListAsync(where, orderField, 0, 0, includes);
        }

        public async Task SaveObjectListAsync(IEnumerable<T> objs)
        {
            await RepositoryBase.SaveObjectListAsync(objs);
        }


        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await RepositoryBase.BeginTransactionAsync();
        }

        /// <summary>
        /// 开启事务, 此方法回自动提交事务，失败则回滚
        /// </summary>
        /// <returns></returns>
        public async Task BeginTransactionAsync(Action action)
        {
            await RepositoryBase.BeginTransactionAsync();
            try
            {
                action();
                CommitTransaction();
            }
            catch (Exception)
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return RepositoryBase.BeginTransaction();
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public void BeginTransaction(Action action, Action<Exception> rollbackAction = null)
        {
            BeginTransaction();
            try
            {
                action();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                rollbackAction?.Invoke(ex);
                throw;
            }
        }


        /// <summary>
        /// 开启事务, 此方法回自动提交事务，失败则回滚
        /// </summary>
        /// <returns></returns>
        public async Task BeginTransactionAsync(Func<Task> action, Action<Exception> rollbackAction = null)
        {
            await BeginTransactionAsync();
            try
            {
                await action();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                rollbackAction?.Invoke(ex);
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <returns></returns>
        public void RollbackTransaction()
        {
            RepositoryBase.RollbackTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        public void CommitTransaction()
        {
            RepositoryBase.CommitTransaction();
        }
    }
}