using Abp.EntityFramework;
using Abp.Modules;
using DFramework.Pan.EntityFramework;
using System.Data.Entity;
using System.Reflection;

namespace DFramework.Pan
{
    [DependsOn(typeof(AbpEntityFrameworkModule), typeof(PanCoreModule))]
    public class PanDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = "MyPanConnectionString";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            Database.SetInitializer<PanDbContext>(null);
        }
    }
}