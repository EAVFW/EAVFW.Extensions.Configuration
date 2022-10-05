using EAVFramework.Shared;
using System;

namespace EAVFW.Extensions.Configuration
{
    [EntityInterface(EntityKey = "Environment Variable")]
    public interface IEnvironmentVariable
    {
        public Guid? ServerId { get; set; }
        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}