namespace MinIONet.Service.Enums
{
    public enum StatusCode
    {
        Success = 00,
        Error = 01,
        NotFound = 02,
        RequireArgs=03,
        ExceptionOccur=04,
        ConnectionFail=05,
        FileAlreadyExist=06,
        ReachMaxFileSize=07,
        NotMatchFileContentType=08
    }
}
