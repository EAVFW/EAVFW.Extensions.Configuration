using Microsoft.Extensions.Configuration;

namespace EAVFW.Extensions.Configuration
{
    public class ApplicationInstance : IApplicationInstance
    {
        public ApplicationInstance(IConfiguration configuration)
        {
            ApplicationName = configuration.GetValue<string>("ApplicationName");
        }
        public string ApplicationName { get; }
    }
}