using System;

namespace Arrba.Parser.Exceptions
{
    public class ServerException : ProviderException
    {
        public ServerException(string providerName) : base(providerName)
        {
        }

        public ServerException(string providerName, string message) : base(providerName, message)
        {
        }

        public ServerException(string providerName, Exception innerException) : base(providerName, innerException)
        {
        }

        public ServerException(string providerName, string message, Exception innerException) : base(providerName, message, innerException)
        {
        }
    }
}
