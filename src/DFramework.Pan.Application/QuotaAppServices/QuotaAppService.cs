using Abp.Domain.Repositories;
using DFramework.Pan.Domain;
using System;
using System.Configuration;
using System.Data.Entity.Infrastructure;

namespace DFramework.Pan.QuotaAppServices
{
    public class QuotaAppService : PanAppServiceBase, IQuotaAppService
    {
        protected readonly long defaultQuota = long.Parse(ConfigurationManager.AppSettings["DefaultQuota"]);
        protected readonly string defaultAppId = ConfigurationManager.AppSettings["AppId"];
        private string _appId;
        protected readonly IRepository<Quota, string> _quotaRepository;
        protected readonly IRepository<QuotaLog, string> _quotaLogRepository;

        public QuotaAppService(IRepository<Quota, string> quotaRepository,
            IRepository<QuotaLog, string> quotaLogRepository)
        {
            _appId = defaultAppId;
            _quotaRepository = quotaRepository;
            _quotaLogRepository = quotaLogRepository;
        }

        public Domain.Quota SetQuota(string ownerId, string size)
        {
            return SetQuota(ownerId, long.Parse(size));
        }

        public Domain.Quota SetQuota(string ownerId, long size)
        {
            var quota = _quotaRepository.FirstOrDefault(c => c.Id == ownerId);
            try
            {
                if (quota == null)
                {
                    quota = new Domain.Quota(ownerId);
                    _quotaRepository.Insert(quota);
                }

                quota.SetQuota(size);
            }
            catch (DbUpdateException ex)
            {
                var sqlException = ex.GetBaseException() as System.Data.SqlClient.SqlException;
                if (sqlException != null && sqlException.Number == 2627/* duplicate key*/)
                {
                    _quotaRepository.Delete(quota);
                    quota = _quotaRepository.FirstOrDefault(c => c.Id == ownerId);
                }
                else
                {
                    throw;
                }
            }

            return quota;
        }

        public Domain.Quota GetQuota(string ownerId)
        {
            var quota = _quotaRepository.FirstOrDefault(c => c.OwnerId == ownerId);
            if (quota == null)
            {
                quota = SetQuota(ownerId, defaultQuota);
            }

            return quota;
        }

        /// <summary>
        /// 检查配额
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="sizeToIncrease"></param>
        public void CheckQuota(string ownerId, long sizeToIncrease)
        {
            var quota = GetQuota(ownerId);
            if (quota.Used + sizeToIncrease > quota.Max)
            {
                throw new Exception("配额不足");
            }
        }

        public Domain.Quota Increase(string ownerId, long size, string fileId)
        {
            return UsingQuota(ownerId, size, fileId, true);
        }

        public Domain.Quota Decrease(string ownerId, long size, string fileId)
        {
            return UsingQuota(ownerId, size, fileId, false);
        }

        private Domain.Quota UsingQuota(string ownerId, long size, string fileId, bool isIncrease = true)
        {
            var quota = GetQuota(ownerId);
            var quotaLog = new QuotaLog(ownerId, (isIncrease ? 1 : -1) * size, _appId, fileId);
            _quotaLogRepository.Insert(quotaLog);

            OptimisticConcurrencyProcessor.Process(() =>
            {
                if (isIncrease)
                    quota.Increase(size);
                else
                    quota.Decrease(size);
                return 1;
            });
            return quota;
        }
    }
}