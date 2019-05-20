using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DFramework.MyStorage.SDK;
using DFramework.Pan.Domain;
using DFramework.Pan.Infrastructure;
using DFramework.Pan.NodeAppServices;
using DFramework.Pan.QuotaAppServices;
using DFramework.Pan.SDK;
using DFramework.Pan.ZipAppServices;

namespace DFramework.Pan.Web.Controllers
{
    public class NodeController : PanControllerBase
    {
        protected IStorageClient _storageClient;
        protected readonly INodeAppService _nodeAppService;
        protected readonly IQuotaAppService _quotaAppService;
        protected readonly IZipAppService _zipAppService;

        public NodeController(StorageClient storageClient,
                                            INodeAppService nodeAppService,
                                            IQuotaAppService quotaAppService,
                                            IZipAppService zipAppService)
        {
            _storageClient = storageClient;
            _nodeAppService = nodeAppService;
            _quotaAppService = quotaAppService;
            _zipAppService = zipAppService;
        }

        [HttpPost]
        public async Task<JsonResult> Upload(string ownerId, string path, string name, string md5, string tags)
        {
            return await CallServiceAsync<FileModel>(true,
                new Func<string, string, string, string, string, string, Task<FileNode>>(DoUpload), ownerId, path,
                name, md5, bool.FalseString, tags);
        }

        [HttpPost]
        public async Task<JsonResult> UploadOverwritten(string ownerId, string path, string name, string md5,
            string tags)
        {
            return await CallServiceAsync<FileModel>(true,
                new Func<string, string, string, string, string, string, Task<FileNode>>(DoUpload),
                ownerId, path, name, md5, bool.TrueString, tags);
        }

        [HttpPost]
        public async Task<JsonResult> UploadIsolate(string ownerId, string name, string md5, string tags)
        {
            return await CallServiceAsync<FileModel>(true,
                new Func<string, string, string, string, Task<FileNode>>(DoUploadIsolate), ownerId,
                name, md5, tags);
        }

        [HttpPost]
        private async Task<FileNode> DoUploadIsolate(string ownerId, string name, string md5, string tags = null)
        {
            return await DoUpload(ownerId, null, name, md5, bool.FalseString, tags/*孤立文件无所谓覆盖问题，每次都是新建*/);
        }

        public JsonResult GetFIle(string ownerId, string path, string name)
        {
            return CallService<FileModel>(false,
                new Func<string, string, string, Domain.Node>(_nodeAppService.GetNode<FileNode>), ownerId, path, name);
        }

        public JsonResult GetFileByFullPath(string ownerId, string fullPath)
        {
            return CallService<FileModel>(false,
                new Func<string, string, Domain.Node>(_nodeAppService.GetNode<FileNode>), ownerId, fullPath);
        }

        public JsonResult GetFileById(string fileId)
        {
            return CallService<FileModel>(false, new Func<string, Domain.Node>(_nodeAppService.GetNode<FileNode>),
                fileId);
        }

        public JsonResult GetFilesByIds(string fileIdListStr)
        {
            return CallService<IEnumerable<FileModel>>(false,
                new Func<string, IEnumerable<Domain.Node>>(_nodeAppService.GetNodeList<FileNode>), fileIdListStr);
        }

        public JsonResult GetFolder(string ownerId, string path)
        {
            return CallService<FolderModel>(false,
                new Func<string, string, Domain.Node>(_nodeAppService.GetNode<FolderNode>), ownerId, path);
        }

        public JsonResult GetFolderById(string nodeId)
        {
            return CallService<FolderModel>(false,
                new Func<string, string, Domain.Node>(_nodeAppService.GetNodeById<FolderNode>), nodeId,
                bool.FalseString);
        }

        public JsonResult GetFolderWithNextLayer(string ownerId, string path)
        {
            return CallService<FolderModel>(false,
                new Func<string, string, Domain.Node>(_nodeAppService.GetNodeWithNextLayer<FolderNode>), ownerId, path);
        }

