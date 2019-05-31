using Abp.Application.Services;
using DFramework.Pan.Domain;

namespace DFramework.Pan.QuotaAppServices
{
    public interface IQuotaAppService : IApplicationService
    {
        Quota SetQuota(string ownerId, string size);

        Quota SetQuota(string ownerId, long size);

        Quota GetQuota(string ownerId);

        /// <summary>
        /// 检查配额
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="sizeToIncrease"></param>
        void CheckQuota(string ownerId, long sizeToIncrease);

        Quota Increase(string ownerId, long size, string fileId);

        Quota Decrease(string ownerId, long size, string fileId);
    }
}