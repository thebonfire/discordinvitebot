using Pat.Services;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Pat
{
    internal class Program
    {
        private static async Task Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Error);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDiscordHost((config, _) =>
                    {
                        config.SocketConfig = new DiscordSocketConfig
                        {
                            LogLevel = LogSeverity.Error,
                            AlwaysDownloadUsers = true,
                            MessageCacheSize = 500,
                            GatewayIntents = GatewayIntents.All,
                            AlwaysDownloadDefaultStickers = true,
                            LogGatewayIntentWarnings = false,
                        };

                        config.Token = "";
                    });

                    services.AddInteractionService((config, _) =>
                    {
                        config.LogLevel = LogSeverity.Error;
                        config.DefaultRunMode = RunMode.Async;
                    });

                    services
                    .AddDbContext<BotContext>()
                    .AddScoped<DataAccessLayer>()
                    .AddHostedService<CommandHandler>()
                    .AddSingleton<InteractionService>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}