        public JsonResult GetFolderByIdWithNextLayer(string nodeId)
        {
            return CallService<FolderModel>(false,
                new Func<string, string, Domain.Node>(_nodeAppService.GetNodeById<FolderNode>), nodeId,
                bool.TrueString);
        }

        public JsonResult CreateFolder(string ownerId, string path, string name)
        {
            return CallService<FolderModel>(true,
                new Func<string, string, string, FolderNode>(_nodeAppService.CreateFolderNode), ownerId, path, name);
        }

        public JsonResult CreateFolderWithError(string ownerId, string path, string name)
        {
            return CallService<FolderModel>(true,
                new Func<string, string, string, FolderNode>(_nodeAppService.CreateFolderNodeWithError), ownerId, path,
                name);
        }

        public JsonResult RenameFile(string ownerId, string path, string oldName, string newName)
        {
            return CallService<FileModel>(true,
                new Func<string, string, string, string, Domain.Node>(_nodeAppService.RenameNode<FileNode>), ownerId,
                path, oldName, newName);
        }

        public JsonResult RenameFolder(string ownerId, string path, string oldName, string newName)
        {
            return CallService<FolderModel>(true,
                new Func<string, string, string, string, Domain.Node>(_nodeAppService.RenameNode<FolderNode>), ownerId,
                path, oldName, newName);
        }

        public JsonResult MoveFile(string ownerId, string path, string name, string newPath)
        {
            return CallService<FileModel>(true,
                new Func<string, string, string, string, Domain.Node>(_nodeAppService.MoveNode<FileNode>), ownerId,
                path, name, newPath);
        }

        public JsonResult MoveFolder(string ownerId, string path, string name, string newPath)
        {
            return CallService<FolderModel>(true,
                new Func<string, string, string, string, Domain.Node>(_nodeAppService.MoveNode<FolderNode>), ownerId,
                path, name, newPath);
        }

        public JsonResult MoveNodes(string ownerId, string path, string name, string newPath)
        {
            return CallService(true, new Action<string, string, string, string>(_nodeAppService.MoveNodes), ownerId,
                path, name, newPath);
        }

        public JsonResult MoveNodesToTargetOwner(string ownerId, string path, string nameListStr, string targetOwnerId,
            string newPath)
        {
            return CallService(true, new Action<string, string, string, string, string>((_ownerId, _path, _nameListStr, _targetOwnerId, _newPath) =>
            {
                string[] nameList = _nameListStr?.Split('/');
                if (nameList?.Length > 0)
                {
                    foreach (var name in nameList)
                    {
                        var node = _nodeAppService.GetNode(_ownerId, _path, name);
                        if (node == null)
                        {
                            throw new Exception("文件或文件夹不存在");
                        }
                        if (node is FileNode)
                        {
                            if (_ownerId != _targetOwnerId)
                            {
                                var size = (node as FileNode).Size;
                                _quotaAppService.CheckQuota(_targetOwnerId, size);
                            }

                            var result = _nodeAppService.MoveNode<FileNode>(_ownerId, _path, name, _targetOwnerId, _newPath) as FileNode;

                            if (_ownerId != _targetOwnerId)
                            {
                                _quotaAppService.Decrease(_ownerId, result.Size, result.Id);
                                _quotaAppService.Increase(_targetOwnerId, result.Size, result.Id);
                            }
                        }
                        else if (node is FolderNode)
                        {
                            long size = 0;
                            if (_ownerId != _targetOwnerId)
                            {
                                size = _nodeAppService.GetFolderSize(_ownerId, path, name);
                                _quotaAppService.CheckQuota(_targetOwnerId, size);
                            }

                            var result = _nodeAppService.MoveNode<FolderNode>(_ownerId, _path, name, _targetOwnerId, _newPath);

                            if (_ownerId != _targetOwnerId)
                            {
                                _quotaAppService.Decrease(_ownerId, size, result.Id);
                                _quotaAppService.Increase(_targetOwnerId, size, result.Id);
                            }
                        }
                    }
                }
            }), ownerId, path, nameListStr, targetOwnerId, newPath);
        }

