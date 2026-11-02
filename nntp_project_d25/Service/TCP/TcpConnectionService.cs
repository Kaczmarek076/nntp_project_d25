using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace nntp_project_d25.Service.TCP;

/// <summary>
/// Manages TCP network connections, stream readers/writers, and protocol-specific 
/// communication (such as handling NNTP multi-line responses) with a remote server.
/// </summary>
public sealed class TcpConnectionService : IAsyncDisposable
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;


    /// <summary>
    /// Protocol response codes that indicate the server will return a multi-line 
    /// block of data terminated by a single period (<c>.</c>) on its own line.
    /// </summary>
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


    /// <summary>
    /// Establishes an asynchronous TCP connection to the specified host and port, 
    /// initializing the underlying text reader and writer streams.
    /// </summary>
    /// <param name="hostname">The server hostname or IP address to connect to.</param>
    /// <param name="port">The network port number (typically 119 for NNTP).</param>
    /// <exception cref="SocketException">Thrown when the network connection fails.</exception>
    public async Task ConnectAsync(string hostname, int port)
    {
        await DisconnectAsync();
        var client = new TcpClient();

        try
        {
            client.Connect(hostname, port); // Perform asynchronous connection instead of blocking the thread.
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


    /// <summary>
    /// Reads a single line of text from the network stream asynchronously.
    /// </summary>
    /// <returns>The line read from the server, or <c>null</c> if the stream has ended.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the client is not connected.</exception>
    public Task<string?> ReadLineAsync()
    {
        if (_reader is null)
            throw new InvalidOperationException("Not connected.");

        return _reader.ReadLineAsync();
    }


    /// <summary>
    /// Reads a complete multi-line response from the server, handling protocol 
    /// termination rules (dot-stuffing and the ending period line).
    /// </summary>
    /// <returns>A list containing all response lines received from the server.</returns>
    /// <exception cref="IOException">Thrown if the server unexpectedly closes the connection.</exception>
    public async Task<List<string>> ReadMultilineResponseAsync()
    {
        var lines = new List<string>();
        string? line = await ReadLineAsync();

        if (line is null)
            throw new IOException("Server closed the connection.");
        lines.Add(line);


        bool isMultiLine =
            MultiLineCodes.Any(code => // Check if the response code indicates a multi-line data block follows.
                line.StartsWith(code.ToString()));

        if (!isMultiLine)
            return lines;

        while (true) // Read lines iteratively until the termination marker (.) is encountered.
        {
            line = await ReadLineAsync();

            if (line is null)
                throw new IOException("Server closed the connection.");

            if (line == ".") //Indicates the end of the multi-line block.
            {
                lines.Add(line);
                break;
            }

            if (line.StartsWith("..")) // Unescape dot-stuffed lines (where a leading dot is doubled by the server).
                line = line[1..];

            lines.Add(line);
        }

        return lines;
    }


    /// <summary>
    /// Sends a text command line to the server asynchronously.
    /// </summary>
    /// <param name="message">The command string to transmit.</param>
    /// <exception cref="InvalidOperationException">Thrown if the client is not connected.</exception>
    public async Task SendLineAsync(string message)
    {
        if (_writer is null)
            throw new InvalidOperationException("Not connected.");

        _writer.WriteLine(message);
    }


    /// <summary>
    /// Gracefully closes and disposes of the active network streams and TCP client.
    /// </summary>
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


    /// <summary>
    /// Performs asynchronous disposal of all unmanaged network resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}