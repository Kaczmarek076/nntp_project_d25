
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

public sealed class TcpConnectionService : IAsyncDisposable
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task ConnectAsync(string hostname, int port)
    {
        await DisconnectAsync();

        var client = new TcpClient();

        try
        {
            await client.ConnectAsync(hostname, port);

            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream)
            {
                AutoFlush = true,
                NewLine = "\r\n"
            };

            _client = client;
            _reader = reader;
            _writer = writer;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public async Task<string?> ReadLineAsync()
    {
        if (_reader is null)
            throw new InvalidOperationException("Not connected.");

        return await _reader.ReadLineAsync();
    }
    
    
    public async Task<List<string>> ReadMultilineResponseAsync()
    {
        var lines = new List<string>();

        while (true)
        {
            string? line = await ReadLineAsync();

            if (line is null)
                throw new IOException("Connection closed by server.");

            // A single period marks the end of NNTP multiline data.
            if (line == ".")
                break;

            // NNTP dot-stuffing: lines starting with ".."
            // represent data lines starting with ".".
            if (line.StartsWith(".."))
                line = line[1..];

            lines.Add(line);
        }

        return lines;
    }
    

    public async Task SendLineAsync(string message)
    {
        if (_writer is null)
            throw new InvalidOperationException("Not connected.");

        await _writer.WriteLineAsync(message);
    }
    
    public async Task DisconnectAsync()
    {
        _reader?.Dispose();
        _writer?.Dispose();

        _reader = null;
        _writer = null;

        _client?.Dispose();
        _client = null;

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}

