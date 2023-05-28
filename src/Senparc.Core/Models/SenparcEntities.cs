﻿using System.Linq;
using System.Reflection;

namespace Senparc.Core.Models
{
    using Microsoft.EntityFrameworkCore;
    using Senparc.Core.Models.DataBaseModel;
    using Senparc.Scf.Core.Models;
    using Senparc.Scf.Core.Models.DataBaseModel;
    using System;
    using System.Linq.Expressions;

    public partial class SenparcEntities : DbContext, ISenparcEntities
    {
        private static readonly bool[] _migrated = { false };


        /// <summary>
        /// 重置数据库Migrate状态，合并将在下次初始化的时候执行
        /// </summary>
        public void ResetMigrate()
        {
            _migrated[0] = false;
        }

        public SenparcEntities(DbContextOptions<SenparcEntities> dbContextOptions) : base(dbContextOptions)
        {
            if (!_migrated[0])
            {
                lock (_migrated)
                {
                    if (!_migrated[0])
                    {
                        Database.Migrate(); // apply all migrations
                        _migrated[0] = true;
                    }
                }
            }
        }

        #region 系统表

        /// <summary>
        /// 菜单
        /// </summary>
        public DbSet<SysMenu> SysMenus { get; set; }

        /// <summary>
        /// 菜单下面的按钮
        /// </summary>
        public DbSet<SysButton> SysButtons { get; set; }

        /// <summary>
        /// 系统角色
        /// </summary>
        public DbSet<SysRole> SysRoles { get; set; }

        /// <summary>
        /// 角色菜单表
        /// </summary>
        public DbSet<SysPermission> SysPermission { get; set; }


        /// <summary>
        /// 角色人员表
        /// </summary>
        public DbSet<SysRoleAdminUserInfo> SysRoleAdminUserInfos { get; set; }


        public virtual DbSet<Account> Accounts { get; set; }

        public virtual DbSet<AdminUserInfo> AdminUserInfos { get; set; }

        public DbSet<SystemConfig> SystemConfigs { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public virtual DbSet<PointsLog> PointsLogs { get; set; }

        public virtual DbSet<AccountPayLog> AccountPayLogs { get; set; }


        #region 不可修改系统表

        #endregion
        /// <summary>
        /// 扩展模块
        /// </summary>
        public DbSet<XscfModule> XscfModules { get; set; }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region 系统表

            modelBuilder.ApplyConfiguration(new AccountConfigurationMapping());
            modelBuilder.ApplyConfiguration(new AdminUserInfoConfigurationMapping());
            modelBuilder.ApplyConfiguration(new FeedbackConfigurationMapping());
            modelBuilder.ApplyConfiguration(new AccountPayLogConfigurationMapping());
            modelBuilder.ApplyConfiguration(new PointsLogConfigurationMapping());

            #region 不可修改系统表

            modelBuilder.ApplyConfiguration(new XscfModuleAccountConfigurationMapping());

            #endregion
            #endregion

            var types = modelBuilder.Model.GetEntityTypes().Where(e => typeof(EntityBase).IsAssignableFrom(e.ClrType));
            foreach (var entityType in types)
            {
                SetGlobalQueryMethodInfo
                        .MakeGenericMethod(entityType.ClrType)
                        .Invoke(this, new object[] { modelBuilder });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static readonly MethodInfo SetGlobalQueryMethodInfo = typeof(SenparcEntities)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQuery");
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public void SetGlobalQuery<T>(ModelBuilder builder) where T : EntityBase
        {
            builder.Entity<T>().HasQueryFilter(z => !z.Flag);
        }
    }
}
