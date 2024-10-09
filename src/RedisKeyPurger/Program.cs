using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisKeyPurger;

var purgerService = ServiceConfiguration.ServiceProvider.GetRequiredService<Service>();

var configuration = ServiceConfiguration.ServiceProvider.GetRequiredService<IConfiguration>();

var logger = ServiceConfiguration.ServiceProvider.GetRequiredService<ILogger<Program>>();

var keyPattern = configuration.GetValue<string>("KeyPattern");

logger.LogInformation("Application started!");

await purgerService.DeleteAsync(keyPattern);