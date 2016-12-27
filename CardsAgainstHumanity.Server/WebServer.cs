using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public class WebServerBase
    {
        protected HttpListener Listener { get; } = new HttpListener();
        
        public WebServerBase(string prefix)
        {
            this.Listener.Prefixes.Add(prefix);
        }
        
        public virtual void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            string response = $@"<html>
    <head>

    </head>
    <body>
        <h1>Webserver is running correctly</h1>
        <h2>Request Information</h2>
        <p>Url: {request.Url}</p>
        <p>Method: {request.HttpMethod}</p>
        <h3>Query</h3>
        {string.Join(Environment.NewLine, request.QueryString.AllKeys.Select(key => $"<p>{key}: {request.QueryString[key]}</p>"))}
    </body>
</html>";

            context.WriteString(response);
        }

        public void Start()
        {
            Console.WriteLine($"Starting webserver on {string.Join(", ", this.Listener.Prefixes)}...");
            this.Listener.Start();
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.WriteLine("Webserver successfully started.");
                while (this.Listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(innerState =>
                    {
                        HttpListenerContext context = (HttpListenerContext)innerState;
#if DEBUG
                        this.ProcessRequest(context);
                        context.Response.OutputStream.Close();
#else
                        try
                        {
                            this.ProcessRequest(context);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e.GetType().Name} caught: {e.Message}");
                        }
                        finally
                        {
                            context.Response.OutputStream.Close();
                        }
#endif
                    }, this.Listener.GetContext());
                }
            });
        }

        public void Stop()
        {
            Console.WriteLine("Stopping webserver...");
            this.Listener.Stop();
            this.Listener.Close();
            Console.WriteLine("Webserver successfully stopped.");
        }
    }
}