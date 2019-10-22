using System;

namespace MoonTools.Core.Graph
{
    public class InvalidVertexException : Exception
    {
        public InvalidVertexException(
            string format,
            params object[] args
        ) : base(string.Format(format, args)) { }
    }
}
