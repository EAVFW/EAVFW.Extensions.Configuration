using EAVFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;

namespace EAVFW.Extensions.Configuration
{
    public static class ServerWatchDogExtensions
    {

        public static IServiceCollection AddServerWatchDog<TContext,TServer>(this IServiceCollection services, ClaimsPrincipal principal = null)
              where TContext : DynamicContext
                where TServer : DynamicEntity, IServerEntity, new()
        {
            services.TryAddSingleton<IApplicationInstance, ApplicationInstance>();
            services.TryAddSingleton<IServerInstance, ServerInstance>();
            services.AddHostedService<ServerWatchDog<TServer,TContext>>();
            if(principal!=null)
                services.AddOptions<ServerWatchDogOptions>().Configure(k => { k.Identity ??= principal; });
            return services;
        }

    }
}