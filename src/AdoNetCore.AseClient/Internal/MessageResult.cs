namespace AdoNetCore.AseClient.Internal
{
    internal sealed class MessageResult
    {
        public AseErrorCollection Errors { get; set; }
        public string Message { get; set; }
    }
}