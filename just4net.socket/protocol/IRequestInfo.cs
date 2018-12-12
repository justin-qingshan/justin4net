namespace just4net.socket.protocol
{
    public interface IRequestInfo
    {
        string Key { get; }
    }

    public interface IRequestInfo<TRequestBody> : IRequestInfo
    {
        TRequestBody Body { get; }
    }

    public interface IRequestInfo<TRequestHeader, TRequestBody> : IRequestInfo<TRequestBody>
    {
        TRequestHeader Header { get; }
    }
}
