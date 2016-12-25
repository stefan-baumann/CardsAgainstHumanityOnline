using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public static class Extensions
    {
        public static void WriteString(this HttpListenerContext context, string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
