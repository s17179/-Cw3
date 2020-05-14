using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApplication.Midllewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null)
            {
                string path = httpContext.Request.Path;
                string querystring = httpContext.Request?.QueryString.ToString();
                string method = httpContext.Request.Method.ToString();
                string bodyStr = "";

                using (StreamReader reader
                    = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }
                
                Console.WriteLine(path);
                Console.WriteLine(querystring);
                Console.WriteLine(method);
                Console.WriteLine(bodyStr);

                File.AppendAllText("requestsLog.txt", $"Metoda: {method}" + Environment.NewLine);
                File.AppendAllText("requestsLog.txt", $"Ścieżka: {path}" + Environment.NewLine);
                File.AppendAllText("requestsLog.txt", $"Query string: {querystring}" + Environment.NewLine);
                File.AppendAllText("requestsLog.txt", $"Body: {bodyStr}" + Environment.NewLine);
                File.AppendAllText("requestsLog.txt", Environment.NewLine);
            }
            
            await _next(httpContext);
        }
    }
}