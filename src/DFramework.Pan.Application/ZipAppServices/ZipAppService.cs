using Abp.Domain.Repositories;

namespace DFramework.Pan.ZipAppServices
{
    public class ZipAppService : PanAppServiceBase, IZipAppService
    {
        protected readonly IRepository<Domain.ZipLog, string> _zipRepository;

        public ZipAppService(IRepository<Domain.ZipLog, string> zipRepository)
        {
            _zipRepository = zipRepository;
        }

        public Domain.ZipLog GetSameKeyZip(string key)
        {
            return _zipRepository.FirstOrDefault(c => c.ZipKey == key);
        }

        public void AddZipLog(string id, string zipKey, string nodeId, string[] inclusionIds)
        {
            var zipLog = new Domain.ZipLog(id, zipKey, nodeId, inclusionIds);
            _zipRepository.Insert(zipLog);
        }

        public Domain.ZipLog GetZipLogById(string id)
        {
            return _zipRepository.FirstOrDefault(c => c.Id == id);
        }

        public Domain.ZipLog GetZipLogByNodeId(string nodeId)
        {
            return _zipRepository.FirstOrDefault(c => c.NodeId == nodeId);
        }
    }
}