        public JsonResult CopyNodes(string ownerId, string path, string nameListStr, string newPath)
        {
            return CallService(true, new Action<string, string, string, string>((_ownerId, _path, _nameListStr, _newPath) =>
            {
                string[] nameList = _nameListStr?.Split('/');
                if (nameList?.Length > 0)
                {
                    foreach (var name in nameList)
                    {
                        var node = _nodeAppService.GetNode(_ownerId, _path, name);
                        if (node == null)
                        {
                            throw new Exception("文件或文件夹不存在");
                        }
                        if (node is FileNode)
                        {
                            var size = (node as FileNode).Size;
                            _quotaAppService.CheckQuota(_ownerId, size);
                            var result = _nodeAppService.CopyNode<FileNode>(_ownerId, _path, name, _newPath, null) as FileNode;
                            _quotaAppService.Increase(_ownerId, result.Size, result.Id);
                        }
                        else if (node is FolderNode)
                        {
                            var size = _nodeAppService.GetFolderSize(_ownerId, path, name);
                            _quotaAppService.CheckQuota(_ownerId, size);
                            var result = _nodeAppService.CopyNode<FolderNode>(_ownerId, _path, name, _newPath, null);
                            _quotaAppService.Increase(_ownerId, size, result.Id);
                        }
                    }
                }
            }), ownerId, path, nameListStr, newPath);
        }

        public JsonResult CopyNodesToTargetOwner(string ownerId, string path, string nameListStr, string targetOwnerId, string newPath)
        {
            return CallService(true, new Action<string, string, string, string, string>((_ownerId, _path, _nameListStr, _targetOwnerId, _newPath) =>
            {
                string[] nameList = _nameListStr?.Split('/');
                if (nameList?.Length > 0)
                {
                    foreach (var name in nameList)
                    {
                        var node = _nodeAppService.GetNode(_ownerId, _path, name);
                        if (node == null)
                        {
                            throw new Exception("文件或文件夹不存在");
                        }
                        if (node is FileNode)
                        {
                            var size = (node as FileNode).Size;
                            _quotaAppService.CheckQuota(_targetOwnerId, size);
                            var result = _nodeAppService.CopyNode<FileNode>(_ownerId, _path, name, _targetOwnerId, _newPath, null) as FileNode;
                            _quotaAppService.Increase(_targetOwnerId, result.Size, result.Id);
                        }
                        else if (node is FolderNode)
                        {
                            var size = _nodeAppService.GetFolderSize(_ownerId, path, name);
                            _quotaAppService.CheckQuota(_targetOwnerId, size);
                            var result = _nodeAppService.CopyNode<FolderNode>(_ownerId, _path, name, _targetOwnerId, _newPath, null);
                            _quotaAppService
.Increase(_targetOwnerId, size, result.Id);
                        }
                    }
                }
            }), ownerId, path, nameListStr, targetOwnerId, newPath);
        }

        public JsonResult CopyFile(string ownerId, string path, string name, string newPath, string newName = null)
        {
            return CallService<FileModel>(true, new Func<string, string, string, string, Domain.Node>((p1, p2, p3, p4) =>
            {
                var size = _nodeAppService.GetNode<FileNode>(ownerId, path, name).Size;
                _quotaAppService.CheckQuota(ownerId, size);

                var result = _nodeAppService.CopyNode<FileNode>(p1, p2, p3, p4, newName) as FileNode;

                _quotaAppService.Increase(ownerId, result.Size, result.Id);
                return result;
            }), ownerId, path, name, newPath);
        }

        public JsonResult CopyFolder(string ownerId, string path, string name, string newPath, string newName = null)
        {
            return CallService<FolderModel>(true, new Func<string, string, string, string, Domain.Node>((p1, p2, p3, p4) =>
            {
                var size = _nodeAppService.GetFolderSize(ownerId, path, name);
                _quotaAppService.CheckQuota(ownerId, size);

                var result = _nodeAppService.CopyNode<FolderNode>(p1, p2, p3, p4, newName);

                _quotaAppService.Increase(ownerId, size, result.Id);
                return result;
            }), ownerId, path, name, newPath);
        }

