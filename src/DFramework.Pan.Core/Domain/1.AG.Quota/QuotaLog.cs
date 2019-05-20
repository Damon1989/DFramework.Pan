using Abp.Domain.Entities.Auditing;
using System;

namespace DFramework.Pan.Domain
{
    public class QuotaLog : AggregateRoot<string>, IHasCreationTime
    {
        public string OwnerId { get; set; }
        public long Size { get; set; }
        public string FileId { get; set; }
        public string AppId { get; set; }

        public DateTime CreationTime { get; set; }

        public QuotaLog()
        {
        }

        public QuotaLog(string ownerId, long size, string appId, string fileId)
        {
            Id = Guid.NewGuid().ToString("n");
            OwnerId = ownerId;
            Size = size;
            FileId = fileId;
            AppId = appId;
        }
    }
}