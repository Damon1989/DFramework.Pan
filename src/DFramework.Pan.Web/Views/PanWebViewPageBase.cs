using Abp.Web.Mvc.Views;

namespace DFramework.Pan.Web.Views
{
    public abstract class PanWebViewPageBase : PanWebViewPageBase<dynamic>
    {
    }

    public abstract class PanWebViewPageBase<TModel> : AbpWebViewPage<TModel>
    {
        protected PanWebViewPageBase()
        {
            LocalizationSourceName = PanConsts.LocalizationSourceName;
        }
    }
}