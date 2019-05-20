using Abp.Modules;
using System.Reflection;

namespace DFramework.Pan
{
    public class PanCoreModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}