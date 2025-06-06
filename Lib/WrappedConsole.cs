using System;
using System.IO;
using System.Text;
using System.Threading;

namespace wow_addon_backuper;

public delegate void ConsoleOutputChanged(object sender, string? value);

public class WrappedConsole : TextWriter
{
    private readonly TextWriter _originalConsoleStream;
    private static WrappedConsole? _instance = null;
    private static readonly Lock _mutex = new();
    public override Encoding Encoding => _originalConsoleStream.Encoding;
    public event ConsoleOutputChanged? Changed;

    private void OnChanged(string? value)
    {
        Changed?.Invoke(this, value);
    }

    private WrappedConsole(TextWriter originalTextWriter)
    {
        _originalConsoleStream = originalTextWriter;
    }

    public static WrappedConsole Instance()
    {
        if (_instance == null)
        {
            lock (_mutex)
            {
                _instance ??= new WrappedConsole(Console.Out);
                Console.SetOut(_instance);
            }
        }
        return _instance;
    }

    public override void WriteLine(string? value)
    {
        _originalConsoleStream.WriteLine(value);

        OnChanged(value);
    }
}