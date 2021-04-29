using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sol_Demo.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ApiMemoryCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiMemoryCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var memoryCacheService = httpContext.RequestServices.GetRequiredService<IMemoryCache>();

            if (!memoryCacheService.TryGetValue("ResponseCacheKey", out string data))
            {
                data = GetApiResponse();

                var cahceEntryOption = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10));

                memoryCacheService.Set<String>("ResponseCacheKey", data, cahceEntryOption);
            }

            return _next(httpContext);
        }

        private String GetApiResponse()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
                //HTTP GET
                var responseTask = client.GetAsync("todos/1").Result;
                var data = responseTask.Content?.ReadAsStringAsync()?.Result;

                return data;
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiMemoryCacheMiddlewareExtensionExtensions
    {
        public static IApplicationBuilder UseApiMemoryCache(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiMemoryCacheMiddleware>();
        }
    }
}