        public JsonResult CopyFileById(string fileId, string targetOwnerId, string newPath, string newName = null)
        {
            return CallService<FileModel>(true, new Func<string, string, string, Domain.Node>((p1, p2, p3) =>
            {
                var size = _nodeAppService.GetNode<FileNode>(fileId
).Size;
                _quotaAppService.CheckQuota(targetOwnerId, size);

                var result = _nodeAppService.CopyNode<FileNode>(p1, p2, p3, newName) as FileNode;

                _quotaAppService.Increase(targetOwnerId, result.Size, result.Id);
                return result;
            }), fileId, targetOwnerId, newPath);
        }

        public JsonResult CopyFolderById(string folderId, string targetOwnerId, string newPath, string newName = null)
        {
            return CallService<FolderModel>(true, new Func<string, string, string, Domain.Node>((p1, p2, p3) =>
            {
                var size = _nodeAppService.GetFolderSize(folderId);
                _quotaAppService.CheckQuota(targetOwnerId, size);

                var result = _nodeAppService.CopyNode<FolderNode>(p1, p2, p3, newName);

                _quotaAppService.Increase(targetOwnerId, size, result.Id);
                return result;
            }), folderId, targetOwnerId, newPath);
        }

        public JsonResult DeleteFolder(string ownerId, string path, string name)
        {
            return CallService<FolderModel>(true, new Func<string, string, string, Domain.Node>((p1, p2, p3) =>
            {
                DoDeleteFolder(ownerId, path, name);
                return _nodeAppService.DeleteNode<FolderNode>(p1, p2, p3) as FolderNode;
            }), ownerId, path, name);
        }

        public JsonResult EmptyFolder(string folderId)
        {
            return CallService<FolderModel>(true, new Func<String, Domain.Node>((p1) =>
            {
                var folder = _nodeAppService.GetNode<FolderNode>(folderId) as FolderNode;
                if (folder == null)
                {
                    //throw new Exception("未找到要删除的目录");
                    return folder;
                }
                DoDeleteFolder(folder.OwnerId, folder.Path, folder.Name);
                return _nodeAppService.EmptyFolder(folder.Id);
            }), folderId);
        }

        public JsonResult DeleteNodes(string ownerId, string path, string nameListStr)
        {
            return CallService(true, new Action<string, string, string>((p1, p2, p3) =>
            {
                string[] nameList = p3?.Split('/');
                if (nameList?.Length > 0)
                {
                    foreach (var name in nameList)
                    {
                        var node = _nodeAppService.GetNode(p1, p2, name);
                        if (node == null)
                        {
                            throw new Exception("文件或文件夹不存在");
                        }
                        if (node is FileNode)
                        {
                            var result = _nodeAppService.DeleteNode<FileNode>(p1, p2, name) as FileNode;
                            _storageClient.Remove(result.StorageFileId);
                            _quotaAppService.Decrease(p1, result.Size, result.Id);
                            FileNodeChanged(node as FileNode);
                        }
                        else if (node is FolderNode)
                        {
                            DoDeleteFolder(p1, p2, name);
                            _nodeAppService.DeleteNode<FolderNode>(p1, p2, name);
                        }
                    }
                }
            }), ownerId, path, nameListStr);
        }

        public JsonResult DeleteFile(string ownerId, string path, string name)
        {
            return CallService<FileModel>(true, new Func<string, string, string, Domain.Node>((p1, p2, p3) =>
            {
                var node = _nodeAppService.DeleteNode<FileNode>(p1, p2, p3) as FileNode;
                _storageClient.Remove(node.StorageFileId);
                _quotaAppService.Decrease(ownerId, node.Size, node.Id);
                FileNodeChanged(node);

                return node;
            }), ownerId, path, name);
        }

