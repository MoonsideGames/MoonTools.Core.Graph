using System;
using System.Runtime.Serialization;

namespace MoonTools.Core.Graph
{
    [Serializable]
    public class NegativeCycleException : Exception
    {
        public NegativeCycleException()
        {
        }

        public NegativeCycleException(string message) : base(message)
        {
        }

        public NegativeCycleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NegativeCycleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}