using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DFramework.Pan.Domain;
using DFramework.Pan.QuotaAppServices;
using DFramework.Pan.SDK;

namespace DFramework.Pan.Web.Controllers
{
    public class QuotaController : PanControllerBase
    {
        private readonly IQuotaAppService _quotaAppService;

        public QuotaController(IQuotaAppService quotaAppService)
        {
            _quotaAppService = quotaAppService;
        }

        public JsonResult GetQuota(string ownerId)
        {
            return CallService<QuotaModel>(false, new Func<string, Quota>(_quotaAppService.GetQuota), ownerId);
        }

        public JsonResult SetQuota(string ownerId, string size)
        {
            return CallService<QuotaModel>(true, new Func<string, string, Quota>(_quotaAppService.SetQuota), ownerId,
                size);
        }
    }
}