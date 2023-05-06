using MessageQueues.Task1.MainProcessingService.Interfaces;
using MessageQueues.Task1.MainProcessingService.Models;
using MessageQueues.Task1.MainProcessingService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageQueues.Task1.MainProcessingService
{
    public static class MainProcessingApp
    {
        private const string RabbitMQSection = "RabbitMQConfig";
        private const string FileProcessingSection = "FileProcessingConfig";
        private static IConfiguration _configuration { get; set; }

        public static void StartAsync()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var fileService = serviceProvider.GetRequiredService<IProcessingService>();
            fileService.StartProcessing();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMQConfig>(_configuration.GetSection(RabbitMQSection));
            services.Configure<FileProcessingConfig>(_configuration.GetSection(FileProcessingSection));

            services.AddSingleton<IMessageConsumer, RabbitMQFileConsumer>();
            services.AddScoped<IProcessingService, FileProcessingService>();
        }
    }
}
