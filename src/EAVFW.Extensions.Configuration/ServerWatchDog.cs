using EAVFramework;
using EAVFramework.Endpoints;
using EAVFramework.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EAVFW.Extensions.Configuration
{

    public class ServerWatchDog<TServer, TContext> : BackgroundService
       where TContext : DynamicContext
        where TServer : DynamicEntity, IServerEntity, new()
    {
        private readonly ILogger  _logger;
        private readonly IOptions<ServerWatchDogOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerInstance _serverInstance;

        public ServerWatchDog(ILogger<ServerWatchDog<TServer,TContext>> logger,
            IOptions<ServerWatchDogOptions> options,
            IServiceProvider serviceProvider, 
            IServerInstance serverInstance)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options=options??throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serverInstance = serverInstance??throw new ArgumentNullException(nameof(serverInstance));
        }


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {



            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var db =
                        scope.ServiceProvider
                            .GetRequiredService<EAVDBContext<TContext>>();
                    var dbcontext = scope.ServiceProvider.GetRequiredService<TContext>();

                    var servers = db.Set<TServer>();
                    var existing = await db.Set<TServer>().FirstOrDefaultAsync(c => c.Name == _serverInstance.ServerName);
                    if (existing != null)
                    {
                        existing.Heartbeat = DateTime.UtcNow;
                        servers.Update(existing);
                    }
                    else
                    {
                        servers.Add(new TServer
                        {
                            Heartbeat = DateTime.UtcNow,
                            Name = _serverInstance.ServerName,
                        });
                    }

                    await db.SaveChangesAsync(_options.Value.Identity);


                    var old = await db.Set<TServer>().Where(s => s.Heartbeat < DateTime.UtcNow.AddMinutes(-5)).ToListAsync();

                    foreach (var server in old)
                    {
                        dbcontext.Remove(server);
                    }

                    await db.SaveChangesAsync(_options.Value.Identity);

                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }


        }
    }


    public static class ServerWatchDogExtensions
    {

        public static IServiceCollection AddServerWatchDog<TContext,TServer>(this IServiceCollection services)
              where TContext : DynamicContext
                where TServer : DynamicEntity, IServerEntity, new()
        {
            services.TryAddSingleton<IApplicationInstance, ApplicationInstance>();
            services.TryAddSingleton<IServerInstance, ServerInstance>();
            services.AddHostedService<ServerWatchDog<TServer,TContext>>();
            return services;
        }

    }
}