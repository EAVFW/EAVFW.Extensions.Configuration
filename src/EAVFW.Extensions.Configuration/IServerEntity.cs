using EAVFramework.Shared;
using System;

namespace EAVFW.Extensions.Configuration
{

    [EntityInterface(EntityKey = "Server")]
    public interface IServerEntity
    {
        public string Name { get; set; }
        public DateTime? Heartbeat { get; set; }

        public Guid Id { get; set; }
    }
}