using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Utilities;
using Senparc.Scf.Core.Cache;
using Senparc.Scf.Core.Config;
using Senparc.Scf.Core.Models;
using Senparc.Scf.Repository;
using Senparc.Scf.Service;
using Senparc.Scf.SMS;
using Senparc.Service;
using Senparc.Web.Hubs;
using Senparc.Weixin;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.Open;
using Senparc.Weixin.Open.ComponentAPIs;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.TenPay;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Scf.Core.Exntesions;
using Microsoft.AspNetCore.Mvc;

namespace Senparc.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //��ȡLog�����ļ�
            var repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var cache = services.BuildServiceProvider().GetService<IMemoryCache>();//���Գɹ�
            services
                .Configure<SenparcWeixinSetting>(Configuration.GetSection("SenparcWeixinSetting"))
                .Configure<SenparcSmsSetting>(Configuration.GetSection("SenparcSmsSetting"))//TODO����SMSģ�����ע��
                ;

            services.AddSenparcGlobalServices(Configuration) //Senparc.CO2NET ȫ��ע��
                    .AddSenparcWeixinServices(Configuration); //Senparc.Weixin ע��


            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });



            //ע�� Repository �� Service
            services
                .AddScoped<FullSystemConfigCache>()//TODO�����ݽӿ��Զ����
                .AddSingleton<PhoneCheckCodeCache>()
                .AddSingleton<QrCodeRegCache>()
                .AddSingleton<OAuthCodeCache>()
                .AddScoped<FullAccountCache>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddScoped<SystemConfigRepository>().AddScoped<SystemConfigService>()
                .AddScoped<AccountRepository>().AddScoped<AccountService>()
                .AddScoped<AdminUserInfoRepository>().AddScoped<AdminUserInfoService>()
                .AddScoped<FeedBackRepository>().AddScoped<FeedBackService>()
                .AddScoped<PointsLogRepository>().AddScoped<PointsLogService>()
                .AddScoped<AccountPayLogRepository>().AddScoped<AccountPayLogService>()
                .AddTransient<EncryptionService>()
                .AddTransient<WeixinService>()
                .AddScoped<SmsRecordService>();

            //��� SenparcCoreSetting �����ļ������ݿ��Ը�����Ҫ��Ӧ�޸ģ�
            //ע�����ݿ�ͻ�������
            services.AddScoped(typeof(ISqlClientFinanceData), typeof(SqlClientFinanceData));

            services.AddScfServices(Configuration, CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IOptions<SenparcSetting> senparcSetting,
            IOptions<SenparcWeixinSetting> senparcWeixinSetting, IHubContext<ReloadPageHub> hubContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();

            #region CO2NET

            // ���� CO2NET ȫ��ע�ᣬ���룡
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                //���� UseSenparcGlobal() �ĸ����÷��� CO2NET Demo��https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
                .UseSenparcGlobal();

            #region ȫ�ֻ������ã����裩

            //��ͬһ���ֲ�ʽ����ͬʱ�����ڶ����վ��Ӧ�ó���أ�ʱ������ʹ�������ռ佫����루�Ǳ��룩
            register.ChangeDefaultCacheNamespace("SCFCache");

            #region ���ú�ʹ�� Redis

            //����ȫ��ʹ��Redis���棨���裬������
            var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis����";
            if (useRedis) //����Ϊ�˷��㲻ͬ�����Ŀ����߽������ã��������жϵķ�ʽ��ʵ�ʿ�������һ����ȷ���ģ������if�������Ժ���
            {
                /* ˵����
                 * 1��Redis �������ַ�����Ϣ��� Config.SenparcSetting.Cache_Redis_Configuration �Զ���ȡ��ע�ᣬ�粻��Ҫ�޸ģ��·��������Ժ���
                /* 2�������ֶ��޸ģ�����ͨ���·� SetConfigurationOption �����ֶ����� Redis ������Ϣ�����޸����ã����������ã�
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);

                //���»�������ȫ�ֻ�������Ϊ Redis
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); //��ֵ�Ի�����ԣ��Ƽ���
                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet�����ʽ�Ļ������

                //Ҳ����ͨ�����·�ʽ�Զ��嵱ǰ��Ҫ���õĻ������
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//��ֵ��
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
            }
            //������ﲻ����Redis�������ã���Ŀǰ����Ĭ��ʹ���ڴ滺�� 

            #endregion

            #region ע����־�����裬���飩

            register.RegisterTraceLog(ConfigTraceLog); //����TraceLog

            #endregion

            #endregion

            #endregion

            #region Weixin ����

            /* ΢�����ÿ�ʼ
             * 
             * ���鰴������˳�����ע�ᣬ�����뽫������ڵ�һλ��
             */

            //ע�Ὺʼ

            #region ΢�Ż��棨���裬������ register.UseSenparcWeixin () ֮ǰ��

            //΢�ŵ� Redis ���棬�����ʹ����ע�͵�������ǰ���뱣֤������Ч��������״�
            if (useRedis)
            {
                app.UseSenparcWeixinCacheRedis();
            }

            #endregion

            //��ʼע��΢����Ϣ�����룡
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value)
                //ע�⣺��һ��û�� ; ����ɽ���д .RegisterXX()
            #region ע�ṫ�ںŻ�С���򣨰��裩

                //ע�ṫ�ںţ���ע������
                .RegisterMpAccount(senparcWeixinSetting.Value, "SCF")
                .RegisterMpAccount("", "", "Senparc_Template")

                //ע�������ںŻ�С���򣨿�ע������
                //.RegisterWxOpenAccount(senparcWeixinSetting.Value, "��ʢ������С���֡�С����")
                //ע�������ƽ̨����ע������
            #region ע�������ƽ̨

                .RegisterOpenComponent(senparcWeixinSetting.Value,
                    //getComponentVerifyTicketFunc
                    componentAppId =>
                    {

                        var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/OpenTicket"));
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var file = Path.Combine(dir, string.Format("{0}.txt", componentAppId));
                        using (var fs = new FileStream(file, FileMode.Open))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                var ticket = sr.ReadToEnd();
                                return ticket;
                            }
                        }
                    },

                     //getAuthorizerRefreshTokenFunc
                     (componentAppId, auhtorizerId) =>
                     {
                         var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/AuthorizerInfo/" + componentAppId));
                         if (!Directory.Exists(dir))
                         {
                             Directory.CreateDirectory(dir);
                         }

                         var file = Path.Combine(dir, string.Format("{0}.bin", auhtorizerId));
                         if (!System.IO.File.Exists(file))
                         {
                             return null;
                         }

                         using (Stream fs = new FileStream(file, FileMode.Open))
                         {
                             var binFormat = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                             var result = (RefreshAuthorizerTokenResult)binFormat.Deserialize(fs);
                             return result.authorizer_refresh_token;
                         }
                     },

                     //authorizerTokenRefreshedFunc
                     (componentAppId, auhtorizerId, refreshResult) =>
                     {
                         var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/AuthorizerInfo/" + componentAppId));
                         if (!Directory.Exists(dir))
                         {
                             Directory.CreateDirectory(dir);
                         }

                         var file = Path.Combine(dir, string.Format("{0}.bin", auhtorizerId));
                         using (Stream fs = new FileStream(file, FileMode.Create))
                         {
                             var binFormat = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                             binFormat.Serialize(fs, refreshResult);
                             fs.Flush();
                         }
                     }, "��ʢ�����硿����ƽ̨")

            #endregion
                //�������⣬��Ȼ�����ڳ�������ط�ע�ṫ�ںŻ�С����
                //AccessTokenContainer.Register(appId, appSecret, name);//�����ռ䣺Senparc.Weixin.MP.Containers

            #endregion

            #region ע��΢��֧�������裩

                //ע������΢��֧���汾��V3������ע������
                .RegisterTenpayV3(senparcWeixinSetting.Value, "SCF") //��¼��ͬһ�� SenparcWeixinSettingItem ������

            #endregion

            ;

            #endregion

            #region .NET CoreĬ�ϲ�֧��GB2312

            //http://www.mamicode.com/info-detail-2225481.html
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            #endregion

            #region Senparc.Core ����

            //���ڽ��HttpContext.Connection.RemoteIpAddressΪnull������
            //https://stackoverflow.com/questions/35441521/remoteipaddress-is-always-null
            app.UseHttpMethodOverride(new HttpMethodOverrideOptions
            {
                //FormFieldName = "X-Http-Method-Override"//��ΪĬ��ֵ
            });

            app.UseSenparcMvcDI();

            //Senparc.Scf.Core.Config.SiteConfig.SenparcCoreSetting = senparcCoreSetting.Value;//��վ����

            //�ṩ��վ��Ŀ¼
            if (env.ContentRootPath != null)
            {
                Senparc.Scf.Core.Config.SiteConfig.ApplicationPath = env.ContentRootPath;
                Senparc.Scf.Core.Config.SiteConfig.WebRootPath = env.WebRootPath;
            }

            #endregion

            #region �첽�߳�

            {
                ////APM Ending ����ͳ��
                //var utility = new APMNeuralDataThreadUtility();
                //Thread thread = new Thread(utility.Run) { Name = "APMNeuralDataThread" };
                //SiteConfig.AsynThread.Add(thread.Name, thread);
            }

            SiteConfig.AsynThread.Values.ToList().ForEach(z =>
            {
                z.IsBackground = true;
                z.Start();
            }); //ȫ������ 

            #endregion

        }

        /// <summary>
        /// ����΢�Ÿ�����־
        /// </summary>
        private void ConfigTraceLog()
        {
            //������ΪDebug״̬ʱ��/App_Data/WeixinTraceLog/Ŀ¼�»�������־�ļ���¼���е�API������־����ʽ�����汾����ر�

            //���ȫ�ֵ�IsDebug��Senparc.CO2NET.Config.IsDebug��Ϊfalse���˴����Ե�������true�������Զ�Ϊtrue
            CO2NET.Trace.SenparcTrace.SendCustomLog("ϵͳ��־",
                "SenparcCoreFramework ϵͳ����"); //ֻ��Senparc.Weixin.Config.IsDebug = true���������Ч

            //ȫ���Զ�����־��¼�ص�
            CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
            {
                //����ÿ�δ���Log����Ҫִ�еĴ���
            };

            //����������WeixinException���쳣ʱ����
            WeixinTrace.OnWeixinExceptionFunc = ex =>
            {
                //����ÿ�δ���WeixinExceptionLog����Ҫִ�еĴ���

                //����ģ����Ϣ������Ա
                //var eventService = new Senparc.Weixin.MP.Sample.CommonService.EventService();
                //eventService.ConfigOnWeixinExceptionFunc(ex);
            };
        }
    }



    public static class PhysicalFileAppBuilderExtensions
    {
        private static readonly PhysicalFileProvider _fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

        /// <summary>
        /// ������ؼ��仯
        /// </summary>
        /// <param name="app"></param>
        /// <param name="hubContext"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsePhysicalFile(this IApplicationBuilder app, IHubContext<ReloadPageHub> hubContext)
        {
            RegisterPhysical(hubContext);
            return app;
        }

        /// <summary>
        /// ע����
        /// </summary>
        /// <param name="hubContext"></param>
        public static void RegisterPhysical(IHubContext<ReloadPageHub> hubContext)
        {
            Task.Run(() =>
            {
                var tcs = new TaskCompletionSource<object>();
                while (true)
                {
                    PhysicalFileAsync(hubContext).GetAwaiter().GetResult();
                }
            });
        }

        /// <summary>
        /// ����ļ��仯
        /// </summary>
        /// <param name="hubContext"></param>
        /// <returns></returns>
        private static async Task PhysicalFileAsync(IHubContext<ReloadPageHub> hubContext)
        {
            var jsToken = _fileProvider.Watch("wwwroot/**/*.js");
            var cssToken = _fileProvider.Watch("wwwroot/**/*.css");
            var cshtmlToken = _fileProvider.Watch("**/*.cshtml");
            var tcs = new TaskCompletionSource<object>();
            //TODO:�������Ч��������Բ�ʹ��while(true)������ѡ����RegisterChangeCallback �ڲ��ٴε���fileProvider.Watch
            jsToken.RegisterChangeCallback(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
                hubContext.Clients.All.SendAsync("ReloadPage", "js�ļ������仯");
            }, tcs);
            cssToken.RegisterChangeCallback(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
                hubContext.Clients.All.SendAsync("ReloadPage", "css�ļ������仯");
            }, tcs);
            cshtmlToken.RegisterChangeCallback(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
                hubContext.Clients.All.SendAsync("ReloadPage", "cshtml�ļ������仯");
            }, tcs);
            await tcs.Task.ConfigureAwait(false);
        }
    }
}