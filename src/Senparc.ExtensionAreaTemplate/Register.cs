﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Trace;
using Senparc.ExtensionAreaTemplate.Functions;
using Senparc.ExtensionAreaTemplate.Models;
using Senparc.ExtensionAreaTemplate.Models.DatabaseModel;
using Senparc.ExtensionAreaTemplate.Respository;
using Senparc.ExtensionAreaTemplate.Services;
using Senparc.Scf.Core.Areas;
using Senparc.Scf.Core.Enums;
using Senparc.Scf.Repository;
using Senparc.Scf.XscfBase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.ExtensionAreaTemplate
{
    public class Register : XscfRegisterBase,
        IXscfRegister, //注册 XSCF 基础模块接口（必须）
        IAreaRegister, //注册 XSCF 页面接口（按需选用）
        IXscfDatabase  //注册 XSCF 模块数据库（按需选用）
    {
        public Register()
        { }


        #region IXscfRegister 接口

        public override string Name => "Senparc.ExtensionAreaTemplate";
        public override string Uid => "1052306E-8C78-4EBF-8CA9-0EB3738350AE";//必须确保全局唯一，生成后必须固定
        public override string Version => "0.2.0";//必须填写版本号

        public override string MenuName => "扩展页面测试模块";
        public override string Icon => "fa fa-dot-circle-o";//参考如：https://colorlib.com/polygon/gentelella/icons.html
        public override string Description => "这是一个示例项目，用于展示如何扩展自己的网页、功能模块，学习之后可以删除，也可以以此为基础模板，改成自己的扩展模块（XSCF Modules）。";

        /// <summary>
        /// 注册当前模块需要支持的功能模块
        /// </summary>
        public override IList<Type> Functions => new Type[] {
            typeof(DownloadSourceCode)
        };


        public override Task InstallOrUpdateAsync(IServiceProvider serviceProvider, InstallOrUpdate installOrUpdate)
        {
            switch (installOrUpdate)
            {
                case InstallOrUpdate.Install:
                    //新安装
                    var colorService = serviceProvider.GetService<AreaTemplate_ColorService>();
                    var databaseCreator = (serviceProvider.GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
                    databaseCreator.CreateTables();

                    break;
                case InstallOrUpdate.Update:
                    //更新
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return Task.CompletedTask;
        }

        public override async Task UninstallAsync(IServiceProvider serviceProvider, Func<Task> unsinstallFunc)
        {
            await unsinstallFunc().ConfigureAwait(false);
        }

        #endregion

        #region IAreaRegister 接口

        public string HomeUrl => "/Admin/MyApp/MyHomePage";

        public List<AreaPageMenuItem> AareaPageMenuItems => new List<AreaPageMenuItem>() {
             new AreaPageMenuItem(GetAreaHomeUrl(),"首页","fa fa-laptop"),
             new AreaPageMenuItem(GetAreaUrl("/Admin/MyApp/About"),"关于","fa fa-bookmark-o"),
        };


        public IMvcBuilder AuthorizeConfig(IMvcBuilder builder)
        {
            builder.AddRazorPagesOptions(options =>
            {
            });

            SenparcTrace.SendCustomLog("系统启动", "完成 Area:MyApp 注册");

            return builder;
        }

        public override IServiceCollection AddXscfModule(IServiceCollection services)
        {
            Func<IServiceProvider, MySenparcEntities> implementationFactory = s =>
                new MySenparcEntities(new DbContextOptionsBuilder<MySenparcEntities>()
                   .UseSqlServer(Scf.Core.Config.SenparcDatabaseConfigs.ClientConnectionString,
                                 b => b.MigrationsAssembly("Senparc.ExtensionAreaTemplate"))
                   .Options);
            services.AddScoped(implementationFactory);
            services.AddScoped<SqlMyAppFinanceData>();
            services.AddScoped<ISqlMyAppFinanceData, SqlMyAppFinanceData>();

            services.AddScoped(typeof(BaseRespository<>));
            services.AddScoped(typeof(AreaTemplate_ColorService));

            services.AddScoped(typeof(AreaTemplate_Color));


            //services.AddScoped(typeof(IRepositoryBase<AreaTemplate_Color>), serviceProvider =>
            //{
            //    var mySenparcEntities = serviceProvider.GetService<MySenparcEntities>();
            //    var sqlData = serviceProvider.GetService<ISqlMyAppFinanceData>();
            //    var obj = new RepositoryBase<AreaTemplate_Color>(sqlData);
            //    return obj;
            //});

            return base.AddXscfModule(services);
        }

        #endregion

        #region IXscfDatabase 接口

        public string UniquePrefix => DATABASE_PREFIX;

        /// <summary>
        /// 数据库前缀
        /// </summary>
        public const string DATABASE_PREFIX = "AreaTemplate";


        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AreaTemplate_ColorConfigurationMapping());
        }

        #endregion
    }
}
