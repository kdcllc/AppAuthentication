﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerGenWithApiVersion(
            this IServiceCollection services,
            string appName = null )
        {
            services.AddVersionedApiExplorer();

            //TODO: fix it once this is resolve https://github.com/microsoft/aspnet-api-versioning/issues/499
            //services.AddApiVersioning(o =>
            //{
            //    o.ReportApiVersions = true;
            //    o.AssumeDefaultVersionWhenUnspecified = true;
            //});

            services.AddMvcCore().AddApiExplorer()
                .AddAuthorization()
                .AddFormatterMappings()
                .AddCacheTagHelper()
                .AddDataAnnotations();

            services.AddSwaggerGen(options =>
            {
                // build intermediate container once.
                var sp = services.BuildServiceProvider();
                var config = sp.GetRequiredService<IConfiguration>();

                var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
                var appliationName = appName ?? config[WebHostDefaults.ApplicationKey];

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(
                        description.GroupName,

                        new OpenApiInfo()
                        {
                            Title = $"{appliationName} API {description.ApiVersion}",
                            Version = description.ApiVersion.ToString()
                        });
                }

                options.IncludeXmlComments(GetXmlDocPath(appliationName));
            });

            return services;
        }

        private static string GetXmlDocPath(string appName)
        {
            if (appName.Contains(','))
            {
                // if app name is the full assembly name, just grab the short name part
                appName = appName.Substring(0, appName.IndexOf(','));
            }

            return Path.Combine(AppContext.BaseDirectory, appName + ".xml");
        }
    }
}