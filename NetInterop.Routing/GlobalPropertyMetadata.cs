namespace NetInterop.Routing
{
    public class GlobalPropertyMetadata
    {
        public GlobalPropertyMetadata()
        {
        }

        public GlobalPropertyMetadata(GlobalPropertyMetadataOptions options)
        {
            GlobalPropertyMetadataOptions = options;
        }

        public GlobalPropertyMetadataOptions GlobalPropertyMetadataOptions { get; set; }
    }
}