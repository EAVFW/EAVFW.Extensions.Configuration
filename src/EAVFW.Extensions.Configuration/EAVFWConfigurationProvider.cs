using DotNetDevOps.Extensions.EAVFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EAVFW.Extensions.Configuration
{
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
}