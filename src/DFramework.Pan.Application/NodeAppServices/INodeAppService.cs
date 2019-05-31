using Abp.Application.Services;
using DFramework.Pan.Domain;
using System.Collections.Generic;

namespace DFramework.Pan.NodeAppServices
{
    public interface INodeAppService : IApplicationService
    {
        Node CopyNode<T>(string ownerId,
                                                        string path,
                                                        string name,
                                                        string newPath,
                                                        string newName = null)
                                                        where T : Node;

        Node CopyNode<T>(string nodeId,
                                                        string targetOwnerId,
                                                        string newPath,
                                                        string newName = null)
                                                        where T : Node;

        Node CopyNode<T>(string ownerId,
                                                        string path,
                                                        string name,
                                                        string targetOwnerId,
                                                        string newPath,
                                                        string newName = null)
                                                        where T : Node;

        /// <summary>
        /// 创建文件节点
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="storageId"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        FileNode CreateFileNode(string ownerId,
                                                    string path,
                                                    string name,
                                                    long size,
                                                    string storageId,
                                                    string tags = null);

        FileNode ModifyFileNode(FileNode fileNode, long size, string storageId);

        FolderNode CreateFolderNode(string ownerId, string fullPath);

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        FolderNode CreateFolderNode(string ownerId, string path, string name);

        FolderNode CreateFolderNodeWithError(string ownerId, string path, string name);

        void DeleteNode(string ownerId, string path, string nameListStr);

        Domain.Node DeleteNode<T>(string ownerId, string path, string name)
            where T : Domain.Node;

        Domain.Node DeleteNode<T>(string nodeId) where T : Domain.Node;

        FolderNode EmptyFolder(string folderId);

        Domain.Node RestoreNode(string nodeId);

        T GetNodeWithNextLayer<T>(string ownerId, string path) where T : Domain.Node;

        T GetNodeById<T>(string nodeId, string withNextLayer) where T : Domain.Node;

        long GetFolderSize(FolderNode folder);

        long GetFolderSize(string folderId);

        long GetFolderSize(string ownerId, string path);

        long GetFolderSize(string ownerId, string path, string name);

        Domain.Node MoveNode<T>(string ownerId, string path, string name, string newPath) where T : Domain.Node;

        Domain.Node MoveNode<T>(string ownerId, string path, string name, string targetOwnerId, string newPath)
            where T : Domain.Node;

        void MoveNodes(string ownerId, string path, string nameListStr, string newPath);

        Domain.Node RenameNode<T>(string ownerId, string path, string oldName, string newName)
            where T : Domain.Node;

        IEnumerable<T> SearchNodes<T>(string ownerId, string path, string nodeName, string recursive)
            where T : Domain.Node;

        IEnumerable<T> SearchNodesByFolderId<T>(string folderId, string nodeName, string recursive)
            where T : Domain.Node;

        int GetFilesCount(string ownerId, string path, string recursive);

        string GetAppId(string fileId);

        List<T> GetNodeList<T>(string nodeIdListStr) where T : Domain.Node;

        List<Domain.Node> GetNodeList(string nodeIds, string ownerId);

        T GetNode<T>(string nodeId) where T : Domain.Node;

        T GetNodeWithNextLayer<T>(string ownerId, string path, string name) where T : Domain.Node;

        Domain.Node GetNode(string nodeId);

        Domain.Node GetNode(string ownerId, string path, string name);

        Domain.Node GetNode(string nodeId, string ownerId);

        T GetNode<T>(string ownerId, string path) where T : Node;

        T GetNode<T>(string ownerId
            , string path
            , string name)
            where T : Domain.Node;
    }
}