﻿using Microsoft.EntityFrameworkCore.Storage;
using Senparc.Scf.Core.Models;
using Senparc.Scf.Repository;

namespace Senparc.Scf.Service
{
    public interface IClientServiceBase<T> : IServiceBase<T> where T : EntityBase, new()//global::System.Data.Objects.DataClasses.EntityObject, new()
    {
        IClientRepositoryBase<T> BaseClientRepository { get; }

        /// <summary>
        /// 开启事物
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction BeginTransaction();
    }


    public class ClientServiceBase<T> : ServiceBase<T>, IClientServiceBase<T> where T : EntityBase, new()//global::System.Data.Objects.DataClasses.EntityObject, new()
    {
        public IClientRepositoryBase<T> BaseClientRepository
        {
            get
            {
                return RepositoryBase as IClientRepositoryBase<T>;
            }
        }

        public ClientServiceBase(IClientRepositoryBase<T> repo)
            : base(repo)
        {
        }

        /// <summary>
        /// 开启事物
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return BaseData.BaseDB.BaseDataContext.Database.BeginTransaction();
        }
    }
}