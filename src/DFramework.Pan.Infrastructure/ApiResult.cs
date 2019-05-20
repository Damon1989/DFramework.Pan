namespace DFramework.Pan.Infrastructure
{
    public class ApiResult
    {
        public ErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public class ApiResult<TResult> : ApiResult
    {
        public TResult Result { get; set; }

        public ApiResult()
        {
        }

        public ApiResult(TResult result)
        {
            Result = result;
        }
    }
}