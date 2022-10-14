using System;

namespace Arrba.Parser.Exceptions
{
    public class ProcessorException : Exception
    {
        public ProcessorException()
            : base("Processor exception")
        {
        }

        public ProcessorException(string message)
            : base(message)
        {
        }

        public ProcessorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
