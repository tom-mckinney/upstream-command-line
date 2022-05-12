using System;

namespace SampleConsoleApp;

public class SampleException : Exception
{
    public SampleException(string message) : base(message)
    {
    }
}