using System;

namespace EAVFW.Extensions.Configuration
{
    public interface IServerEntity
    {
        public string Name { get; set; }
        public DateTime Heartbeat { get; set; }

        public Guid Id { get; set; }
    }
}