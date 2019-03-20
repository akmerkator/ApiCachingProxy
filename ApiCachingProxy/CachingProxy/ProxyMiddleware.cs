using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;

using Polly;
using Polly.Caching.Distributed;

namespace ApiCachingProxy
{
    public static class ProxyMiddlewareExtensions
    {
        public static void RunCachingProxy(this IApplicationBuilder app)
        {
            var factory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();
            app.Run(HandleRequest(factory));
        }

        private static RequestDelegate HandleRequest(IHttpClientFactory factory)
            => async httpContext =>
            {
                var cache = httpContext.RequestServices.GetService<IDistributedCache>();
                var path = httpContext.Request.Path.HasValue
                    ? httpContext.Request.Path.Value
                    : "/";

                var cacheKey = GetCacheKey(httpContext, path);
                var cachePolicy = GetCachePolicy(cache);
                var client = factory.CreateClient("HttpClient");
                var response = await Policy
                    .Handle<HttpRequestException>()
                    .Retry(2)
                    .Execute(() => cachePolicy.ExecuteAsync(
                        async ctx => await GetResponse(httpContext, client, path),
                        cacheKey));

                var buffer = Encoding.ASCII.GetBytes(response);
                await httpContext.Response.Body.WriteAsync(buffer);
            };

        private static Polly.Caching.AsyncCachePolicy<string> GetCachePolicy(IDistributedCache cache)
            => Policy.CacheAsync(cache.AsAsyncCacheProvider<string>(), TimeSpan.FromDays(10));

        private static async Task<string> GetResponse(
            HttpContext context,
            HttpClient client,
            string path)
        {
            var uri = new Uri(client.BaseAddress, path);
            switch (context.Request.Method)
            {
                case "GET":
                    var response = await client.GetAsync(uri);
                    return await response.Content.ReadAsStringAsync();
                case "POST":
                    return await GetPostAsync(context, client, path)
                        .Result
                        .Content
                        .ReadAsStringAsync();
                default:
                    break;
            }

            return null;
        }

        private static Context GetCacheKey(HttpContext context, string path)
        {
            switch (context.Request.Method)
            {
                case "GET":
                    return new Context(path);
                case "POST":
                    return new Context(path + "\n" + context.Request.Body.ToString());
                default:
                    return new Context();
            }
        }

        private static async Task<HttpResponseMessage> GetPostAsync(
            HttpContext context,
            HttpClient client,
            string path)
        {
            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(client.BaseAddress, path)))
            {
                foreach (var header in context.Request.Headers)
                    request.Headers.Add(header.Key, header.Value.ToString());

                return await client.SendAsync(request);
            }
        }
    }
}
