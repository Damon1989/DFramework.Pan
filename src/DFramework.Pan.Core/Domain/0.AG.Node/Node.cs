using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFramework.Pan.Domain
{
    public abstract class Node : AggregateRoot<string>, IHasCreationTime
    {
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDeleted { get; set; }
        public string ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Node Parent { get; set; }

        public string Tags { get; set; }
        public DateTime CreationTime { get; set; }
        public virtual string FullPath => Path + Name;

        public bool Equals(Node another)
        {
            return this.GetType() == another.GetType() && this.OwnerId == another.OwnerId && this.Path == another.Path &&
                   this.Name == another.Name;
        }

        protected Node()
        {
        }

        protected Node(string ownerId, string name, string path, FolderNode parentNode)
        {
            Id = Guid.NewGuid().ToString("n");
            OwnerId = ownerId;
            Name = name;
            Path = path;
            if (parentNode != null)
            {
                parentNode.AddNode(this);
            }

            IsDeleted = false;
        }

        public void Rename(string newName)
        {
            if (Name == newName)
            {
                throw new Exception("文件或目录名没有变化.");
            }

            Name = newName;
        }

        public virtual void Delete()
        {
            IsDeleted = true;
        }

        public abstract Node Clone(FolderNode parentNode, string newName = null);

        public void UpdateOwnerId(string ownerId)
        {
            OwnerId = ownerId;
        }
    }
}