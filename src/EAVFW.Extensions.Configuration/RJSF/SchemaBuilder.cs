using Newtonsoft.Json;
using NJsonSchema;
using System.Runtime.Serialization;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EAVFW.Extensions.Configuration.RJSF
{
    public interface ISchemaBuilder
    {
        UISchema Uischema { get; }
        Type Type { get; }

        RJSFProps Build();
    }
   
    public class SchemaBuilder : ISchemaBuilder
    {
        public static SchemaBuilder<T> FromType<T>()
        {
            return new SchemaBuilder<T>();
        }
        public static ISchemaBuilder FromType(Type type)
        {
            var builder = typeof(SchemaBuilder<>).MakeGenericType(type);
            return Activator.CreateInstance(builder) as ISchemaBuilder;
        }

        public UISchema Uischema { get; } = new UISchema();
        
        public Type Type { get; private set; }

        public SchemaBuilder(Type type)
        {
            Type = type;
        }
        public String GetEnumMemberValue(RJSFOption value)

        {
            return value.GetType()
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }




      
        public ISchemaBuilder AddUIOption<TOptionValue>(string propertyName, RJSFOption option, TOptionValue value)
        {
            var uischema = Uischema;

            if (!uischema.ContainsKey(propertyName))
            {
                uischema[propertyName] = new UISchema();
            }
            uischema = uischema[propertyName];

            uischema.Add($"ui:{GetEnumMemberValue(option)}", new UISchema(value));

            return this;
        }
        public ISchemaBuilder AddUIOption<TOptionValue>(RJSFOption option, TOptionValue value)
        { 
            Uischema.Add($"ui:{GetEnumMemberValue(option)}", new UISchema(value));

            return this;
        }


        public RJSFProps Build()
        {
            foreach(var attr in Type.GetCustomAttributes<RJSFUIOptionAttribute>())
            {
                AddUIOption(attr.Option, attr.Value);
            }   

            foreach (var prop in Type.GetProperties())
            {
                var propName = prop.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;

                var options = prop.GetCustomAttributes<RJSFUIOptionAttribute>();
                foreach (var option in options)
                {

                    AddUIOption(propName, option.Option, option.Value);

                }
 
            }
            var schema = JsonSchema.FromType(Type);



            return new RJSFProps
            {
                Schema = schema,
                UISchema = Uischema,
            };
        }

     
    }
    public class SchemaBuilder<T> : SchemaBuilder, ISchemaBuilder
    {
        public SchemaBuilder() : base(typeof(T))
        {

        }
        public ISchemaBuilder AddUIOption<TProp, TOptionValue>(Expression<Func<T, TProp>> picker, RJSFOption option, TOptionValue value)
        {
            var uischema = Uischema;

            var m = picker.Body as MemberExpression;
            if (m != null)
            {
                var scope = m.Member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;

                if (!uischema.ContainsKey(scope))
                {
                    uischema[scope] = new UISchema();
                }
                uischema = uischema[scope];
            }

            uischema.Add($"ui:{GetEnumMemberValue(option)}", new UISchema(value));

            return this;
        }


    }

}
