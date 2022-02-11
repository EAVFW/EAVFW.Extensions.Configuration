using Microsoft.Extensions.Configuration;
using System;

namespace EAVFW.Extensions.Configuration
{
    public class ServerInstance : IServerInstance
    {
        public ServerInstance(IConfiguration configuration)
        {
            ServerName = configuration.GetValue<string>("COMPUTERNAME") ?? Guid.NewGuid().ToString();
        }
        public string ServerName { get; private set; }



    }
}