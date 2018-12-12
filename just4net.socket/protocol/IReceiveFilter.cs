namespace just4net.socket.protocol
{
    public interface IReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest);

        int LeftBufferSize { get; }

        IReceiveFilter<TRequestInfo> NextReceiveFilter { get; }

        void Reset();

        FilterState State { get; }

        int OffsetDelta { get; }
    }

    public enum FilterState
    {
        Normal = 0,
        Error = 1
    }
}
