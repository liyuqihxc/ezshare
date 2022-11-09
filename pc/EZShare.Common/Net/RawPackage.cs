using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;

namespace EZShare.Common.Net
{
    public class RawPackage
    {
        public RawPackage()
        {

        }

        public int ContentLength { get; }

        public string ContentType { get; }

        public object Body { get; }
    }

    public static class ContentTypes
    {
        public static string Json => nameof(Json);

        public static string ByteStream => nameof(ByteStream);

        public static string Text => nameof(Text);
    }
}
