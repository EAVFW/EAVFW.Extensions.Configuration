using EAVFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
}