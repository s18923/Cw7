using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw5.Middlewares
{
    public class LoggingMiddleware
    {

        private readonly RequestDelegate next;
        public LoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {

            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null)
            {
                string sciezka = httpContext.Request.Path;
                string querystring = httpContext.Request?.QueryString.ToString();
                string metoda = httpContext.Request.Method.ToString();
                string bodyStr = "";

                using (StreamReader reader
                 = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }

                File.AppendAllText("log.txt", sciezka + querystring + metoda + bodyStr);
            }

            await next(httpContext); 
        }

    }
}

