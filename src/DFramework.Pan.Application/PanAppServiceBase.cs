using Abp.Application.Services;

namespace DFramework.Pan
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class PanAppServiceBase : ApplicationService
    {
        protected PanAppServiceBase()
        {
            LocalizationSourceName = PanConsts.LocalizationSourceName;
        }
    }
}