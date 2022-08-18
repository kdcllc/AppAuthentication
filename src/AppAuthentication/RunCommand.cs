using AppAuthentication.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace AppAuthentication
{
    [Command(
        "run",
        Description = "Runs instance of the local server that returns authentication tokens.",
        UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw,
        AllowArgumentSeparator = true)]
    [HelpOption("--help")]
    internal class RunCommand
    {
        [Option(
            "-a|--authority",
            Description = "Authority Azure TenantId or Azure Directory ID")]
        public string Authority { get; private set; }

        [Option(
            "-r|--resource",
            Description = "Resource to authenticate against. Provided https://login.microsoftonline.com/{tenantId}. Default set to https://vault.azure.net/")]
        public string Resource { get; private set; }

        /// <summary>
        /// Property types of ValueTuple{bool,T} translate to CommandOptionType.SingleOrNoValue.
        /// Input                   | Value
        /// ------------------------|--------------------------------
        /// (none)                  | (false, default(LogLevel))
        /// --verbose               | (true, LogLevel.Information)
        /// --verbose:information   | (true, LogLevel.Information)
        /// --verbose:debug         | (true, LogLevel.Debug)
        /// --verbose:trace         | (true, LogLevel.Trace).
        /// </summary>
        [Option(Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public (bool hasValue, LogLevel level) Verbose { get; } = (false, LogLevel.Error);

        [Option(
            "-e|--environment",
            Description = "Specify Hosting Environment Name for the cli tool execution. The Default is Development")]
        public string HostingEnvironment { get; private set; }

        [Option(
            "-p|--port",
            Description = "Specify Web Host port number otherwise it is automatically generated. The Default port if open is 5050.")]
        public int? Port { get; private set; }

        [Option(
            "-c|--config",
            Description = "Allows to specify a configuration file besides appsettings.json to be specified. The Default appsetting.json located in the execution path.")]
        public string ConfigFile { get; private set; }

        [Option(
            "-t|--token-provider",
            Description = "The Azure CLI Access Token Provider to retrieve the Authentication Token. The Default provider is AzureCli.")]
        public TokenProvider TokenProvider { get; } = TokenProvider.AzureCli;

        [Option("-f|--fix", Description = "Fix command resets Environment Variables.")]
        public bool Fix { get; private set; }

        [Option(
            "-l|--local",
            Description = "Setup MSI_ENDPOINT to be pointing to localhost. The Default is set to support Docker Containers only.")]
        public bool Local { get; set; }

        public string[] RemainingArguments { get; }

        private async Task<int> OnExecuteAsync(IConsole console)
        {
            if (Fix)
            {
                EnvironmentHostedService.ResetVariables();
            }

            var builderConfig = new WebHostBuilderOptions
            {
                Authority = Authority,
                HostingEnvironment = !string.IsNullOrWhiteSpace(HostingEnvironment) ? HostingEnvironment : "Development",
                Resource = !string.IsNullOrWhiteSpace(Resource) ? Resource : "https://vault.azure.net/",
                Verbose = Verbose.hasValue,
                Level = Verbose.level,
                ConfigFile = ConfigFile,
                SecretId = Guid.NewGuid().ToString(),
                IsLocal = Local
            };

            try
            {
                builderConfig.Port = Port ?? ConsoleHandler.GetRandomUnusedPort();

                console.WriteLine(
                    ConsoleColor.Blue,
                    "[{0}][Listening]:[http://{1}:{2}]",
                    nameof(AppAuthentication).ToUpperInvariant(),
                    builderConfig.IsLocal ? Constants.MsiLocalhostUrl : Constants.MsiContainerUrl,
                    builderConfig.Port);

                var host = WebServer.CreateDefaultBuilder(builderConfig)
                                    .ConfigureWeb(TokenProvider)
                                    .Build();

                console.WriteLine(ConsoleColor.Yellow, "MSI Endpoint Started");

                await host.RunAsync();

                console.WriteLine(ConsoleColor.Blue, "MSI Endpoint Stopped");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.Red);
                return -1;
            }
        }
    }

    internal enum TokenProvider
    {
        VisualStudio,
        AzureCli
    }
}