        public JsonResult SearchFiles(string ownerId, string path, string fileName, bool recursive)
        {
            return CallService<IEnumerable<FileModel>>(false,
                new Func<string, string, string, string, IEnumerable<FileNode>>(_nodeAppService.SearchNodes<FileNode>),
                ownerId, path, fileName, recursive.ToString());
        }

        public JsonResult SearchFolders(string ownerId, string path, string folderName, bool recursive)
        {
            return CallService<IEnumerable<FolderNode>>(false,
                new Func<string, string, string, string, IEnumerable<FolderNode>>(_nodeAppService
                    .SearchNodes<FolderNode>), ownerId, path, folderName, recursive.ToString());
        }

        public JsonResult SearchFilesByFolderId(string folderId, string fileName, bool recursive)
        {
            return CallService<IEnumerable<FileModel>>(false,
                new Func<string, string, string, IEnumerable<FileNode>>(_nodeAppService
                    .SearchNodesByFolderId<FileNode>), folderId, fileName, recursive.ToString());
        }

        public JsonResult SearchFoldersByFolderId(string folderId, string folderName, bool recursive)
        {
            return CallService<IEnumerable<FolderNode>>(false,
                new Func<string, string, string, IEnumerable<FolderNode>>(_nodeAppService
                    .SearchNodesByFolderId<FolderNode>), folderId, folderName, recursive.ToString());
        }

        public JsonResult GetFilesCount(string ownerId, string path, bool recursive)
        {
            return CallService<int>(false, new Func<string, string, string, int>(_nodeAppService.GetFilesCount), ownerId, path, recursive.ToString());
        }

        internal FileNode CreateThumb(string ownerId, string fullPath, int width, int height, string contentType = "")
        {
            var file = _nodeAppService.GetNode<FileNode>(ownerId, fullPath) as FileNode;
            if (!file.IsImage)
            {
                throw new Exception("不是图片文件,无法制作缩略图");
            }

            //GetAppId会找到原始文件上传时的appid,将生成的缩略图算在该app上
            //但考虑性能问题,把appid设为固定的一个id,即所有生成的缩略图都算在该app上
            //var thumbOwnerId = nodeService.GetAppId(file.Id);

            var thumbPath = String.Format("/Thumb/{0}", file.Id);
            var thumbName = String.Format("{0}x{1}", width, height);
            var thumb = _nodeAppService.GetNode<FileNode>(ThumbOwnerId, thumbPath, thumbName) as FileNode;
            if (thumb != null)
                return thumb;
            else
            {
                var imageStream = ImageUtil.Thumbnail(_storageClient.GetFileStream(file.StorageFileId).Stream, width, height, contentType);
                var storageFile = _storageClient.Upload(imageStream);
                var fileNode = _nodeAppService.CreateFileNode(ThumbOwnerId, thumbPath, thumbName, storageFile.Size, storageFile.Id);
                //var quotaService = new QuotaService(appId: ThumbOwnerId);
                _quotaAppService.Increase(ThumbOwnerId, storageFile.Size, fileNode.Id);
                return fileNode;
            }
        }

