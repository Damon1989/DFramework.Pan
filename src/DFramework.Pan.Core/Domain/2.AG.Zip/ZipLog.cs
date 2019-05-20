using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFramework.Pan.Domain
{
    public class ZipLog : AggregateRoot<string>, IHasCreationTime
    {
        public ZipLog()
        {
        }

        public ZipLog(string id, string zipKey, string nodeId, string[] inclusionIds) : this()
        {
            Id = id;
            ZipKey = zipKey;
            NodeId = nodeId;
            InclusionIds = Newtonsoft.Json.JsonConvert.SerializeObject(InclusionIds);
        }

        public string ZipKey { get; set; }
        public string NodeId { get; set; }
        public string InclusionIds { get; set; }

        public DateTime CreationTime { get; set; }

        [NotMapped]
        public string[] InclusionIdList => !string.IsNullOrEmpty(InclusionIds)
            ? Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(InclusionIds)
            : new string[0];
    }
}