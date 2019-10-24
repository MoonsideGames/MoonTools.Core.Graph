using System;
using System.Runtime.Serialization;

namespace MoonTools.Core.Graph
{
    [Serializable]
    public class NegativeWeightException : Exception
    {
        public NegativeWeightException()
        {
        }

        public NegativeWeightException(string message) : base(message)
        {
        }

        public NegativeWeightException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NegativeWeightException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}