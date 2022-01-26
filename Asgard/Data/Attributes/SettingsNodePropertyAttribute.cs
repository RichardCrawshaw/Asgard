using System;

namespace Asgard.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SettingsNodePropertyAttribute : Attribute
    {
        public string? Name { get; set; }

        public string? Path { get; set; }
    }
}
