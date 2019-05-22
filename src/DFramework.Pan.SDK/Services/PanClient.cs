using System;
using DFramework.Pan.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFramework.Pan.SDK.Services
{
    public class PanClient:IPanClient
    {
        private HttpUtility _httpClient;

        public PanClient(string host, string appId)
        {
            if (string.IsNullOrEmpty(host)||string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("host和appId是必选参数");
            }

            _httpClient = new HttpUtility(host, new Dictionary<string, string> {{"appId", appId}});
        }

        public FileModel CopyFile(string ownerId, string path, string name, string newPath, FileExistStrategy strategy,
            string newName = "")
        {
            if (strategy==FileExistStrategy.Overwrite||strategy==FileExistStrategy.Rename)
            {
                throw new System.NotImplementedException();
            }

            var url = "Node/CopyFile";
            return _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}, {"newPath", newPath}, {"newName", newName}
            });
        }

        public FileModel CopyFile(string fileId, string targetOwnerId, string newPath, FileExistStrategy strategy,
            string newName = "")
        {
            if (strategy==FileExistStrategy.Overwrite||strategy==FileExistStrategy.Rename)
            {
                throw new System.NotImplementedException();
            }

            var url = "Node/CopyFileById";
            return _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"fileId", fileId}, {"targetOwnerId", targetOwnerId}, {"newPath", newPath}, {"newName", newName}
            });
        }

        public FileModel CopyFIle(string ownerId, string path, string name, string targetOwnerId, string newPath,
            FileExistStrategy strategy, string newName = "")
        {
            var file = GetFile(ownerId, path, name);
            return CopyFile(file.Id, targetOwnerId, newPath, strategy, newName);
        }

        public FolderModel CopyFolder(string folderId, string targetOwnerId, string newPath)
        {
            var url = "Node/CopyFolderById";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"folderId", folderId}, {"targetOwnerId", targetOwnerId}, {"newPath", newPath}
            });
        }

        public FolderModel CopyFolder(string ownerId, string path, string name, string newPath)
        {
            var url = "Node/CopyFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}, {"newPath", newPath}
            });
        }

        public void CopyNodes(string ownerId, string path, string[] nameList, string newPath)
        {
            var url = "Node/CopyNodes";
            _httpClient.Post(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"nameListStr", string.Join("/", nameList)}, {"newPath", newPath}
            });
        }

        public void CopyNodes(string ownerId, string path, string[] nameList, string targetOwnerId, string newPath)
        {
            var url = "Node/CopyNodesToTargetOwner";
            _httpClient.Post(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"nameListStr", string.Join("/", nameList)},
                {"targetOwnerId", targetOwnerId}, {"newPath", newPath}
            });
        }

        public FolderModel CreateFolder(string ownerId, string path, string name)
        {
            var url = "Node/CreateFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}
            });
        }

        public FileModel DeleteFile(string ownerId, string path, string name)
        {
            var url = "Node/DeleteFile";
            return _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}
            });
        }

        public FolderModel DeleteFolder(string ownerId, string path, string name)
        {
            var url = "Node/DeleteFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}
            });
        }

        public void DeleteNodes(string ownerId, string path, string[] nameList)
        {
            var url = "Node/DeleteNodes";
            _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"nameListStr", string.Join("/", nameList)}
            });
        }

        public FolderModel EmptyFolder(string folderId)
        {
            var url = "Node/EmptyFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string> {{"folderId", folderId}});
        }

        public FileModel GetFile(string ownerId, string path, string name)
        {
            var url = $"Node/GetFile?ownerId={ownerId}&path={path}&name={name}";
            return _httpClient.Get<FileModel>(url);
        }

        public FileModel GetFile(string ownerId, string fullPath)
        {
            var url = $"Node/GetFileByFullPath?ownerId={ownerId}&fullPath={fullPath}";
            return _httpClient.Get<FileModel>(url);
        }

        public FileModel GetFile(string fileId)
        {
            var url = $"Node/GetFileById?fileId={fileId}";
            return _httpClient.Get<FileModel>(url);
        }

        public List<FileModel> GetFiles(string[] fileIds)
        {
            var fileIdsStr = string.Join("/", fileIds);
            var url = $"Node/GetFilesByIds?fileIdListStr={fileIdsStr}";
            return _httpClient.Get<List<FileModel>>(url);
        }

        public Task<string> GetFileContent(string fileId)
        {
            var file = GetFile(fileId);
            if (file!=null)
            {
                var client = new HttpClient();
                return client.GetStringAsync(file.Url);
            }

            return null;
        }

        public int GetFilesCount(string ownerId, string path, bool recursive = false)
        {
            var url = $"Node/GetFilesCount?ownerId={ownerId}&path={path}&recursive={recursive}";
            return _httpClient.Get<int>(url);
        }

        public Task<Stream> GetFileStream(string fileId)
        {
            var file = GetFile(fileId);
            if (file!=null)
            {
                var client = new HttpClient();
                return client.GetStreamAsync(file.Url);
            }

            return null;
        }

        public FolderModel GetFolder(string ownerId, string path)
        {
            var url = $"Node/GetFolder?ownerId={ownerId}&path={path}";
            return _httpClient.Get<FolderModel>(url);
        }

        public long GetFolderSize(string ownerId, string path)
        {
            var url = "Node/GetFolderSize";
            return _httpClient.Post<long>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}
            });
        }

        public FileModel Modify(string fileId, Stream stream, string tags = null)
        {
            return ModifyAsync(fileId, stream, tags).Result;
        }

        public FileModel Modify(string ownerId, string path, string name, Stream stream, string tags = null)
        {
            return ModifyAsync(ownerId, path, name, stream, tags).Result;
        }

        public Task<FileModel> ModifyAsync(string fileId, Stream stream, string tags = null)
        {
            var oldFile = GetFile(fileId);
            return UploadAsync(oldFile.OwnerId, oldFile.Path, oldFile.Name, stream, FileExistStrategy.Overwrite, tags);
        }

        public Task<FileModel> ModifyAsync(string ownerId, string path, string name, Stream stream, string tags = null)
        {
            return UploadAsync(ownerId, path, name, stream, FileExistStrategy.Overwrite, tags);
        }

        public FileModel MoveFile(string ownerId, string path, string name, string newPath)
        {
            var url = "Node/MoveFile";
            return _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}, {"newPath", newPath}
            });
        }

        public FolderModel MoveFolder(string ownerId, string path, string name, string newPath)
        {
            var url = "Node/MoveFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}, {"newPath", newPath}
            });
        }

        public void MoveNodes(string ownerId, string path, string[] nameList, string newPath)
        {
            var url = "Node/MoveNodes";
            _httpClient.Post(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", string.Join("/", nameList)}, {"newPath", newPath}
            });
        }

        public void MoveNodes(string ownerId, string path, string[] nameList, string targetOwnerId, string newPath)
        {
            var url = "Node/MoveNodesToTargetOwner";
            _httpClient.Post(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"nameListStr", string.Join("/", nameList)},
                {"targetOwnerId", targetOwnerId}, {"newPath", newPath}
            });
        }

        public FileModel RenameFile(string ownerId, string path, string oldName, string newName)
        {
            var url = "Node/RenameFile";
            return _httpClient.Post<FileModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"oldName", oldName}, {"newName", newName}
            });
        }

        public FolderModel RenameFolder(string ownerId, string path, string oldName, string newName)
        {
            var url = "Node/RenameFolder";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"oldName", oldName}, {"newName", newName}
            });
        }

        public NodeModel[] SearchNodes(string ownerId, string path, string nodeName, NodeType nodeType, bool recursive = false)
        {
            var nodes = new List<NodeModel>();
            if (nodeType.HasFlag(NodeType.File))
            {
                var url =
                    $"Node/SearchFiles?ownerId={ownerId}&path={path}&fileName={nodeName}&recursive={recursive.ToString()}";
                nodes.AddRange(_httpClient.Get<FileModel[]>(url));
            }

            if (nodeType.HasFlag(NodeType.Folder))
            {
                var url =
                    $"Node/SearchFolders?ownerId={ownerId}&path={path}&folderName={nodeName}&recursive={recursive.ToString()}";
                nodes.AddRange(_httpClient.Get<FolderModel[]>(url));
            }

            return nodes.ToArray();
        }

        public NodeModel[] SearchNodesByFolderId(string folderId, string nodeName, NodeType nodeType, bool recursive = false)
        {
            var nodes = new List<NodeModel>();
            if (nodeType.HasFlag(NodeType.File))
            {
                var url = $"Node/SearchFiles?folderId={folderId}&fileName={nodeName}&recursive={recursive.ToString()}";
                nodes.AddRange(_httpClient.Get<FileModel[]>(url));
            }

            if (nodeType.HasFlag(NodeType.Folder))
            {
                var url =
                    $"Node/SearchFolders?folderId={folderId}&folderName={nodeName}&recursive={recursive.ToString()}";
                nodes.AddRange(_httpClient.Get<FolderModel[]>(url));
            }

            return nodes.ToArray();
        }

        public FileModel Upload(string ownerId, string path, string fileName, Stream stream, FileExistStrategy strategy,
            string tags = null)
        {
            return UploadAsync(ownerId, path, fileName, stream, strategy, tags).Result;
        }

        /// <summary>
        /// 上传文件  增加了上传同名时的策略
        /// </summary>
        /// <param name="ownerId">作者</param>
        /// <param name="path">路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <param name="strategy"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Task<FileModel> UploadAsync(string ownerId, string path, string fileName, Stream stream, FileExistStrategy strategy,
            string tags = null)
        {
            if (stream==null)
            {
                throw new System.Exception("文件流为空.");
            }

            var md5 = stream.GetFileMD5();

            return DoUploadAsync(ownerId, path, fileName, null, md5, strategy, tags)
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        var innerEx = t.Exception.InnerException;
                        if (innerEx is SysException && (innerEx as SysException).ErrorCode == ErrorCode.FileMd5NotFound)
                        {
                            return DoUploadAsync(ownerId, path, fileName,stream, string.Empty, strategy, tags);//整个文件上传;
                        }
                        else
                        {
                            throw innerEx;
                        }
                    }
                    else
                    {
                        return t;
                    }
                }).Unwrap();
        }

        public FileModel UploadIsolate(string ownerId, string fileName, Stream fileStream, string tags = null)
        {
            return UploadIsolateAsync(ownerId, fileName, fileStream, tags).Result;
        }

        public Task<FileModel> UploadIsolateAsync(string ownerId, string fileName, Stream stream, string tags = null)
        {
            return UploadAsync(ownerId, null, fileName, stream, FileExistStrategy.Fault, tags);
        }

        /// <summary>
        /// 检测是否所有Node都属于OwnId，任意一个不属于则报错
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public List<NodeModel> GetNodeList(string[] nodeIds, string ownerId)
        {
            var nodeIdsStr = string.Join("/", nodeIds);
            var url = $"Node/GetNodeList?nodeIds={nodeIdsStr}&ownId={ownerId}";
            return _httpClient.Get<List<NodeModel>>(url);
        }

        public string GetZipDownloadUrl(string[] nodeIds, string appId, string path)
        {
            var url = "Node/CreateNodeZip";
            var fileModel = _httpClient.PostAsync<FileModel>(url, new Dictionary<string, string>
            {
                {"nodeIds", string.Join("/", nodeIds)}, {"appId", appId}, {"path", path}
            }).Result;
            return fileModel.DownloadUrl.Replace("/Download/", $"/ZipDownload/{fileModel.Id}/");
        }

        /// <summary>
        ///  通过NodeId获取Node信息，不论是文件还是文件夹
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public NodeModel GetNode(string nodeId)
        {
            var url = $"Node/GetNode?nodeId={nodeId}";
            return _httpClient.Get<NodeModel>(url);
        }

        /// <summary>
        ///  通过nodeIds构成一个虚拟的Folder，并不实际存在于网盘中
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        public VirtualFolderModel GetVirtualFolderModel(string[] nodeIds)
        {
            var url = $"Node/GetVirtualFolderModel?nodeIds={string.Join("/", nodeIds)}";
            return _httpClient.Get<VirtualFolderModel>(url);
        }

        /// <summary>
        ///  获取路径和路径下子文件夹中的所有文件及文件夹
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderModel GetFolderWithNextLayer(string ownerId, string path)
        {
            var url = $"Node/GetFolderWithNextLayer?ownerId={ownerId}&path={path}";
            return _httpClient.Get<FolderModel>(url);
        }


        /// <summary>
        ///  创建文件夹，若有同名文件夹，报错
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FolderModel CreateFolderWithError(string ownerId, string path, string name)
        {
            var url = "Node/CreateFolderWithError";
            return _httpClient.Post<FolderModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"path", path}, {"name", name}
            });
        }

        /// <summary>
        /// 根据Id获取文件夹
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public FolderModel GetFolderById(string nodeId)
        {
            var url = $"Node/GetFolderById?nodeId={nodeId}";
            return _httpClient.Get<FolderModel>(url);
        }

        /// <summary>
        ///  根据Id获取文件夹及其子文件夹中的所有文件及文件夹
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public FolderModel GetFolderByIdWithNextLayer(string nodeId)
        {
            var url = $"Node/GetFolderByIdWithNextLayer?nodeId={nodeId}";
            return _httpClient.Get<FolderModel>(url);
        }

        private FileModel DoUpload(string ownerId, string path, string name, Stream fileStream, string md5,
            FileExistStrategy strategy)
        {
            return DoUploadAsync(ownerId, path, name, fileStream, md5, strategy).Result;
        }

        private Task<FileModel> DoUploadAsync(string ownerId, string path, string name, Stream fileStream, string md5,
            FileExistStrategy strategy, string tags = null)
        {
            var content = new MultipartFormDataContent();
            string url = null;
            if (path!=null)
            {
                if (strategy==FileExistStrategy.Rename)
                {
                    var sequence = 1;
                    var oldName = Path.GetFileNameWithoutExtension(name);
                    var oldExtension = Path.GetExtension(name);
                    while (GetFile(ownerId, path, name) != null)
                    {
                        name = $"{oldName}({sequence++}){oldExtension}";
                    }
                }

                content.Add(new StringContent(path), @"""path""");
                url = strategy == FileExistStrategy.Overwrite ? "Node/UploadOverwritten" : "Node/Upload";
            }
            else
            {
                url = "Node/UploadIsolate";
            }


            if (fileStream!=null)
            {
                content.Add(new StreamContent(fileStream), "file", "\"" + name + "\"");
            }

            content.Add(new StringContent(ownerId), @"""ownerId""");
            content.Add(new StringContent(name), @"""name""");
            content.Add(new StringContent(md5), @"""md5""");
            content.Add(new StringContent(tags??""), @"""tags""");

            return _httpClient.PostAsync<FileModel>(url, null, content);
        }
    }
}