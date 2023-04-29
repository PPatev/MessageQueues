using MessageQueues.Task1.DataCapturingServicePdf.Intefaces;
using MessageQueues.Task1.DataCapturingServicePdf.Models;
using MessageQueues.Task1.DataCapturingServicePdf.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageQueues.Task1.DataCapturingServicePdf
{
    public static class DataCaptureApp
    {
        //

        //public static void CreateHostBuilder(string[] args)
        //{
        //    IConfiguration configuration = new ConfigurationBuilder()
        //        .AddJsonFile("appsettings.json")
        //        .Build();

        //    var host = Host.CreateDefaultBuilder(args).ConfigureServices(services => 
        //    {
        //        services.Configure<RabbitMQConfig>(configuration.GetSection(RabbitMQSection));

        //        services.AddScoped<IRabbitMQService, RabbitMQService>();

        //    }).Build();
        //}

        private const string RabbitMQSection = "RabbitMQConfig";
        private const string FileTransferSection = "FileTransferConfig";
        private static IConfiguration _configuration { get; set; }

        public static void Start()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            CreateExchange(serviceProvider);
            var fileService = serviceProvider.GetRequiredService<ITransferingService>();
            fileService.StartTransfering();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMQConfig>(_configuration.GetSection(RabbitMQSection));
            services.Configure<FileTransferConfig>(_configuration.GetSection(FileTransferSection));
            services.AddScoped<IMessageProducer, RabbitMQProducer>();
            services.AddScoped<ITransferingService, FileTransferingService>();
        }

        private static void CreateExchange(IServiceProvider provider)
        {
            var rabbitMQConfig = provider.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(rabbitMQConfig.ConnectionString);

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(rabbitMQConfig.ExchangeName, ExchangeType.Headers, true, true);

            channel.Close();
            connection.Close();
        }

    }
}
