using System;
using System.ComponentModel;

namespace Arrba.Parser.Provider.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProviderDescriptionAttribute : DescriptionAttribute
    {
        public string Host { get; set; }
    }
}
