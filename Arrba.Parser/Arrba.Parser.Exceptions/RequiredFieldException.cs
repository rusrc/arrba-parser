using System;

namespace Arrba.Parser.Exceptions
{
    public class RequiredFieldException : ProviderException
    {
        public RequiredFieldException(string providerName)
            : base(providerName, "Can't get field or field returns null")
        {
        }

        public RequiredFieldException(string providerName, string message)
            : base(providerName, message)
        {
        }

        public RequiredFieldException(string providerName, string message, Exception innerException)
            : base(providerName, message, innerException)
        {
        }
    }
}
