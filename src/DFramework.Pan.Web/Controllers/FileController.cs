using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DFramework.MyStorage.SDK;

namespace DFramework.Pan.Web.Controllers
{
    public class FileController : PanControllerBase
    {
        public static ICacheManager _cacheManager = new MemoryCacheManager();

        public static int CacheTime = int.Parse(ConfigurationManager.AppSettings["CacheTime"]);

        //public static Dictionary<string, string> ContentTypesDictionary =
        //    System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentTypes.json"))
        //        .ToJsonObject<Dictionary<string, string>>();

        // GET: File
        public ActionResult Index()
        {
            return View();
        }
    }
}