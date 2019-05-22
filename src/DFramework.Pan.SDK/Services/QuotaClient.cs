using System;
using System.Collections.Generic;
using DFramework.Pan.Infrastructure;

namespace DFramework.Pan.SDK.Services
{
    public class QuotaClient:IQuotaClient
    {
        HttpUtility _httpClient;

        public QuotaClient(string host, string appId)
        {
            if (string.IsNullOrEmpty(host)||string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("host和appId是必选参数");
            }
            _httpClient = new HttpUtility(host, new Dictionary<string, string> { { "appId", appId } });
        }

        public QuotaModel GetQuota(string ownerId)
        {
            var url = $"Quota/GetQuota?ownerId={ownerId}";
            return _httpClient.Get<QuotaModel>(url);
        }

        public QuotaModel SetQuota(string ownerId, long size)
        {
            var url = "Quota/SetQuota";
            return _httpClient.Post<QuotaModel>(url, new Dictionary<string, string>
            {
                {"ownerId", ownerId}, {"size", size.ToString()}
            });
        }
    }
}