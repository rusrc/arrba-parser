using System;
using System.Collections.Generic;
using System.Text;

namespace Arrba.Parser.Exceptions
{
    public class ItemSoldException : ProviderException
    {
        public ItemSoldException(string providerName) : base(providerName)
        {
        }

        public ItemSoldException(string providerName, string message) : base(providerName, message)
        {
        }

        public ItemSoldException(string providerName, Exception innerException) : base(providerName, innerException)
        {
        }

        public ItemSoldException(string providerName, string message, Exception innerException) : base(providerName, message, innerException)
        {
        }
    }
}