        private async Task<FileNode> DoUpload(string ownerId,
                                                                            string path,
                                                                            string name,
                                                                            string md5,
                                                                            string overwritten,
                                                                            string tags = null)
        {
            FileData storageFileByMd5 = null;
            long size = 0;
            HttpPostedFileBase file = null;
            if (!string.IsNullOrEmpty(md5))//根据md5直接获取storage file
            {
                try
                {
                    storageFileByMd5 = _storageClient.GetFileData(md5);
                    size = storageFileByMd5.Size;
                }
                catch (Exception e)
                {
                    throw new SysException(Infrastructure.ErrorCode.FileMd5NotFound);
                }
            }
            else //根据Request.File获取file
            {
                if (Request.Files.Count != 1)
                {
                    throw new SysException(Infrastructure.ErrorCode.OnlyOneFileAllowed, "只能上传一个文件." + Request.Files.Count);
                }

                file = Request.Files[0];
                size = file.ContentLength;
            }

            FileNode fileToOverwrite = (path == null ? null : _nodeAppService.GetNode<FileNode>(ownerId, path, name));
            var isModify = fileToOverwrite != null && overwritten == Boolean.TrueString;

            //检查配额，及检查文件是否存在，放在这里做可以让Upload/Modify调storageClient.Upload的时序一致
            long sizeToChange = size;
            if (isModify)
            {
                sizeToChange -= fileToOverwrite.Size;
            }
            else if (fileToOverwrite != null)
            {
                throw new Exception("文件已存在.");
            }

            _quotaAppService.CheckQuota(ownerId, sizeToChange);

            //storage上传
            var storageFile = storageFileByMd5 ?? await _storageClient.UploadAsync(file.InputStream);

            //增加/更新文件节点
            FileNode fileNode = null;
            if (isModify)
            {
                _storageClient.Remove(fileToOverwrite.StorageFileId);
                _quotaAppService.Decrease(ownerId, fileToOverwrite.Size, fileToOverwrite.Id);
                fileNode = _nodeAppService.ModifyFileNode(fileToOverwrite, size, storageFile.Id);
            }
            else
            {
                fileNode = _nodeAppService.CreateFileNode(ownerId, path, name, size, storageFile.Id, tags);
            }

            _quotaAppService.Increase(ownerId, size, fileNode.Id);
            FileNodeChanged(fileNode);
            return fileNode;
        }

        //文件更新,删除，文件夹删除：清理文件缓存,删除缩略图
        //文件重命名，移动，文件夹重命名，移动：不清楚李缓存（等待过去），不删除缩略图（文件id还是这个）
        private void FileNodeChanged(FileNode node)
        {
            //清理读缓存
            FileController._cacheManager.Remove(node.OwnerId + node.FullPath);
            //包括缩略图的缓存，有w和h后缀
            var folderNodes = _nodeAppService.GetNode<FolderNode>(ThumbOwnerId, $"/Thumb/{node.Id}")?.Files;
            foreach (var folderNode in folderNodes)
            {
                FileController._cacheManager.Remove(node.OwnerId + node.FullPath + "/" + folderNode.Name);
            }

            //删除原有缩略图
            if (node.IsImage)
            {
                DeleteFolder(ThumbOwnerId, "/Thumb", node.Id);
            }
        }

        public async Task<JsonResult> CreateNodeZip(string nodeIds, string appId, string path)
        {
            var result = await ExceptionManager.Process(async () =>
            {
                if (string.IsNullOrEmpty(nodeIds))
                {
                    throw new Exception("错误的NodeIds");
                }

                string[] idLists = nodeIds.Split('/');
                FileNode zipFile = null;
                List<Domain.Node> nodeList = _nodeAppService.GetNodeList<Domain.Node>(nodeIds);
                if (nodeList.Count > 0)
                {
                    var keyIds = GetZipKey(nodeList);
                    keyIds.Sort();

                    string zipKeys = Newtonsoft.Json.JsonConvert.SerializeObject(keyIds);
                    var zipLog = _zipAppService.GetSameKeyZip(zipKeys);
                    if (zipLog != null && !string.IsNullOrEmpty(zipLog.NodeId))
                    {
                        zipFile = _nodeAppService.GetNode(zipLog.NodeId) as FileNode;
                    }
                    else
                    {
                        var zipId = Guid.NewGuid().ToString().ToLower();
                        using (MemoryStream stream = new MemoryStream())
                        {
                            #region 构建压缩包

                            using (var zipArchive = CreateZipArchive(stream, ZipArchiveMode.Update))
                            {
                                foreach (var node in nodeList)
                                {
                                    if (node is FileNode)
                                    {
                                        try
                                        {
                                            var fileNode = node as FileNode;
                                            var nodeStream =
                                                await _storageClient.GetFileStreamAsync(fileNode.StorageFileId);
                                            await CreateFile(zipArchive, node.Name, nodeStream.Stream);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            throw;
                                        }
                                    }
                                    else if (node is FolderNode)
                                    {
                                        await CreateFolder(node.Name, node.OwnerId, node.Path + node.Name + "/",
                                            zipArchive, null, true);
                                    }
                                }
                            }

                            #endregion 构建压缩包

                            stream.Seek(0, SeekOrigin.Begin);
                            var md5 = stream.GetFileMD5();
                            await _storageClient.UploadAsync(stream);
                            zipFile = await DoUpload(appId, path, zipId + ".zip", md5, Boolean.FalseString);
                        }

                        _zipAppService.AddZipLog(zipId, zipKeys, zipFile.Id, idLists);
                    }
                }

                var zipFileModel = PrepareResult(AutoMapper.Mapper.Map<FileModel>(zipFile));
                return zipFileModel;
            });
            return Json(result);
        }

