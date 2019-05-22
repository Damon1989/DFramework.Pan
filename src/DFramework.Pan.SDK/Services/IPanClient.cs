using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFramework.Pan.SDK.Services
{
    public enum FileExistStrategy
    {
        Fault,Overwrite,Rename
    }

    public enum NodeType
    {
        File=0x01,
        Folder=0x02,
        Both=File|Folder
    }
    public interface IPanClient
    {
        FileModel CopyFile(string ownerId, string path, string name, string newPath, FileExistStrategy strategy,
            string newName = "");

        FileModel CopyFile(string fileId, string targetOwnerId, string newPath, FileExistStrategy strategy,
            string newName = "");

        FileModel CopyFIle(string ownerId, string path, string name, string targetOwnerId, string newPath,
            FileExistStrategy strategy,
            string newName = "");

        FolderModel CopyFolder(string folderId, string targetOwnerId, string newPath);

        FolderModel CopyFolder(string ownerId, string path, string name, string newPath);

        void CopyNodes(string ownerId, string path, string[] nameList, string newPath);
        void CopyNodes(string ownerId, string path, string[] nameList, string targetOwnerId, string newPath);

        FolderModel CreateFolder(string ownerId, string path, string name);
        FileModel DeleteFile(string ownerId, string path, string name);
        FolderModel DeleteFolder(string ownerId, string path, string name);

        void DeleteNodes(string ownerId, string path, string[] nameList);

        FolderModel EmptyFolder(string folderId);

        FileModel GetFile(string ownerId, string path, string name);
        FileModel GetFile(string ownerId, string fullPath);
        FileModel GetFile(string fileId);

        List<FileModel> GetFiles(string[] fileIds);
        Task<string> GetFileContent(string fileId);
        int GetFilesCount(string ownerId, string path, bool recursive = false);
        Task<Stream> GetFileStream(string fileId);
        FolderModel GetFolder(string ownerId, string path);

        long GetFolderSize(string ownerId, string path);
        FileModel Modify(string fileId, Stream stream, string tags = null);

        FileModel Modify(string ownerId, string path, string name, Stream stream, string tags = null);

        Task<FileModel> ModifyAsync(string fileId, Stream stream, string tags = null);

        Task<FileModel> ModifyAsync(string ownerId, string path, string name, Stream stream, string tags = null);

        FileModel MoveFile(string ownerId, string path, string name, string newPath);
        FolderModel MoveFolder(string ownerId, string path, string name, string newPath);

        void MoveNodes(string ownerId, string path, string[] nameList, string newPath);
        void MoveNodes(string ownerId, string path, string[] nameList, string targetOwnerId, string newPath);

        FileModel RenameFile(string ownerId, string path, string oldName, string newName);
        FolderModel RenameFolder(string ownerId, string path, string oldName, string newName);

        NodeModel[] SearchNodes(string ownerId, string path, string nodeName, NodeType nodeType,
            bool recursive = false);

        NodeModel[] SearchNodesByFolderId(string folderId, string nodeName, NodeType nodeType, bool recursive = false);

        FileModel Upload(string ownerId, string path, string fileName, Stream stream, FileExistStrategy strategy,
            string tags = null);

        Task<FileModel> UploadAsync(string ownerId, string path, string fileName, Stream stream,
            FileExistStrategy strategy, string tags = null);

        FileModel UploadIsolate(string ownerId, string fileName, Stream fileStream, string tags = null);

        Task<FileModel> UploadIsolateAsync(string ownerId, string fileName, Stream stream, string tags = null);

        List<NodeModel> GetNodeList(string[] nodeIds, string ownerId);

        string GetZipDownloadUrl(string[] nodeIds, string appId, string path);

        NodeModel GetNode(string nodeId);

        VirtualFolderModel GetVirtualFolderModel(string[] nodeIds);

        FolderModel GetFolderWithNextLayer(string ownerId, string path);

        FolderModel CreateFolderWithError(string ownerId, string path, string name);

        FolderModel GetFolderById(string nodeId);

        FolderModel GetFolderByIdWithNextLayer(string nodeId);

    }
}
