using System;

namespace Upstream.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
