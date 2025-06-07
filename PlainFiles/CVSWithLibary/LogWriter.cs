﻿namespace CVSWithLibary;

public class LogWriter : IDisposable
{
    private readonly StreamWriter _writer;

    public LogWriter(string path)
    {
        _writer = new StreamWriter(path, append: true)
        {
            AutoFlush = true
        };
    }

    public void WriteLog(string level, string user, string message)
    {
        var timestamp = DateTime.Now.ToString("s");
        _writer.WriteLine($"{timestamp} [{level}] ({user}) {message}");
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }
}