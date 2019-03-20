﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiCachingProxy
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
            => services.AddCachingProxy(options =>
                {
                    options.BaseUri = new Uri("http://localhost:5555");
                });

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            => app.RunCachingProxy();
    }
}
