using System;

namespace Arrba.Parser.Exceptions
{
    public class DealershipNotFoundException : NotFoundException
    {
        public DealershipNotFoundException(string providerName) : base(providerName)
        {
        }

        public DealershipNotFoundException(string providerName, string message) : base(providerName, message)
        {
        }

        public DealershipNotFoundException(string providerName, string message, Exception innerException) : base(providerName, message, innerException)
        {
        }
    }
}
