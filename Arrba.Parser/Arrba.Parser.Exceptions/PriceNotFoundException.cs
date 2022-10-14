using System;

namespace Arrba.Parser.Exceptions
{
    public class PriceNotFoundException : NotFoundException
    {
        public PriceNotFoundException(string providerName) : base(providerName)
        {
        }
        public PriceNotFoundException(string providerName, string message) : base(providerName, message)
        {
        }

        public PriceNotFoundException(string providerName, string message, Exception innerException) : base(providerName, message, innerException)
        {
        }
    }
}
