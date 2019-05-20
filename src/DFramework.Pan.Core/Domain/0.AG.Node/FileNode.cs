namespace DFramework.Pan.Domain
{
    public class FileNode : Node
    {
        public long Size { get; set; }
        public string StorageFileId { get; set; }

        public FileNode()
        {
        }

        public FileNode(string accountId, string name, string path, FolderNode parentNode, long size, string fileId,
            string tags = null) : base(accountId, name, path, parentNode)
        {
            Size = size;
            StorageFileId = fileId;
            Tags = tags;
        }

        public override Node Clone(FolderNode parentNode, string newName = null)
        {
            return new FileNode(parentNode.OwnerId, newName ?? Name, Path, parentNode, Size, StorageFileId);
        }

        public bool IsImage
        {
            get
            {
                var nameToLower = Name.ToLower();
                return nameToLower.EndsWith(".jpg") ||
                       nameToLower.EndsWith(".jpeg") ||
                       nameToLower.EndsWith(".bmp") ||
                       nameToLower.EndsWith(".gif") ||
                       nameToLower.EndsWith(".tif") ||
                       nameToLower.EndsWith(".tiff") ||
                       nameToLower.EndsWith(".emf") ||
                       nameToLower.EndsWith(".exif") ||
                       nameToLower.EndsWith(".wmf") ||
                       //nameToLower.EndsWith(".ico") ||
                       nameToLower.EndsWith(".png");
            }
        }
    }
}