using DotNetDevOps.Extensions.EAVFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EAVFW.Extensions.Configuration
{
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
}