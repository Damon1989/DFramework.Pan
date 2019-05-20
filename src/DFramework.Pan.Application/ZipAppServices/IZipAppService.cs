using Abp.Application.Services;
using DFramework.Pan.Domain;

namespace DFramework.Pan.ZipAppServices
{
    public interface IZipAppService : IApplicationService
    {
        ZipLog GetSameKeyZip(string key);

        void AddZipLog(string id, string zipKey, string nodeId, string[] inclusionIds);

        ZipLog GetZipLogById(string id);

        ZipLog GetZipLogByNodeId(string nodeId);
    }
}