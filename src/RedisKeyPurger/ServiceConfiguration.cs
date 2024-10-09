using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisKeyPurger
{
    public class ServiceConfiguration
    {
        private static readonly Lazy<IServiceProvider> LazyServiceProvider =
            new Lazy<IServiceProvider>(BuildServiceProvider);

        public static IServiceProvider ServiceProvider => LazyServiceProvider.Value;

        private static IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();

            var services = new ServiceCollection();

            services.AddSingleton(configuration);

            services.AddLogging(cfg =>
            {
                cfg.AddSimpleConsole(opts =>
                {
                    opts.TimestampFormat = "[dd-MM-yyyyTHH:mm:ss.fffffffK]-";
                    opts.ColorBehavior = LoggerColorBehavior.Enabled;
                });
            });

            services.AddSingleton<Task<ConnectionMultiplexer>>(_ =>
            {
                var connection = configuration.GetSection("ConnectionStrings").GetValue<string>("Redis");
                var configurationOpts = ConfigurationOptions.Parse(connection);

                return ConnectionMultiplexer.ConnectAsync(configurationOpts);
            });

            services.AddSingleton(_ =>
            {
                var keyPurgeOptions = configuration.GetSection(nameof(KeyPurgeOptions)).Get<KeyPurgeOptions>();

                return keyPurgeOptions;
            });

            services.AddSingleton<Service>();

            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", false);

            return configBuilder.Build();
        }
    }
}