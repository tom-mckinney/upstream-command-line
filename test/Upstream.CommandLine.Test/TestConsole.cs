using System.CommandLine;
using System.CommandLine.IO;
using System.Text;

namespace Upstream.CommandLine.Test;

public class TestConsole : IConsole
{
    private readonly StringBuilder _output;
    private readonly TestStreamWriter _streamWriter;

    public TestConsole()
    {
        _output = new StringBuilder();
        _streamWriter = new TestStreamWriter(_output);
    }

    private class TestStreamWriter : IStandardStreamWriter
    {
        private readonly StringBuilder _output;

        public TestStreamWriter(StringBuilder output)
        {
            _output = output;
        }

        public void Write(string value)
        {
            _output.Append(value);
        }
    }

    public string GetOutput() => _output.ToString();

    public IStandardStreamWriter Out => _streamWriter;
    public bool IsOutputRedirected => true;
    public IStandardStreamWriter Error => _streamWriter;
    public bool IsErrorRedirected => true;
    public bool IsInputRedirected => true;
}