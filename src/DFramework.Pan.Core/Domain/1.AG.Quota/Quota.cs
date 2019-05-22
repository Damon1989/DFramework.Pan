using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace DFramework.Pan.Domain
{
    public class Quota : AggregateRoot<string>, IHasCreationTime
    {
        public string OwnerId { get; set; }
        public long Max { get; set; }

        [ConcurrencyCheck]
        public long Used { get; set; }

        private Quota()
        {
        }

        public Quota(string ownerId)
        {
            Id = Guid.NewGuid().ToString("n");
            OwnerId = ownerId;
        }

        public void Increase(long size)
        {
            this.Used += size;
        }

        public void Decrease(long size)
        {
            this.Used -= size;
            if (this.Used < 0)
            {
                this.Used = 0;
            }
        }

        public void SetQuota(long quota)
        {
            if (quota <= 0)
            {
                throw new Exception("配额必须大于0");
            }
            else if (quota < Used)
            {
                throw new Exception($"已经使用了");
            }

            this.Max = quota;
        }

        public string NumberByUnit(int number)
        {
            if (number <= OneKB)
            {
                return $"{number}Bytes";
            }
            if (number <= OneMB)
            {
                return $"{number / OneKB}KB";
            }
            if (number <= OneGB)
            {
                return $"{number / OneMB}MB";
            }
            return $"{number / OneGB}GB";
        }

        private const int OneKB = 1024;
        private const int OneMB = 1024 * 1024;
        private const int OneGB = 1024 * 1024 * 1024;
        public DateTime CreationTime { get; set; }
    }
}