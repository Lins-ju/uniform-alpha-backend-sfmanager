using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Cosmos;
using SFManager.Source;
using SFManager.Source.Domain;
using SFManager.Source.Persistence;
using Utils.Source;
using Utils.Source.Domain;
using Utils.Source.Extensions;

namespace SFManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables()
               .Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var isRunningLocal = Environment.GetEnvironmentVariable("IS_RUNNING_LOCAL");

            if (isRunningLocal == "true" || isRunningLocal == null)
            {
                Console.WriteLine("isRunningLocal is set to TRUE");
                services.Configure<TokenOptions>(Configuration.GetSection(TokenOptions.Section));
                services.Configure<CosmosOptions>(Configuration.GetSection(CosmosOptions.Section));
                services.Configure<S3Options>(Configuration.GetSection(S3Options.Section));

                var cosmosConfig = Configuration.GetSection(CosmosOptions.Section);
                var cosmosClientOptions = new CosmosClientOptions
                {
                    LimitToEndpoint = true,
                    ConnectionMode = ConnectionMode.Gateway
                };
                services.AddSingleton<CosmosClient>(cosmosClient =>
                {
                    var connectionString = cosmosConfig.GetValue<string>("ConnectionString");
                    return new CosmosClient(connectionString, cosmosClientOptions);
                });
                services.AddSingleton<AmazonS3Client>(s3Client =>
                {
                    var creds = new BasicAWSCredentials("fakeMyAccessKeyId", "fakeMySecretAccessKey");
                    var clientConfigS3 = new AmazonS3Config
                    {
                        ServiceURL = "http://localhost:4566",
                        AuthenticationRegion = "us-east-1",
                        ForcePathStyle = true
                    };
                    return new AmazonS3Client(creds, clientConfigS3);
                });
            }
            else
            {
                Console.WriteLine("isRunningLocal is set to FALSE");
                var tokenOptions = new TokenOptions
                {
                    SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY")
                };
                var cosmosOptions = new CosmosOptions
                {
                    ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING"),
                    DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME"),
                    ContainerName = Environment.GetEnvironmentVariable("CONTAINER_NAME")
                };

                var s3Options = new S3Options
                {
                    BucketName = Environment.GetEnvironmentVariable("BUCKET_NAME"),
                    ServiceUrl = Environment.GetEnvironmentVariable("SERVICE_URL"),
                    AWS_REGION = Environment.GetEnvironmentVariable("AWS_REGION"),
                    AWS_ACESS_KEY_ID = Environment.GetEnvironmentVariable("AWS_ACESS_KEY_ID"),
                    AWS_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                };

                services.Configure<TokenOptions>(options =>
                {
                    options.SecretKey = tokenOptions.SecretKey;
                });
                services.Configure<CosmosOptions>(options =>
                {
                    options.ConnectionString = cosmosOptions.ConnectionString;
                    options.DatabaseName = cosmosOptions.DatabaseName;
                    options.ContainerName = cosmosOptions.ContainerName;
                });
                services.Configure<S3Options>(options =>
                {
                    options.BucketName = s3Options.BucketName;
                    options.ServiceUrl = s3Options.ServiceUrl;
                    options.AWS_REGION = s3Options.AWS_REGION;
                    options.AWS_ACESS_KEY_ID = s3Options.AWS_ACESS_KEY_ID;
                    options.AWS_SECRET_ACCESS_KEY = s3Options.AWS_SECRET_ACCESS_KEY;
                });
                services.AddSingleton<CosmosClient>(cosmosClient =>
                {
                    return new CosmosClient(cosmosOptions.ConnectionString, new CosmosClientOptions
                    {
                        LimitToEndpoint = true,
                        ConnectionMode = ConnectionMode.Gateway
                    });
                });

                services.AddSingleton<AmazonS3Client>(s3Client =>
                {
                    var creds = new BasicAWSCredentials(s3Options.AWS_ACESS_KEY_ID, s3Options.AWS_SECRET_ACCESS_KEY);
                    var clientConfigS3 = new AmazonS3Config
                    {
                        ServiceURL = s3Options.ServiceUrl,
                        AuthenticationRegion = s3Options.AWS_REGION,
                        ForcePathStyle = true
                    };
                    return new AmazonS3Client(creds, clientConfigS3);
                });
            }

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Listen(IPAddress.Any, 8000);
            });
            services.AddControllers();

            services.AddSingleton<IJWTManager, JWTManager>();
            services.AddSingleton<S3Datastore>();
            services.AddSingleton<CosmosDatastore>();
            services.AddSingleton<FileManagerController>();
            services.AddSingleton<FileManagerService>();
        }

        public void Configure(WebApplication app, IWebHostEnvironment environment)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseCustomAuthenticationMiddleware();

            app.MapControllers();
        }
    }
}
