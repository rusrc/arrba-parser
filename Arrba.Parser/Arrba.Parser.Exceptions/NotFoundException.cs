using System;

namespace Arrba.Parser.Exceptions
{
    public class NotFoundException : ProviderException
    {
        public NotFoundException(string providerName)
            : base(providerName)
        {
        }

        public NotFoundException(string providerName, string message)
            : base(providerName, message)
        {
        }

        public NotFoundException(string providerName, string message, Exception innerException)
        : base(providerName, message, innerException)
        {
        }
    }
}
