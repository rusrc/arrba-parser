using System;

namespace Arrba.Parser.Exceptions
{
    public class ProviderException : Exception
    {
        public ProviderException(string providerName)
            : base(providerName)
        {
        }

        public ProviderException(string providerName, string message)
            : base(providerName + "\n" + message)
        {
        }

        public ProviderException(string providerName, Exception innerException)
            : base(providerName, innerException)
        {
        }

        public ProviderException(string providerName, string message, Exception innerException)
            : base(providerName + "\n" + message, innerException)
        {
        }
    }
}
