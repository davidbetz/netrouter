namespace NetInterop.Routing
{
    public class ResponseResult : Result
    {
        public ResponseResult(IHeader header)
        {
            Header = header;
        }

        public IHeader Header { get; set; }
    }
}