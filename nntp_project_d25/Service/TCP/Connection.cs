using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Data;

namespace nntp_project_d25.Service.TCP;

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
            Console.WriteLine(hostname + ":" + port);
            client.Connect(hostname, port);
            NetworkStream stream = client.GetStream();

            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream)
            {
                AutoFlush = true,
                NewLine = "\r\n"
            };

            _client = client;
            _reader = reader;
            _writer = writer;
            Console.WriteLine("Connected");
        }
        catch
        {
            client.Dispose();
            Console.WriteLine("Could not connect to server.");
            throw;
        }
    }

    public Task<string?> ReadLineAsync()
    {
        if (_reader is null)
            throw new InvalidOperationException("Not connected.");

        return _reader.ReadLineAsync();
    }

    private static readonly List<int> MultiLineCodes = new()
    {
        100, // HELP: Help text follows
        101, // CAPABILITIES: Capability list follows
        215, // LIST: Information follows
        220, // ARTICLE: Article follows
        221, // HEAD: Article headers follow
        222, // BODY: Article body follows
        224, // OVER: Article overview follows
        225, // HDR: Article headers follow
        230, // NEWNEWS: List of new articles follows
        231 // NEWGROUPS: List of new newsgroups follows
    };

    public async Task<List<string>> ReadMultilineResponseAsync()
    {
        var lines = new List<string>();
        string? line = await ReadLineAsync();
        Console.WriteLine(line);
        
        if (line is null)
            throw new IOException("Server closed the connection.");
        lines.Add(line);

        bool isMultiLine = MultiLineCodes.Any(code =>
            line.StartsWith(code.ToString()));
        
        if (!isMultiLine)
            return lines;

        while (true)
        {
            line = await ReadLineAsync();

            if (line is null)
                throw new IOException("Server closed the connection.");

            if (line == ".")
            {
                lines.Add(line);
                break;
            }

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

        _writer.WriteLine(message);
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