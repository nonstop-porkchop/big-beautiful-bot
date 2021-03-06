using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BBB;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => { services.AddHostedService<DiscordBotHost>(); })
            .ConfigureAppConfiguration(app =>
            {
                app.AddJsonFile("appsettings.json");
                app.AddUserSecrets<DiscordBotHost>();
            });
    }
}