using System;
using Microsoft.Extensions.DependencyInjection;

namespace ApiCachingProxy
{
    public static class CachingProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddCachingProxy(
            this IServiceCollection services,
            Action<CachingProxyOptions> setupAction)
        {
            services
                .AddDistributedRedisCache(options =>
                {
                    options.Configuration = "127.0.0.1";
                })
                .AddHttpClient("HttpClient")
                .ConfigureHttpClient(client =>
                {
                    var options = new CachingProxyOptions();
                    setupAction(options);
                    client.BaseAddress = options.BaseUri;
                });

            return services;
        }
    }
}
