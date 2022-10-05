using EAVFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EAVFW.Extensions.Configuration
{
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
}