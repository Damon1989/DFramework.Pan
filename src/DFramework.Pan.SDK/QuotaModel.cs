namespace DFramework.Pan.SDK
{
    public class QuotaModel
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public long Max { get; set; }
        public long Used { get; set; }
    }
}