using System.Security.Claims;

namespace EAVFW.Extensions.Configuration
{
    public class ServerWatchDogOptions
    {
        public ClaimsPrincipal Identity { get;  set; }
    }
}