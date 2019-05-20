using Abp.Application.Services;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.WebApi;
using System.Reflection;

namespace DFramework.Pan
{
    [DependsOn(typeof(AbpWebApiModule), typeof(PanApplicationModule))]
    public class PanWebApiModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            Configuration.Modules.AbpWebApi().DynamicApiControllerBuilder
                .ForAll<IApplicationService>(typeof(PanApplicationModule).Assembly, "app")
                .Build();
        }
    }
}