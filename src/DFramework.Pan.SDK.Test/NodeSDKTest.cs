using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFramework.Pan.SDK.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DFramework.Pan.SDK.Test
{
    [TestClass]
    public class NodeSDKTest
    {
        private static string appId = "app1";

        private static string host = "http://localhost:61754/";
        private string ownerId = "user1";
        private string targetOwnerId = "user2";

        private IPanClient client = new PanClient(host, appId);
        private IQuotaClient quotaClient = new QuotaClient(host, appId);

        private readonly string testFilePath =
            Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
                "DFramework.Pan.SDK.Test\\TestFiles");

        [TestMethod]
        public void Cleanup()
        {
            var db = new DbContext("MyPanConnectionString");
            db.Database.ExecuteSqlCommand("delete from p_node where ownerId=@p0", ownerId);
            db.Database.ExecuteSqlCommand("delete from p_node where ownerId=@p0", targetOwnerId);
            db.Database.ExecuteSqlCommand("delete from p_Quota where ownerId=@p0", ownerId);
            db.Database.ExecuteSqlCommand("delete from p_Quota where ownerId=@p0", targetOwnerId);
            db.Database.ExecuteSqlCommand("delete from p_QuotaLog where ownerId=@p0", ownerId);
            db.Database.ExecuteSqlCommand("delete from p_QuotaLog where ownerId=@p0", targetOwnerId);
        }


        [TestMethod]
        public void CreateFolder()
        {
            var folder = client.CreateFolder(ownerId, "/folder1/folder2/folder3", "folder4");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.Name == "folder4");
            Assert.IsTrue(folder.Path == "/folder1/folder2/folder3/");
            Assert.IsTrue(folder.IsEmpty);

            var folder1 = client.CreateFolder(ownerId, "/", "folder11");
            Assert.IsNotNull(folder1);
            Assert.IsTrue(folder1.Name == "folder11");
            Assert.IsTrue(folder1.Path == "/");
            Assert.IsTrue(folder1.IsEmpty);

            var folder3 = client.CreateFolder(ownerId, "/folder1/folder2/", "folder31");
            Assert.IsNotNull(folder3);
            Assert.IsTrue(folder3.Name == "folder31");
            Assert.IsTrue(folder3.Path == "/folder1/folder2/");
            Assert.IsTrue(folder3.IsEmpty);

            var folderC = client.CreateFolder(ownerId, "/folder1/folder2/", "中文 目录~!@#$%^&*()_+-=[]{};':,.<>?");
            Assert.IsTrue(folderC.Name == "中文 目录~!@#$%^&*()_+-=[]{};':,.<>?");
        }

        [TestMethod]
        public void GetFile()
        {
            var file = client.GetFile(ownerId, "/folder1/", "file1.txt");
            Assert.IsNotNull(file);

            var file2 = client.GetFile(file.Id);
            Assert.IsNotNull(file2);
        }

        [TestMethod]
        public void GetFileStream()
        {
            var file = client.GetFile(ownerId, "/folder1/", "file1.txt");
            Assert.IsNotNull(file);

            var content = File.ReadAllText(Path.Combine(testFilePath, "damon.txt"));
            var fileString = client.GetFileContent(file.Id).Result;
            Assert.AreEqual(content, fileString);
        }

        [TestMethod]
        public void GetFileByFullPath()
        {
            var file = client.GetFile(ownerId, "/folder1/file1.txt");
            Assert.IsNotNull(file);
            Assert.IsTrue(file.Name == "file1.txt");
        }


        [TestMethod]
        public void GetFolder()
        {
            var folder = client.GetFolder(ownerId, "/");
            Assert.IsNotNull(folder);
            Assert.IsTrue(folder.Files != null);
            Assert.IsTrue(folder.SubFolders != null);
            Assert.AreEqual(folder.SubFolders.Count(), 1);
            Assert.AreEqual(folder.Files.Count(), 0);

            var folder1 = client.GetFolder(ownerId, "/folder1");
            Assert.IsNotNull(folder1);
            Assert.IsTrue(folder1.Files != null);
            Assert.IsTrue(folder1.Files.All(file => file.Url == host + "File/" + ownerId + file.Path + file.Name));

            client.CreateFolder(ownerId, "/folder1/folder2", "folder3");
            var folderWithLayer = client.GetFolderByIdWithNextLayer(folder1.Id);
            Assert.IsNotNull(folderWithLayer);
            Assert.AreEqual(folderWithLayer.SubFolders.Count(), 2);
            Assert.AreEqual(folderWithLayer.SubFolders.FirstOrDefault(f => f.Name == "folder2").SubFolders.Count(), 1);

            client.CreateFolder(ownerId, "/folder1/folder2", "folder5");
            var folder5 = client.GetFolder(ownerId, "/folder1/folder2/folder5");
            Assert.IsTrue(folder5.IsEmpty);

            client.DeleteFolder(ownerId, "/folder1/folder2", "folder5");
        }

        [TestMethod]
        public void UploadFile()
        {
            Cleanup();
            quotaClient.SetQuota(ownerId, 1048576);
            var oldQuota = quotaClient.GetQuota(ownerId).Used;
            long fileSize = 0;
            using (var stream=File.OpenRead(Path.Combine(testFilePath,"damon.txt")))
            {
                var file = client.Upload(ownerId, "/folder1", "file1.txt", stream, FileExistStrategy.Fault, "damon");
                Assert.IsNotNull(file);
                Assert.IsTrue(file.Name == "file1.txt");
                Assert.AreEqual("damon", file.Tags);
                fileSize += file.Size;
            }

            using (var stream=File.OpenRead(Path.Combine(testFilePath,"damon.txt")))
            {
                //第二次上传，应该是返回同一个storage
                var file1 = client.Upload(ownerId, "/folder1/folder2/", "file2.txt", stream,FileExistStrategy.Fault);
                Assert.IsNotNull(file1);
                Assert.IsTrue(file1.Name == "file2.txt");
                fileSize += file1.Size;
            }

            using (var stream=File.OpenRead(Path.Combine(testFilePath,"1.jpg")))
            {
                var file2 = client.Upload(ownerId, "/folder1/folder21", "file21.txt", stream, FileExistStrategy.Fault);
                Assert.IsNotNull(file2);
                Assert.AreEqual(file2.Name, "file21.txt");
                Assert.AreEqual(file2.Url, host + "File/" + ownerId + file2.Path+file2.Name);
            }

            //上传一个孤立文件
            using (var stream=File.OpenRead(Path.Combine(testFilePath,"1.jpg")))
            {
                var file = client.UploadIsolate(ownerId, "isolate.jpg", stream, "isolate");
                Assert.IsNotNull(file);
                Assert.IsTrue(file.Name == "isolate.jpg");
                var fileIsolate = client.GetFile(file.Id);
                Assert.IsNotNull(fileIsolate);
                Console.WriteLine(fileIsolate.Url);
                Assert.IsTrue(fileIsolate.Url == host + "Isolate/" + file.Id);
                Assert.IsTrue(fileIsolate.DownloadUrl == fileIsolate.Url);
                Assert.IsTrue(fileIsolate.ThumbUrl == String.Empty);
                Assert.IsTrue(fileIsolate.Tags == "isolate");

                fileSize += file.Size;
            }

            //异步
            using (var stream=File.OpenRead(Path.Combine(testFilePath,"damon.txt")))
            {
                var file = client.UploadAsync(ownerId, "/folderAsync", "fileAsync.txt", stream, FileExistStrategy.Fault)
                    .Result;
                Assert.IsNotNull(file);
                Assert.IsTrue(file.Name == "fileAsync.txt");
                fileSize += file.Size;
            }
            //上传后重命名
            using (var stream=File.OpenRead(Path.Combine(testFilePath,"damon.txt")))
            {
                var file = client.UploadAsync(ownerId, "/folderAsync/folderRename", "fileRename.txt", stream,
                    FileExistStrategy.Rename).Result;
                Assert.IsNotNull(file);
                Assert.IsTrue(file.Name == "fileRename.txt");
                fileSize += file.Size;

                for (int i = 1; i <= 3; i++)
                {
                    file = client.UploadAsync(ownerId, "/folderAsync/folderRename", "fileRename.txt", stream, FileExistStrategy.Rename).Result;
                    Assert.IsNotNull(file);
                    Assert.IsTrue(file.Name == String.Format("fileRename({0}).txt", i));
                    fileSize += file.Size;
                }
            }

            var newQuota = quotaClient.GetQuota(ownerId).Used;
            //Assert.AreEqual(oldQuota + fileSize, newQuota);
        }
    }
}
