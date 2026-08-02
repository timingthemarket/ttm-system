using Microsoft.Extensions.DependencyInjection;
using TTM.Shared.Services;

namespace TTM.Shared.Extensions;

public static class DiscordExtension
{
    public static IServiceCollection AddTtmDiscordService(this IServiceCollection services)
    {
        services.AddHttpClient<IDiscordService, DiscordService>(client =>
        {
            client.BaseAddress = new Uri("https://discord.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
