using System;

namespace EAVFW.Extensions.Configuration.RJSF
{
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Class, AllowMultiple = true)]
    public class RJSFUIOptionAttribute : Attribute
    {
        public RJSFUIOptionAttribute(RJSFOption option, object value)
        {
            Option = option;
            Value = value;
        }
        public RJSFOption Option { get; set; }
        public object Value { get; set; }
    }


    
}
