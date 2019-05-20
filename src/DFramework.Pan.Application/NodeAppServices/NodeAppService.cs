using Abp.Domain.Repositories;
using DFramework.Pan.Domain;
using DFramework.Pan.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DFramework.Pan.NodeAppServices
{
    public class NodeAppService : PanAppServiceBase, INodeAppService
    {
        private readonly IRepository<Node, string> _nodeRepository;
        private readonly PanDbContext _panDbContext;

        public NodeAppService(IRepository<Domain.Node, string> nodeRepository,
                                                PanDbContext panDbContext)
        {
            _nodeRepository = nodeRepository;
            _panDbContext = panDbContext;
        }

        public Node CopyNode<T>(string ownerId, string path, string name, string newPath, string newName = null) where T : Node
        {
            //本身网盘复制
            return CopyNode<T>(ownerId, path, name, ownerId, newPath, newName);
        }

        public Node CopyNode<T>(string nodeId, string targetOwnerId, string newPath, string newName = null) where T : Node
        {
            var node = GetNode(nodeId);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }

            return CopyNode<T>(node.OwnerId, node.Path, node.Name, targetOwnerId, newPath, newName);
        }

        public Node CopyNode<T>(string ownerId, string path, string name, string targetOwnerId, string newPath, string newName = null) where T : Node
        {
            var node = GetNode<T>(ownerId, path, name);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }

            var newFolder = GetNode<FolderNode>(targetOwnerId, newPath);
            if (newFolder == null)
            {
                newFolder = CreateFolderNode(targetOwnerId, newPath);
            }

            if (node is FolderNode && node.Equals(newFolder))
            {
                throw new Exception("不能复制到本身目录");
            }

            var folderNode = node as FolderNode;
            if (folderNode != null && folderNode.IsParentOf(newFolder))
            {
                throw new Exception("不能复制到本身的子目录.");
            }

            if (newFolder.ContainsNode<T>(newName ?? name))
            {
                throw new Exception("存在同名文件或目录.");
            }

            var newNode = DoCopyNode(node, newFolder, newName);
            _panDbContext.SaveChanges();
            return newNode;
        }

        /// <summary>
        /// 递归拷贝目录
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newFolder"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private Domain.Node DoCopyNode(Domain.Node node,
            FolderNode newFolder,
            string newName = null)
        {
            var newNode = node.Clone(newFolder, newName);
            if (node is FolderNode folderNode)
            {
                AttachChildren(folderNode);
                foreach (var item in folderNode.SubFolders)
                {
                    DoCopyNode(item, (FolderNode)newNode);
                }
                foreach (var item in folderNode.Files)
                {
                    DoCopyNode(item, (FolderNode)newNode);
                }
            }

            _panDbContext.Nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// 获取子目录和文件
        /// </summary>
        /// <param name="node"></param>
        private void AttachChildren(FolderNode node)
        {
            node.SubFolders = GetNodes<FolderNode>(node.OwnerId, node.FullPath);
            node.Files = GetNodes<FileNode>(node.OwnerId, node.FullPath);
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ownerId"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private IList<T> GetNodes<T>(string ownerId
            , string path
            , string name = null)
            where T : Domain.Node
        {
            CheckPath(ref path);
            return _panDbContext.Nodes.OfType<T>().Where(c => !c.IsDeleted && c.OwnerId == ownerId
                                                                           && c.Path.ToLower() == path.ToLower() &&
                                                                           (name == null || name == c.Name))
                .OrderBy(c => c.CreationTime).ToList();
        }

        public FileNode CreateFileNode(string ownerId, string path, string name, long size, string storageId, string tags = null)
        {
            FolderNode parentFolderNode = null;
            if (path != null)//path为null时不创建文件结构，只能通过id存取
            {
                CheckPath(ref path);
                CheckName(name);

                //创建上册目录
                var pathName = SplitFullPath(path);
                parentFolderNode = CreateFolderNode(ownerId, pathName.Item1, pathName.Item2);
            }

            var fileNode = new FileNode(ownerId, name, path, parentFolderNode, size, storageId, tags);
            _panDbContext.Nodes.Add(fileNode);
            _panDbContext.SaveChanges();
            return fileNode;
        }

        public FileNode ModifyFileNode(FileNode fileNode, long size, string storageId)
        {
            fileNode.Size = size;
            fileNode.StorageFileId = storageId;
            _panDbContext.SaveChanges();
            return fileNode;
        }

        public FolderNode CreateFolderNode(string ownerId, string fullPath)
        {
            var pathName = SplitFullPath(fullPath);
            return CreateFolderNode(ownerId, pathName.Item1, pathName.Item2);
        }

        public FolderNode CreateFolderNode(string ownerId, string path, string name)
        {
            FolderNode node = null;
            LockUtility.Lock(ownerId, () =>
            {
                node = DoCreateFolderNode(ownerId, path, name) as FolderNode;
                _panDbContext.SaveChanges();
            });
            return node;
        }

        public FolderNode CreateFolderNodeWithError(string ownerId, string path, string name)
        {
            FolderNode node = null;
            LockUtility.Lock(ownerId, () =>
            {
                node = DoCreateFolderNodeWithError(ownerId, path, name) as FolderNode;
                _panDbContext.SaveChanges();
            });
            return node;
        }

        public void DeleteNode(string ownerId, string path, string nameListStr)
        {
            var nameList = nameListStr?.Split('/');
            if (nameList?.Length > 0)
            {
                foreach (var name in nameList)
                {
                    var node = GetNode(ownerId, path, name);
                    if (node == null)
                    {
                        throw new Exception("文件或文件夹不存在");
                    }
                }
            }
        }

        public Node DeleteNode<T>(string ownerId, string path, string name) where T : Node
        {
            var node = GetNode<T>(ownerId, path, name);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }

            return DoDeleteNode(node);
        }

        public Node DeleteNode<T>(string nodeId) where T : Node
        {
            var node = GetNode(nodeId);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }

            return DoDeleteNode(node);
        }

        public FolderNode EmptyFolder(string folderId)
        {
            var node = GetNode(folderId);
            if (node == null)
            {
                throw new Exception("未见或目录未找到.");
            }

            return DoEmptyFolder(node as FolderNode);
        }

        public Node RestoreNode(string nodeId)
        {
            var node = _panDbContext.Nodes.FirstOrDefault(c => c.IsDeleted && c.Id == nodeId);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }

            node.IsDeleted = false;
            _panDbContext.SaveChanges();
            return node;
        }

        public T GetNodeWithNextLayer<T>(string ownerId, string path) where T : Node
        {
            CheckPath(ref path);
            var pathName = SplitFullPath(path);
            return GetNodeWithNextLayer<T>(ownerId, pathName.Item1, pathName.Item2);
        }

        public T GetNodeById<T>(string nodeId, string withNextLayer) where T : Node
        {
            var node = GetNode<T>(nodeId);
            if (node is FolderNode && withNextLayer == Boolean.TrueString)
            {
                foreach (var item in (node as FolderNode).SubFolders)
                {
                    AttachChildren(item);
                }
            }

            return node;
        }

        public long GetFolderSize(FolderNode folder)
        {
            if (folder == null || folder.IsEmpty)
            {
                return 0;
            }

            return folder.SubFolders.Sum(c =>
            {
                AttachChildren(c);
                return GetFolderSize(c);
            }) + folder.Files.Sum(c => c.Size);
        }

        public long GetFolderSize(string folderId)
        {
            return GetFolderSize(GetNode<FolderNode>(folderId));
        }

        public long GetFolderSize(string ownerId, string path)
        {
            return GetFolderSize(GetNode<FolderNode>(ownerId, path));
        }

        public long GetFolderSize(string ownerId, string path, string name)
        {
            return GetFolderSize(GetNode<FolderNode>(ownerId, path, name));
        }

        public Node MoveNode<T>(string ownerId, string path, string name, string newPath) where T : Node
        {
            //查找文件/目录
            var node = GetNode<T>(ownerId, path, name);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }
            //目标目录
            var newFolder = GetNode<FolderNode>(ownerId, newPath);
            if (newFolder == null)
            {
                newFolder = CreateFolderNode(ownerId, newPath);
            }
            //不能移动到本身目录
            if (node is FolderNode && node.Equals(newFolder))
            {
                throw new Exception("不能移动到本身目录.");
            }
            //不能移动到本身的子目录
            if (node is FolderNode && (node as FolderNode).IsParentOf(newFolder))
            {
                throw new Exception("不能移动到本身的子目录.");
            }
            //移动后是否重名
            if (newFolder.ContainsNode<T>(name))
            {
                throw new Exception("存在同名文件或目录.");
            }
            //移动目录下面的所有子目录/文件的路径
            //连带isDeleted的记录一起移动
            if (node is FolderNode)
            {
                var sql = $"update p_Node set path='{newFolder.FullPath}'+substring(path,len('{node.Path}')+1,8000) " +
                          $"from p_Node where ownerId='{ownerId}' and path like '{node.FullPath}%'";
                _panDbContext.Database.ExecuteSqlCommand(sql);
            }

            newFolder.AddNode(node);
            _panDbContext.SaveChanges();
            return node;
        }

        public Node MoveNode<T>(string ownerId, string path, string name, string targetOwnerId, string newPath) where T : Node
        {
            //查找文件/目录
            var node = GetNode<T>(ownerId, path, name);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }
            //目标目录
            var newFolder = (FolderNode)GetNode<FolderNode>(targetOwnerId, newPath);
            if (newFolder == null)
            {
                newFolder = CreateFolderNode(targetOwnerId, newPath);
            }
            //不能移动到本身目录
            if (node is FolderNode && node.Equals(newFolder))
            {
                throw new Exception("不能移动到本身目录");
            }
            //不能移动到本身的子目录
            if (node is FolderNode && (node as FolderNode).IsParentOf(newFolder))
            {
                throw new Exception("不能移动到本身的子目录.");
            }
            //移动后是否重名
            if (newFolder.ContainsNode<T>(name))
            {
                throw new Exception("存在同名文件或目录.");
            }
            //移动目录下面的所有子目录/文件的路径
            //连带isDeleted的记录一起移动
            if (node is FolderNode)
            {
                var sql =
                    $"update p_Node set path='{newFolder.FullPath}'+substring(path,len('{node.Path}')+1,8000), ownerId='{targetOwnerId}'" +
                    $" from p_Node where ownerId='{ownerId}' and path like '{node.FullPath}%'";
                _panDbContext.Database.ExecuteSqlCommand(sql);
            }

            node.UpdateOwnerId(targetOwnerId);
            newFolder.AddNode(node);
            _panDbContext.SaveChanges();
            return node;
        }

        public void MoveNodes(string ownerId, string path, string nameListStr, string newPath)
        {
            var nameList = nameListStr?.Split('/');
            if (nameList?.Length > 0)
            {
                foreach (var name in nameList)
                {
                    var node = GetNode(ownerId, path, name);
                    if (node == null)
                    {
                        throw new Exception("文件或文件夹不存在");
                    }

                    if (node is FileNode)
                    {
                        MoveNode<FileNode>(ownerId, path, name, newPath);
                    }

                    if (node is FolderNode)
                    {
                        MoveNode<FolderNode>(ownerId, path, name, newPath);
                    }
                }
            }
        }

        public Node RenameNode<T>(string ownerId, string path, string oldName, string newName) where T : Node
        {
            var node = GetNode<T>(ownerId, path, oldName);
            if (node == null)
            {
                throw new Exception("文件或目录未找到.");
            }
            //改名后是否重名
            var tmpNode = GetNode<T>(ownerId, path, newName);
            if (tmpNode != null && tmpNode.Equals(node))
            {
                throw new Exception("存在同名文件或目录.");
            }
            //重命名目录下面的所有子目录/文件的路径
            //连带isDeleted的记录一起重命名
            var oldFullPath = node.FullPath;
            node.Rename(newName);
            if (node is FolderNode)
            {
                var sql =
                    $"update p_Node set path='{node.FullPath}'+substring(path,len('{oldFullPath}')+1,8000) from p_Node" +
                    $" where ownerId='{ownerId}' and path like '{oldFullPath}%'";
                _panDbContext.Database.ExecuteSqlCommand(sql);
            }

            _panDbContext.SaveChanges();
            return node;
        }

        public IEnumerable<T> SearchNodes<T>(string ownerId, string path, string nodeName, string recursive) where T : Node
        {
            var folder = GetNode<FolderNode>(ownerId, path);
            if (folder == null)
            {
                return new T[0];
            }

            var nodes = folder.Children.OfType<T>().Where(c => c.Name.ToLower().Contains(nodeName.ToLower()));
            if (recursive == Boolean.TrueString)
            {
                nodes = nodes.Union(folder.SubFolders.SelectMany(c =>
                    SearchNodes<T>(ownerId, c.FullPath, nodeName, recursive)));
            }

            return nodes;
        }

        public IEnumerable<T> SearchNodesByFolderId<T>(string folderId, string nodeName, string recursive) where T : Node
        {
            var folder = GetNode<FolderNode>(folderId);
            if (folder == null)
            {
                return new T[0];
            }
            var nodes = folder.Children.OfType<T>().Where(n => n.Name.ToLower().Contains(nodeName.ToLower()));
            if (recursive == Boolean.TrueString)
            {
                nodes = nodes.Union(folder.SubFolders.SelectMany(n => SearchNodesByFolderId<T>(n.Id, nodeName, recursive)));
            }
            return nodes;
        }

        public int GetFilesCount(string ownerId, string path, string recursive)
        {
            return SearchNodes<FileNode>(ownerId, path, string.Empty, recursive).Count();
        }

        public string GetAppId(string fileId)
        {
            Domain.Node node = GetNode<FileNode>(fileId);
            QuotaLog log = null;
            while (log == null && node != null)
            {
                log = _panDbContext.QuotaLogs.FirstOrDefault(ql => ql.FileId == node.Id);
                if (log == null)
                {
                    //如果是CopyFolder产生的文件,只会存Folder的id,这里不断往上层遍历至该Folder
                    node = GetNode<FolderNode>(node.OwnerId, node.Path);
                }
            }
            if (log == null)
            {
                throw new Exception("无法在Log表中找到该文件的AppId");
            }
            return log.AppId;
        }

        public List<T> GetNodeList<T>(string nodeIdListStr) where T : Node
        {
            var nodeIds = nodeIdListStr.Split('/');
            var nodeList = new List<T>();
            if (nodeIds.Length > 0)
            {
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    var node = GetNode<T>(nodeIds[i]);
                    if (node != null)
                    {
                        nodeList.Add(node);
                    }
                }
            }

            return nodeList;
        }

        public List<Node> GetNodeList(string nodeIds, string ownerId)
        {
            var idList = nodeIds.Split('/');
            if (string.IsNullOrEmpty(nodeIds))
            {
                throw new Exception("错误的NodeIds");
            }

            var nodeList = new List<Domain.Node>();
            if (idList.Length > 0)
            {
                for (int i = 0; i < idList.Length; i++)
                {
                    var node = GetNode(idList[i], ownerId);

                    if (node == null)
                    {
                        throw new Exception("文件已删除或不存在");
                    }

                    nodeList.Add(node);
                }
            }

            return nodeList;
        }

        public T GetNode<T>(string nodeId) where T : Node
        {
            var node = _panDbContext.Nodes.OfType<T>().FirstOrDefault(c => !c.IsDeleted && c.Id == nodeId);
            if (node is FolderNode)
            {
                AttachChildren(node as FolderNode);
            }
            return node;
        }

        public T GetNodeWithNextLayer<T>(string ownerId, string path, string name) where T : Node
        {
            CheckPath(ref path);
            CheckName(name);
            Domain.Node node;
            if (typeof(T) == typeof(FolderNode) && path == FolderNode.ROOT_PATH)
            {
                //当用户不存在时,根目录依然返回一个node
                //这样就不存在初始化根目录这个步骤
                node = FolderNode.GetRootNode(ownerId);
            }
            else
            {
                node = GetNodes<T>(ownerId, path, name).FirstOrDefault();
            }
            if (node is FolderNode)
            {
                var folderNode = node as FolderNode;

                AttachChildren(folderNode);

                if (folderNode.SubFolders.Count() > 0)
                {
                    foreach (var item in folderNode.SubFolders.ToList())
                    {
                        AttachChildren(item);
                    }
                }
            }

            return node as T;
        }

        public Node GetNode(string nodeId)
        {
            var node = _panDbContext.Nodes.Where(c => !c.IsDeleted && c.Id == nodeId).FirstOrDefault();
            if (node is FolderNode)
            {
                AttachChildren(node as FolderNode);
            }

            return node;
        }

        public Node GetNode(string ownerId, string path, string name)
        {
            CheckPath(ref path);
            CheckName(name);
            Domain.Node node = GetNodes(ownerId, path, name).FirstOrDefault();
            return node;
        }

        public Node GetNode(string nodeId, string ownerId)
        {
            var node = _panDbContext.Nodes.FirstOrDefault(c => !c.IsDeleted && c.Id == nodeId && c.OwnerId == ownerId);
            if (node is FolderNode)
            {
                AttachChildren(node as FolderNode);
            }
            return node;
        }

        public T GetNode<T>(string ownerId, string path) where T : Node
        {
            CheckPath(ref path);
            var pathName = SplitFullPath(path);
            return GetNode<T>(ownerId, pathName.Item1, pathName.Item2);
        }

        public T GetNode<T>(string ownerId, string path, string name) where T : Node
        {
            CheckPath(ref path);
            CheckName(name);
            Domain.Node node;
            if (typeof(T) == typeof(FolderNode) && path == FolderNode.ROOT_PATH)
            {
                //当用户不存在时，根目录依然返回一个node
                //这样就不存在初始化根目录这个步骤
                node = FolderNode.GetRootNode(ownerId);
            }
            else
            {
                node = GetNodes<T>(ownerId, path, name).FirstOrDefault();
            }

            if (node is FolderNode)
            {
                AttachChildren(node as FolderNode);
            }
            return node as T;
        }

        /// <summary>
        ///  校验目录或文件名
        /// </summary>
        /// <param name="name"></param>
        private void CheckName(string name)
        {
            if (name.Contains("/") || name.Contains("\\"))
            {
                throw new Exception(@"文件名或目录名包含非法字符");
            }
        }

        /// <summary>
        /// path以/开头，如果没有以/结尾则会追加
        /// </summary>
        /// <param name="path"></param>
        private void CheckPath(ref string path)
        {
            path = '/' + path.Replace('\\', '/').TrimStart('/').TrimEnd('/') + '/';
            path = path.Replace("//", "/");
        }

        private Tuple<string, string> SplitFullPath(string fullPath)
        {
            if (fullPath == "/")
            {
                return Tuple.Create(FolderNode.ROOT_PATH, FolderNode.ROOT_NAME);
            }

            var path = fullPath.TrimEnd('/').Substring(0, fullPath.TrimEnd('/').LastIndexOf('/') + 1);
            var name = fullPath.TrimEnd('/').Substring(fullPath.TrimEnd('/').LastIndexOf('/') + 1);
            return Tuple.Create(path, name);
        }

        private Domain.Node DoCreateFolderNode(string ownerId, string path, string name)
        {
            var node = GetNode<FolderNode>(ownerId, path, name);
            //如果已经存在了,直接返回
            if (node == null)
            {
                //创建上层目录
                var pathName = SplitFullPath(path);
                var parentFolderNode = (FolderNode)DoCreateFolderNode(ownerId, pathName.Item1, pathName.Item2);
                //增加节点
                node = new FolderNode(ownerId, name, path, parentFolderNode);
                _panDbContext.Nodes.Add(node);
            }
            return node;
        }

        private Domain.Node DoCreateFolderNodeWithError(string ownerId, string path, string name)
        {
            var node = GetNode<FolderNode>(ownerId, path, name);
            //如果已经存在了,直接返回
            if (node == null)
            {
                //创建上层目录
                var pathName = SplitFullPath(path);
                var parentFolderNode = (FolderNode)DoCreateFolderNode(ownerId, pathName.Item1, pathName.Item2);
                //增加节点
                node = new FolderNode(ownerId, name, path, parentFolderNode);
                _panDbContext.Nodes.Add(node);
            }
            else
            {
                throw new Exception("存在同名目录.");
            }
            return node;
        }

        private Domain.Node DoDeleteNode(Domain.Node node)
        {
            if (node is FolderNode)
            {
                _panDbContext.Database.ExecuteSqlCommand($"update p_Node set isDeleted=1" +
                                                         $" from p_Node where ownerId='{node.OwnerId}' and (" +
                                                         $"path like '{node.FullPath}%' or id='{node.Id}')");
            }
            else
            {
                node.Delete();
                _panDbContext.SaveChanges();
            }
            return node;
        }

        private FolderNode DoEmptyFolder(FolderNode node)
        {
            _panDbContext.Database.ExecuteSqlCommand($"update p_Node set isDeleted=1 " +
                                                     $"from p_Node where ownerId='{node.OwnerId}' and path" +
                                                     $" like '{node.FullPath}%'");
            return node;
        }

        private IList<Domain.Node> GetNodes(string ownerId, string path, string name = null)
        {
            CheckPath(ref path);
            return _panDbContext.Nodes.Where(c => !c.IsDeleted && c.OwnerId == ownerId
                                                               && c.Path.ToLower() == path.ToLower() &&
                                                               (name == null || name == c.Name))
                .OrderBy(c => c.CreationTime).ToList();
        }
    }
}