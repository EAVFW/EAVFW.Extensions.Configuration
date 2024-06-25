using Newtonsoft.Json;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EAVFW.Extensions.Configuration.RJSF
{
    [Newtonsoft.Json.JsonConverter(typeof(UISchemaConverter))]

    public class UISchema : Dictionary<string, UISchema>
    {
        public UISchema(object value)
        {
            Value = value;
        }
        public UISchema()
        {

        }
        public string Label => this["ui:label"].Value as string;
        public object Value { get; set; }
    }

}
