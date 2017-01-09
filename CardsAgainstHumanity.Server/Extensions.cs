using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public static class Extensions
    {
        private const bool UseGzip = true;

        public static void WriteString(this HttpListenerContext context, string data)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);

                if (UseGzip && data.Length > 250)
                {
                    using (MemoryStream bufferStream = new MemoryStream(buffer.Length / 3 * 2))
                    {
                        using (GZipStream zip = new GZipStream(bufferStream, CompressionMode.Compress, true))
                        {
                            zip.Write(buffer, 0, buffer.Length);
                        }
                        buffer = bufferStream.ToArray();
                    }

                    context.Response.AddHeader("Content-Encoding", "gzip");
                }
                else
                {
                    context.Response.ContentEncoding = Encoding.UTF8;
                }

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch { }
        }
    }
}