        private async Task CreateFolder(string folderName, string folderOwnerId, string folderPath,
            ZipArchive zipArchive, ZipArchiveEntry zipArchiveEntry, bool isArchive)
        {
            ZipArchiveEntry zipFileFolder;
            if (isArchive)
            {
                zipFileFolder = CreateFolder(zipArchive, folderName);
            }
            else
            {
                zipFileFolder = CreateFolder(zipArchiveEntry, folderName);
            }

            //var folderContents = nodeService.GetNode<FolderNode>(node.OwnerId, node.Path);
            var folderContent = PrepareResult(Mapper.Map<FolderModel>(new Func<String, String, Domain.Node>(_nodeAppService.GetNode<FolderNode>).DynamicInvoke(folderOwnerId, folderPath)));
            if (folderContent.Files.Count() > 0)
            {
                foreach (var file in folderContent.Files.ToList())
                {
                    HttpClient client = new HttpClient();

                    try
                    {
                        using (var fileStream = await client.GetStreamAsync(file.DownloadUrl))
                        {
                            await CreateFile(zipFileFolder, file.Name, fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    //fileStream.Seek(0, SeekOrigin.Begin);
                }
            }
            if (folderContent.SubFolders.Count() > 0)
            {
                foreach (var folder in folderContent.SubFolders.ToList())
                {
                    await CreateFolder(folder.Name, folder.OwnerId, folder.Path + folder.Name + "/", zipArchive, zipFileFolder, false);
                }
            }
        }

        private static ZipArchive CreateZipArchive(Stream stream, ZipArchiveMode model)
        {
            return new ZipArchive(stream, model, true);
        }

        private static ZipArchiveEntry CreateFolder(ZipArchive zipArchive, string folder)
        {
            if (folder.Last() != '/' || folder.Last() != '\\')
            {
                folder = folder + "/";
            }
            return zipArchive.CreateEntry(folder, CompressionLevel.Optimal);
        }

        private static ZipArchiveEntry CreateFolder(ZipArchiveEntry zipArchiveEntry, string folder)
        {
            if (folder.Last() != '/' || folder.Last() != '\\')
            {
                folder = folder + "/";
            }
            return zipArchiveEntry.Archive.CreateEntry(Path.Combine(zipArchiveEntry.FullName, folder), CompressionLevel.Optimal);
        }

        private static async Task CreateFile(ZipArchive zipArchive, string fileName, Stream stream)
        {
            ZipArchiveEntry fileEntry = zipArchive.CreateEntry(fileName);
            using (var entryStream = fileEntry.Open())
            {
                await stream.CopyToAsync(entryStream);
            }
        }

        private static async Task CreateFile(ZipArchiveEntry zipEntry, string fileName, Stream stream)
        {
            var fileEntry = zipEntry.Archive.CreateEntry(Path.Combine(zipEntry.FullName, fileName));
            using (var entryStream = fileEntry.Open())
            {
                await stream.CopyToAsync(entryStream);
            }
        }

        private List<string> GetZipKey(List<Domain.Node> nodeList)
        {
            var nodeIds = new List<string>();
            nodeList.ForEach(node =>
            {
                nodeIds.Add(node.Id.ToLower());
                if (node is FolderNode)
                {
                    var folderNode = node as FolderNode;
                    if (folderNode.Children.Any())
                    {
                        var childrenNodeList =
                            _nodeAppService.GetNodeList<Domain.Node>(string.Join("/",
                                folderNode.Children.Select(c => c.Id).ToArray()));
                        var childrenNodeIds = GetZipKey(childrenNodeList);
                        nodeIds.AddRange(childrenNodeIds);
                    }
                }
            });

            return nodeIds;
        }

        private void DoDeleteFolder(String ownerId, String path, String name)
        {
            var folder = _nodeAppService.GetNode<FolderNode>(ownerId, path, name) as FolderNode;
            //匿名函数的递归(奇葩)
            Func<Action<FolderNode>, Action<FolderNode>> g = x => x = f =>
            {
                if (f == null || f.IsEmpty)
                    return;
                f.Files.ForEach(file =>
                {
                    //从文件系统删除
                    _storageClient.Remove(file.StorageFileId);
                    //从配额扣除
                    _quotaAppService.Decrease(ownerId, file.Size, file.Id);
                    //清理缓存
                    FileNodeChanged(file);
                });
                f.SubFolders.ForEach(subFolder => x(_nodeAppService.GetNode<FolderNode>(ownerId, subFolder.FullPath) as FolderNode));
            };
            g(null)(folder);
        }

        public JsonResult GetNodeList(string nodeIds, string ownId)
        {
            var result = ExceptionManager.Process(() =>
            {
                var nodelist = _nodeAppService.GetNodeList(nodeIds, ownId);

                List<NodeModel> nodeModels = new List<NodeModel>();

                foreach (var item in nodelist)
                {
                    NodeModel nodeModel = new NodeModel
                    {
                        Id = item.Id,
                        OwnerId = item.OwnerId,
                        Name = item.Name,
                        Path = item.Path
                    };

                    //nodeModel.CreateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm");

                    nodeModels.Add(nodeModel);
                }

                return nodeModels;
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVirtualFolderModel(string nodeIds)
        {
            var result = ExceptionManager.Process(() =>
            {
                if (string.IsNullOrEmpty(nodeIds))
                {
                    throw new Exception("错误的NodeIds");
                }

                string[] idLists = nodeIds.Split('/');

                var nodelist = _nodeAppService.GetNodeList<Domain.Node>(nodeIds);

                VirtualFolderModel folder = new VirtualFolderModel();

                List<FileModel> subFiles = new List<FileModel>();
                List<FolderModel> subFolders = new List<FolderModel>();

                foreach (var item in nodelist)
                {
                    if (item is FileNode)
                    {
                        subFiles.Add(PrepareResult(Mapper.Map<FileModel>(item)));
                    }
                    else if (item is FolderNode)
                    {
                        subFolders.Add(PrepareResult(Mapper.Map<FolderModel>(item)));
                    }
                }

                folder.Files = subFiles;
                folder.SubFolders = subFolders;

                if (subFiles.Count == 0 && subFolders.Count == 0)
                {
                    folder.IsEmpty = true;
                }

                return folder;
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNode(string nodeId)
        {
            var result = ExceptionManager.Process(() =>
            {
                NodeModel nodeModel = new NodeModel();

                var node = _nodeAppService.GetNode(nodeId);

                if (node != null)
                {
                    nodeModel.Id = node.Id;
                    nodeModel.OwnerId = node.OwnerId;
                    nodeModel.Name = node.Name;
                    nodeModel.Path = node.Path;
                    //nodeModel.CreationTime = node.CreationTime.ToString("yyyy-MM-dd HH:mm");
                    if (node is FileNode)
                    {
                        nodeModel.Type = "File";
                    }
                    else if (node is FolderNode)
                    {
                        nodeModel.Type = "Folder";
                    }
                }

                return nodeModel;
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}