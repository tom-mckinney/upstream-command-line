using System;
using System.Collections.Generic;
using System.Text;

namespace Upstream.CommandLine
{
    public class CommandLineException : Exception
    {
        public CommandLineException(string message) : base(message)
        {
        }

        public CommandLineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
