using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Abp.Json;
using DFramework.MyStorage.SDK;
using DFramework.Pan.Domain;
using DFramework.Pan.NodeAppServices;
using DFramework.Pan.QuotaAppServices;
using DFramework.Pan.ZipAppServices;

namespace DFramework.Pan.Web.Controllers
{
    public class FileController : PanControllerBase
    {
        public static ICacheManager _cacheManager = new MemoryCacheManager();

        public static int CacheTime = int.Parse(ConfigurationManager.AppSettings["CacheTime"]);

        public static Dictionary<string, string> ContentTypesDictionary =
            System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentTypes.json"))
                .FromJsonString<Dictionary<string, string>>();
        private readonly INodeAppService _nodeAppService;
        private readonly IStorageClient _storageClient;
        private readonly IQuotaAppService _quotaAppService;
        private readonly IZipAppService _zipAppService;

        public FileController(INodeAppService nodeAppService,IStorageClient storageClient,
            IQuotaAppService quotaAppService,IZipAppService zipAppService)
        {
            _nodeAppService = nodeAppService;
            _storageClient = storageClient;
            _quotaAppService = quotaAppService;
            _zipAppService = zipAppService;
        }

        public async Task<FileStreamData> GetFileStream(string ownerId, string fullPath, bool useCache)
        {
            Func<FileNode> func = () => _nodeAppService.GetNode<FileNode>(ownerId, fullPath);
            var file = useCache ? _cacheManager.Get<FileNode>($"{ownerId}/{fullPath}", CacheTime, func):func();
            return await _storageClient.GetFileStreamAsync(file.StorageFileId);
        }

        public async Task<ActionResult> Isolate(string fileId)
        {
            try
            {
                FileNode file = _cacheManager.Get(fileId, CacheTime, () => _nodeAppService.GetNode<FileNode>(fileId));
                var streamData = await _storageClient.GetFileStreamAsync(file.StorageFileId);
                return File(streamData.Stream, GetContentType(file.Name), file.Name);
            }
            catch (Exception e)
            {
                return new HttpNotFoundResult();
            }

        }

        public async Task<ActionResult> Index(string ownerId, string fullPath, bool useCache = true)
        {
            try
            {
                var streamData = await GetFileStream(ownerId, fullPath, useCache);
                return new FileStreamResult(streamData.Stream, GetContentType(fullPath));
            }
            catch (Exception e)
            {
                return new HttpNotFoundResult();
            }
        }

        public async Task<ActionResult> Download(string ownerId, string fullPath, bool useCache = true)
        {
            try
            {
                var streamData = await GetFileStream(ownerId, fullPath, useCache);
                return File(streamData.Stream, GetContentType(fullPath), GetFileName(fullPath));
            }
            catch (Exception e)
            {
                return new HttpNotFoundResult();
            }
        }

        public async Task<ActionResult> Thumb(string ownerId, string fullPath, bool useCache = true)
        {
            try
            {
                var width = 0;
                var height = 0;
                int.TryParse(Request.QueryString["w"], out width);
                int.TryParse(Request.QueryString["h"], out height);
                Func<FileNode> func = () =>
                    new NodeController(_storageClient, _nodeAppService, _quotaAppService, _zipAppService)
                        .CreateThumb(ownerId, fullPath, width, height, GetFileExtension(fullPath));

                var thumb = useCache
                    ? _cacheManager.Get($"{ownerId}/{fullPath}/{width}x{height}", CacheTime, func)
                    : func();

                var streamData = await GetFileStream(thumb.OwnerId, thumb.FullPath, useCache);
                return new FileStreamResult(streamData.Stream, GetContentType(fullPath));
            }
            catch (Exception e)
            {
                return new HttpNotFoundResult();
            }
        }


        public async Task<ActionResult> ZipDownload(string zipNodeId, string ownerId, string fullPath,
            bool useCache = true)
        {
            try
            {
                string zipName = GetFileName(fullPath);
                var zipLog = _zipAppService.GetZipLogByNodeId(zipNodeId);
                if (zipLog!=null)
                {
                    var node = _nodeAppService.GetNode(zipLog.InclusionIdList[0]);

                    if (node!=null)
                    {
                        if (zipLog.InclusionIdList.Length > 1)
                        {
                            var zipShowName = ConfigurationManager.AppSettings["ZipShowName"];
                            if (!string.IsNullOrEmpty(zipShowName))
                            {
                                zipName = string.Format(zipShowName, node.Name);
                            }
                        }
                        else
                        {
                            zipName = $"{node.Name}.zip";
                        }
                    }
                }

                var streamData = await GetFileStream(ownerId, fullPath, useCache);
                return File(streamData.Stream, GetContentType(fullPath), zipName);
            }
            catch (Exception e)
            {
                return new HttpNotFoundResult();
            }
        }

        private static string GetContentType(string fileName)
        {
            var contentType = "application/octet-stream";
            if (!string.IsNullOrEmpty(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                var ext = fi.Extension.ToLower();
                if (!string.IsNullOrEmpty(ext)&&ContentTypesDictionary.ContainsKey(ext))
                {
                    contentType = ContentTypesDictionary[ext];
                }
            }

            return contentType;
        }

        public static string GetFileName(string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                FileInfo fi = new FileInfo(fullPath);
                return fi.Name;
            }

            return "";
        }

        private static string GetFileExtension(string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                FileInfo fi = new FileInfo(fullPath);
                return fi.Extension;
            }
            return "";
        }
    }
}