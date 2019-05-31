using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Abp.Json;
using Abp.Web.Models;
using Abp.Web.Mvc.Controllers;
using AutoMapper;
using DFramework.Pan.Domain;
using DFramework.Pan.Infrastructure;
using DFramework.Pan.SDK;
using ErrorCode = DFramework.Pan.Infrastructure.ErrorCode;

namespace DFramework.Pan.Web.Controllers
{
    /// <summary>
    /// Derive all Controllers from this class.
    /// </summary>
    [DontWrapResult]
    public abstract class PanControllerBase : AbpController
    {
        protected PanControllerBase()
        {
            LocalizationSourceName = PanConsts.LocalizationSourceName;
        }

        protected string GetHeader(string headerName)
        {
            string header = null;
            var result = ExceptionManager.Process(() =>
            {
                try
                {
                    header = Request.Headers[headerName];
                }
                catch (Exception e)
                {
                    header = System.Web.HttpContext.Current.Request.Headers[headerName];
                }

                if (string.IsNullOrEmpty(header))
                {
                    throw new Exception($"必须将{headerName}加入到HttpHeader中");
                }
            });
            if (result.ErrorCode != ErrorCode.NoError)
            {
                System.Web.HttpContext.Current.Response.Write(result.ToJsonString());
                System.Web.HttpContext.Current.Response.End();
            }

            return header;
        }

        protected string CurrentAppId => GetHeader("appId");

        protected string ThumbOwnerId => ConfigurationManager.AppSettings["AppId"];

        protected JsonResult CallService(bool isPost, Delegate method, params string[] args)
        {
            var apiResult = ExceptionManager.Process(() =>
            {
                ValidateMethod(method, args);
                return PrepareResult(method.DynamicInvoke(args));
            });
            return Json(apiResult, isPost ? JsonRequestBehavior.DenyGet : JsonRequestBehavior.AllowGet);
        }

        protected JsonResult CallService<T>(bool isPost, Delegate method, params string[] args)
        {
            var apiResult = ExceptionManager.Process<T>(() =>
            {
                ValidateMethod(method, args);

                return PrepareResult(Mapper.Map<T>(method.DynamicInvoke(args)));
            });
            return Json(apiResult, isPost ? JsonRequestBehavior.DenyGet : JsonRequestBehavior.AllowGet);
        }

        protected async Task<JsonResult> CallServiceAsync<T>(bool isPost, Delegate method, params string[] args)
        {
            var apiResult = await ExceptionManager.Process<T>(async () =>
            {
                ValidateMethod(method, args);
                var result = await (method.DynamicInvoke(args) as Task<FileNode>);
                return PrepareResult(Mapper.Map<T>(result));
            });
            return Json(apiResult, isPost ? JsonRequestBehavior.DenyGet : JsonRequestBehavior.AllowGet);
        }

        public T PrepareResult<T>(T model)
        {
            //全局返回值处理
            //HttpRuntime.AppDomainAppVirtualPath 返回“/”或“/VirtualPath”,所有这里处理让它始终以“/”结尾
            //更新：现在从webconfig读出了，但“/”的逻辑是一样的
            var host = ConfigurationManager.AppSettings["PanHost"];
            if (string.IsNullOrEmpty(host))
            {
                host = $"{Request.Url.Scheme}://{Request.Url.Authority}{HttpRuntime.AppDomainAppVirtualPath}";
            }

            if (!host.EndsWith("/"))
            {
                host += "/";
            }

            var fileModel = model as FileModel;
            if (fileModel != null)
            {
                fileModel.Host = host;
            }

            var folderModel = model as FolderModel;
            if (folderModel != null)
            {
                foreach (var file in folderModel.Files)
                {
                    file.Host = host;
                }
            }

            var fileModelList = model as List<FileModel>;
            if (fileModelList != null)
            {
                foreach (var item in fileModelList)
                {
                    item.Host = host;
                }
            }

            return model;
        }

        /// <summary>
        /// 校验参数
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        private static void ValidateMethod(Delegate method, string[] args)
        {
            //全局验证参数
            var methodParams = method.Method.GetParameters();
            for (int i = 0; i < methodParams.Length; i++)
            {
                if (methodParams[i].Name.ToLower() == "md5" ||
                    methodParams[i].Name.ToLower() == "tags") //不需要验证的参数
                {
                    continue;
                }

                if (string.IsNullOrEmpty(args[i]))
                {
                    throw new Exception($"{methodParams[i].Name}不能为空");
                }
            }
        }
    }
}