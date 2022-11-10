using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;

namespace EZShare.Common.Net
{
    public class PackageHeaders
    {
        public PackageHeaders()
        {

        }

        public int ContentLength { get; }

        public string ContentType { get; }

        public string? SessionId { get; }

        public bool? TransferEncodingChunked { get; }

        public TransferCompressionTypes? TransferCompressionType { get; }

        public string? ContentPathName { get; }
    }

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

    public static class PackageExtensions
    {

    }
}
