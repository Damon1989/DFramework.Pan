using Abp.Modules;
using System.Reflection;

namespace DFramework.Pan
{
    [DependsOn(typeof(PanCoreModule))]
    public class PanApplicationModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}