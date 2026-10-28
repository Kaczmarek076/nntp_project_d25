using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task<string?> ReadLineAsync()
    {
        if (_reader is null)
            throw new InvalidOperationException("Not connected.");

        return await _reader.ReadLineAsync();
    }


    public async Task<List<string>> ReadMultilineResponseAsync()
    {
        var lines = new List<string>();

        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(10));

        while (true)
        {
            string? line = await ReadLineAsync().WaitAsync(cts.Token);

            if (line is null)
                throw new IOException("Connection closed by server.");

            if (line == ".")
                break;

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


    private static readonly Dictionary<int, string> StatusCodes = new()
    {
        //place in a txt file and do the checks from the file.
        // 1xx - Informational
        [100] = "Continue",
        [101] = "Switching Protocols",
        [102] = "Processing",
        [103] = "Early Hints",

        // 2xx - Success
        [200] = "OK",
        [201] = "Created",
        [202] = "Accepted",
        [203] = "Non-Authoritative Information",
        [204] = "No Content",
        [205] = "Reset Content",
        [206] = "Partial Content",
        [207] = "Multi-Status",
        [208] = "Already Reported",
        [226] = "IM Used",

        // 3xx - Redirection
        [300] = "Multiple Choices",
        [301] = "Moved Permanently",
        [302] = "Found",
        [303] = "See Other",
        [304] = "Not Modified",
        [305] = "Use Proxy",
        [306] = "Unused",
        [307] = "Temporary Redirect",
        [308] = "Permanent Redirect",

        // 4xx - Client errors
        [400] = "Bad Request",
        [401] = "Unauthorized",
        [402] = "Payment Required",
        [403] = "Forbidden",
        [404] = "Not Found",
        [405] = "Method Not Allowed",
        [406] = "Not Acceptable",
        [407] = "Proxy Authentication Required",
        [408] = "Request Timeout",
        [409] = "Conflict",
        [410] = "Gone",
        [411] = "Length Required",
        [412] = "Precondition Failed",
        [413] = "Content Too Large",
        [414] = "URI Too Long",
        [415] = "Unsupported Media Type",
        [416] = "Range Not Satisfiable",
        [417] = "Expectation Failed",
        [418] = "I'm a teapot",
        [421] = "Misdirected Request",
        [422] = "Unprocessable Content",
        [423] = "Locked",
        [424] = "Failed Dependency",
        [425] = "Too Early",
        [426] = "Upgrade Required",
        [428] = "Precondition Required",
        [429] = "Too Many Requests",
        [431] = "Request Header Fields Too Large",
        [451] = "Unavailable For Legal Reasons",

        // 5xx - Server errors
        [500] = "Internal Server Error",
        [501] = "Not Implemented",
        [502] = "Bad Gateway",
        [503] = "Service Unavailable",
        [504] = "Gateway Timeout",
        [505] = "HTTP Version Not Supported",
        [506] = "Variant Also Negotiates",
        [507] = "Insufficient Storage",
        [508] = "Loop Detected",
        [510] = "Not Extended",
        [511] = "Network Authentication Required"
    };
}