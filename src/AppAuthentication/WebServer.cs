using AppAuthentication.AzureCli;
using AppAuthentication.VisualStudio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AppAuthentication
{
    internal static class WebServer
    {
        internal static WebHostBuilder CreateDefaultBuilder(WebHostBuilderOptions options)
        {
            var builder = new WebHostBuilder();

            builder.UseEnvironment(options.HostingEnvironment);
            var fullPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(options.ConfigFile)))
            {
                fullPath = Path.GetDirectoryName(options.ConfigFile);
            }

            options.ContentRoot = fullPath;

            builder.UseContentRoot(fullPath);

            var defaultConfigName = !string.IsNullOrWhiteSpace(options.ConfigFile) ? Path.GetFileName(options.ConfigFile) : "appsettings.json";

            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddEnvironmentVariables(prefix: "ASPNETCORE_");
            configBuilder.AddEnvironmentVariables(prefix: "DOTNETCORE_");

            // appsettings file or others
            configBuilder.AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.json"), optional: true)
                  .AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.{options.HostingEnvironment}.json"), optional: true);

            if (options.Arguments != null)
            {
                configBuilder.AddCommandLine(options.Arguments);
            }

            var config = configBuilder.Build();

            builder.UseConfiguration(config);

            builder
                .UseKestrel()
                .UseUrls(string.Format(Constants.HostUrl, Constants.MsiLocalhostUrl, options.Port))
                .SuppressStatusMessages(true)
                .ConfigureLogging(logger =>
                {
                    logger.ClearProviders();
                    logger.AddConsole();

                    if (options.Verbose)
                    {
                        logger.AddDebug();
                        logger.AddFilter(x => true);
                    }
                });

            builder
                .ConfigureServices(services =>
                {
                    services.AddHostedService<EnvironmentHostedService>();
                    services.AddSingleton(options);
                });

            return builder;
        }

        internal static WebHostBuilder ConfigureWeb(this WebHostBuilder builder, TokenProvider tokenProvider)
        {
            // header: Secret = MSI_SECRET
            // ?resource=clientid=&api-version=2017-09-01
            builder.Configure(app =>
               {
                   app.MapWhen(
                      context => context.Request.Path.Value.Contains("/oauth2"),
                      (IApplicationBuilder pp) =>
                      {
                          pp.Map("/oauth2/token", (IApplicationBuilder ppa) =>
                          {
                              ppa.Run(async (context) =>
                              {
                                  var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                                  var options = app.ApplicationServices.GetRequiredService<WebHostBuilderOptions>();
                                  var logger = loggerFactory.CreateLogger("host");

                                  try
                                  {
                                      var provider = app.ApplicationServices.GetRequiredService<IAccessTokenProvider>();

                                      var requestResource = context.Request.Query["resource"].ToString();

                                      logger.LogDebug("[Request][QueryString][resource]:['{query}']", requestResource);

                                      var resource = !string.IsNullOrWhiteSpace(requestResource) ? requestResource : options.Resource;

                                      var token = await provider.GetAuthResultAsync(resource, options.Authority);

                                      var jsonResponse = JsonConvert.SerializeObject(token);

                                      context.Response.Headers.Add("content-type", "application/json");

                                      logger.LogDebug("Sending response");

                                      await context.Response.WriteAsync(jsonResponse);
                                  }
                                  catch (Exception ex)
                                  {
                                      logger.LogError(ex, "Error occurred processing the request");

                                      context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                                      await context.Response.WriteAsync(ex.ToString());
                                  }
                              });
                          });
                      });

                   app.Run(context =>
                   {
                       context.Response.Headers.Add("content-type", "text/html");
                       return context.Response.WriteAsync(@"<a href=""/hello"">hello</a> <a href=""/world"">world</a>");
                   });
               })
              .ConfigureServices((hostingContext, services) =>
              {

                  services.AddTransient<IProcessManager, ProcessManager>();
                  services.AddTransient<VisualStudioTokenProviderFile>();

                  switch (tokenProvider)
                  {
                      case TokenProvider.VisualStudio:
                          services.AddSingleton<IAccessTokenProvider, VisualStudioAccessTokenProvider>();

                          break;
                      case TokenProvider.AzureCli:
                          services.AddSingleton<IAccessTokenProvider, AzureCliAccessTokenProvider>();
                          break;
                  }
              });

            return builder;
        }
    }
}
