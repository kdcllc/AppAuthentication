using Azure;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Text;
using Console = Colorful.Console;

public class Main : IMain
{
    private readonly ILogger<Main> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public Main(
        IHostApplicationLifetime applicationLifetime,
        IConfiguration configuration,
        ILogger<Main> logger)
    {
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IConfiguration Configuration { get; set; }

    public async Task<int> RunAsync()
    {
        _logger.LogInformation("Main executed");

        // use this token for stopping the services
        _applicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

        // enable logging for Azure.Idenity +
        using AzureEventSourceListener listener = new(
            (args, message) =>
            Console.WriteLine("[{0:HH:mm:ss:fff}][{1}] {2}", DateTimeOffset.Now, args.Level, message),
            level: EventLevel.Verbose);

        Console.WriteLine("Getting Azure Storage Token", Color.Blue);

        // get tocken for azure blob storage
        var token = await new DefaultAzureCredential()
                // can add list of scopes to new[]
                .GetTokenAsync(new TokenRequestContext(new[] { "https://storage.azure.com/.default" }));

        Console.WriteLine(token.Token, color: Color.Azure);

        // custom secret azure key vault
        var testKey = Configuration.GetValue<string>("betk8sweb:testValue");

        if (string.IsNullOrEmpty(testKey))
        {
            throw new ArgumentException("Keyvault failed to retrieve the value");
        }

        Console.WriteLine("Azure Key Vault Secret: {0}", testKey, Color.Yellow);

        await CreateBlockBlobAsync(
            Configuration.GetValue<string>("Storage:StorageName"),
            Configuration.GetValue<string>("Storage:ContainerName"),
            $"{Guid.NewGuid()}.txt");

        Console.WriteLine("File successully was uploaded to Azure Blob Storage", Color.Blue);

        // uncomment this code to get all of the configuration that are loaded into the application.
        // var config = Configuration as IConfigurationRoot;
        // config?.DebugConfigurations();

        return 0;
    }

    private static async Task CreateBlockBlobAsync(string accountName, string containerName, string blobName)
    {
        // Construct the blob container endpoint from the arguments.
        string containerEndpoint = string.Format(
            "https://{0}.blob.core.windows.net/{1}",
            accountName,
            containerName);

        // Get a credential and create a service client object for the blob container.
        var containerClient = new BlobContainerClient(new Uri(containerEndpoint), new DefaultAzureCredential());

        try
        {
            // Create the container if it does not exist.
            await containerClient.CreateIfNotExistsAsync();

            // Upload text to a new block blob.
            string blobContents = $"This is a block blob. {DateTime.Now}";
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

            using MemoryStream stream = new MemoryStream(byteArray);
            await containerClient.UploadBlobAsync(blobName, stream);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
            throw;
        }
    }
}
