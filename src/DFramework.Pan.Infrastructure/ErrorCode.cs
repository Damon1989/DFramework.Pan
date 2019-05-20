namespace DFramework.Pan.Infrastructure
{
    public enum ErrorCode
    {
        NoError,

        AccountNotExist = 1000,
        OnlyOneFileAllowed,
        FileMd5NotFound,
        UnknownError
    }
}