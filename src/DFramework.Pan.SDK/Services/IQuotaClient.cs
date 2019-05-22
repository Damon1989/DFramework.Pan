namespace DFramework.Pan.SDK.Services
{
    public interface IQuotaClient
    {
        QuotaModel GetQuota(string ownerId);
        QuotaModel SetQuota(string ownerId, long size);
    }
}