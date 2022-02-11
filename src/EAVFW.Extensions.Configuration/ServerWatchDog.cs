using DotNetDevOps.Extensions.EAVFramework;
using DotNetDevOps.Extensions.EAVFramework.Endpoints;
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

    public class EAVFWConfigurationSource<TContext, TEnvironmentVariable, TServer> : IConfigurationSource
        where TContext : DynamicContext
        where TEnvironmentVariable : DynamicEntity, IEnvironmentVariable
        where TServer : DynamicEntity, IServerEntity
    {
        public Action<IServiceCollection> ConfigureServices { get; set; }
        public IHostEnvironment Environment { get; set; }

        public EAVFWConfigurationSource()
        {
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new EAVFWConfigurationProvider<TContext,TEnvironmentVariable,TServer>(this);
    }

    public static class ConfigurationBuilderExtensions
    {
        public static IHostBuilder WithEAVFWConfiguration<TContext,TEnvironmentVariable,TServer>(this IHostBuilder builder, string applicationName, Action<IServiceCollection> contextRegistration)
             where TContext : DynamicContext
            where TEnvironmentVariable : DynamicEntity, IEnvironmentVariable
            where TServer : DynamicEntity, IServerEntity
        {
            return builder.ConfigureAppConfiguration((ctx, builder) =>
            {
                var config = builder.Build();

                builder.AddEAVFWConfiguration<TContext,TEnvironmentVariable,TServer>(ctx.HostingEnvironment, (services) =>
                {
                    services.AddSingleton<IConfiguration>(config);

                    services.AddSingleton<IApplicationInstance, ApplicationInstance>();

                    contextRegistration(services);
                    
                });
            });
        }

        public static IConfigurationBuilder AddEAVFWConfiguration<TContext, TEnvironmentVariable, TServer>
        (
            this IConfigurationBuilder builder, IHostEnvironment environment, Action<IServiceCollection> services
        )
            where TContext : DynamicContext
            where TEnvironmentVariable : DynamicEntity, IEnvironmentVariable
            where TServer : DynamicEntity, IServerEntity
        {
            //var tempConfig = builder.Build();
            //var connectionString =
            //    tempConfig.GetConnectionString("ApplicationDB");

            return builder.Add(new EAVFWConfigurationSource<TContext,TEnvironmentVariable,TServer>
            { ConfigureServices = services, Environment = environment });
        }
    }

    public interface IEnvironmentVariable
    {
        public Guid? ServerId { get; set; }
        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class EAVFWConfigurationProviderLoader<TContext,TEnvironmentVariable,TServer>
        where TContext : DynamicContext
        where TEnvironmentVariable : DynamicEntity, IEnvironmentVariable
        where TServer : DynamicEntity, IServerEntity


    {
        private readonly TContext _db;
        private readonly IServerInstance _server;
        private readonly IApplicationInstance _application;

        public EAVFWConfigurationProviderLoader(TContext db, IServerInstance server, IApplicationInstance application)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        internal void Load(IDictionary<string, string> data)
        {

            var set = from variable in _db.Set<TEnvironmentVariable>()
                       join server in _db.Set<TServer>() on variable.ServerId equals server.Id into test
                       from server in test.DefaultIfEmpty()
                       where (server == null || server.Name == _server.ServerName) && (variable.ApplicationName == null || variable.ApplicationName == _application.ApplicationName)
                       select variable;


            //var set = _db.Set<TEnvironmentVariable>()
            //        .Include(x => x.Server).Where(ev =>
            //        (ev.Server == null || ev.Server.Name == _server.ServerName) &&
            //        (ev.ApplicationName == null || ev.ApplicationName == _application.ApplicationName))
            //    .ToList();

            foreach (var item in set)
            {
                data.Add(item.Name, item.Value);
            }
        }
    }

    public class EAVFWConfigurationProvider<TContext, TEnvironmentVariable, TServer> : ConfigurationProvider 
        where TContext : DynamicContext
        where TEnvironmentVariable : DynamicEntity, IEnvironmentVariable
        where TServer : DynamicEntity, IServerEntity


    {
        private readonly EAVFWConfigurationSource<TContext, TEnvironmentVariable, TServer> _source;
        private IServiceProvider _serviceProvider = null;

        public EAVFWConfigurationProvider(EAVFWConfigurationSource<TContext, TEnvironmentVariable, TServer> source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public override void Load()
        {
            if (_serviceProvider == null)
            {
                var collection = new ServiceCollection();
                collection.AddLogging();
                collection.AddSingleton(_source.Environment);
                collection.AddScoped<EAVFWConfigurationProviderLoader<TContext,TEnvironmentVariable,TServer>>();
                collection.AddSingleton<IServerInstance, ServerInstance>();
                _source.ConfigureServices(collection);
                _serviceProvider = collection.BuildServiceProvider();
            }


            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var loader = scope.ServiceProvider.GetRequiredService<EAVFWConfigurationProviderLoader<TContext,TEnvironmentVariable,TServer>>();

                loader.Load(Data);
            }

          
        }

      
    }

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