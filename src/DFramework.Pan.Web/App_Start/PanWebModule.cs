using Abp.Localization;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Modules;
using Abp.Web.Mvc;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DFramework.Pan.Web
{
    [DependsOn(
        typeof(AbpWebMvcModule),
        typeof(PanDataModule),
        typeof(PanApplicationModule),
        typeof(PanWebApiModule))]
    public class PanWebModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Add/remove languages for your application
            Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flag-gb", true));
            Configuration.Localization.Languages.Add(new LanguageInfo("tr", "Türkçe", "famfamfam-flag-tr"));
            Configuration.Localization.Languages.Add(new LanguageInfo("zh-CN", "简体中文", "famfamfam-flag-cn"));
            Configuration.Localization.Languages.Add(new LanguageInfo("ja", "日本語", "famfamfam-flag-jp"));

            //Add/remove localization sources here
            Configuration.Localization.Sources.Add(
                new DictionaryBasedLocalizationSource(
                    PanConsts.LocalizationSourceName,
                    new XmlFileLocalizationDictionaryProvider(
                        HttpContext.Current.Server.MapPath("~/Localization/Pan")
                        )
                    )
                );

            //Configure navigation/menu
            Configuration.Navigation.Providers.Add<PanNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}