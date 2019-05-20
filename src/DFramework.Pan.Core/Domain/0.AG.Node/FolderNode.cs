using System;
using System.Collections.Generic;
using System.Linq;

namespace DFramework.Pan.Domain
{
    public class FolderNode : Node
    {
        public IEnumerable<FolderNode> SubFolders { get; set; }
        public IEnumerable<FileNode> Files { get; set; }

        public const string ROOT_NAME = "ROOT_NAME_INNER";
        public const string ROOT_PATH = "/ROOT_PATH_INNER/";

        public bool IsEmpty => (SubFolders == null || !SubFolders.Any()) && (Files == null || !Files.Any());
        public IEnumerable<Node> Children => this.SubFolders.Union<Node>(Files);
        public int Depth => Path.Split('/').Length;

        public bool IsParentOf(FolderNode another)
        {
            return another.OwnerId == this.OwnerId
                   && another.Path.StartsWith(this.Path)
                   && this.Depth > another.Depth;
        }

        public bool ContainsNode<T>(string name)
        where T : Node
        {
            return !this.IsEmpty &&
                   this.Children.OfType<T>().Any(f => !f.IsDeleted && f.Name == name);
        }

        public override string FullPath
        {
            get
            {
                if (IsRootNode())
                {
                    return "/";
                }

                return base.FullPath.TrimEnd('/') + "/";
            }
        }

        public bool IsRootNode()
        {
            return this.Path == ROOT_PATH && this.Name == ROOT_NAME;
        }

        private FolderNode()
        {
        }

        public FolderNode(string ownerId, string name, string path, FolderNode parentNode)
            : base(ownerId, name, path, parentNode)
        {
        }

        public Node AddNode(Node node)
        {
            node.Path = FullPath;
            return node;
        }

        public static FolderNode GetRootNode(string ownerId)
        {
            return new FolderNode(ownerId, "", "/", null);
        }

        public override void Delete()
        {
            if (!IsEmpty)
            {
                throw new Exception("目录不为空,无法删除.");
            }
            base.Delete();
        }

        public override Node Clone(FolderNode parentNode, string newName = null)
        {
            return new FolderNode(parentNode.OwnerId, newName ?? Name, Path, parentNode);
        }
    }
}