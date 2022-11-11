using System;
using System.IO;
using System.Threading.Tasks;

namespace EZShare.Common.Net
{
    public static class ContentTypes
    {
        public static string Json => "json";

        public static string ByteStream => "bytestream";

        public static string Text => "text";
    }

    public enum TransferCompressionTypes
    {
        Lzma,
        gzip
    }

    public class RemoteMessage
    {
        public RemoteMessage()
        {

        }

        public int ContentLength { get; }

        public string ContentType { get; }

        public string? SessionId { get; }

        public bool? TransferEncodingChunked { get; }

        public TransferCompressionTypes? TransferCompressionType { get; }

        public string? ContentPathName { get; }

        public Stream Content { get; }
    }

    public static class MessageExtensions
    {
        public static Task<RemoteMessage> GetRemoteMessage(this Stream stream)
        {
            throw new NotImplementedException();
        }

        public static Task<RemoteMessage> GetEncryptedRemoteMessage(this Stream stream)
        {
            throw new NotImplementedException();
        }

        public static Task<T> ReadContentAsJsonAsync<T>(this RemoteMessage remoteMessage)
        {
            throw new NotImplementedException();
        }

        public static Task<string> ReadContentAsText(this RemoteMessage remoteMessage)
        {
            throw new NotImplementedException();
        }
    }
}
