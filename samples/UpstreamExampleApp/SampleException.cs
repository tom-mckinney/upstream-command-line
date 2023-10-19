using System;

namespace UpstreamExampleApp;

public class SampleException : Exception
{
    public SampleException(string message) : base(message)
    {
    }
}