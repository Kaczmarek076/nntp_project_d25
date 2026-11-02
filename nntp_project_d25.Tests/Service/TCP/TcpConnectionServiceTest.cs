using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nntp_project_d25.Service.TCP;

namespace nntp_project_d25.Tests.Service.TCP;

[TestClass]
[TestSubject(typeof(TcpConnectionService))]
public class TcpConnectionServiceTest
{

    /// <summary>
    /// Tests that ConnectAsync successfully connects when a local TCP server is listening.
    /// </summary>
    [TestMethod]
    public async Task ConnectAsync_WhenServerIsActive_Succeeds()
    {
        // Arrange: Spin up a temporary local TCP listener on an available port (0 picks a random free port)
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var service = new TcpConnectionService();

        try
        {
            // Act: Attempt to connect to the active local server
            await service.ConnectAsync("127.0.0.1", port);

            // Assert: If no exception is thrown, the connection succeeded.
            // Clean up by disconnecting the service.
            await service.DisconnectAsync();
        }
        finally
        {
            // Ensure the temporary listener is stopped
            listener.Stop();
        }
    }

    /// <summary>
    /// Tests that ConnectAsync intentionally fails and throws a SocketException 
    /// when trying to connect to a port where no server is running.
    /// </summary>
    [TestMethod]
    public async Task ConnectAsync_WhenPortIsClosed_ThrowsSocketException()
    {
        // Arrange: Use an arbitrary port number that is guaranteed to have no active listener
        int closedPort = 59999;
        var service = new TcpConnectionService();

        // Act & Assert: Verify that a SocketException is thrown due to connection refusal
        await Assert.ThrowsAsync<SocketException>(async () =>
        {
            await service.ConnectAsync("127.0.0.1", closedPort);
        });
    }

    [TestMethod]
    public async Task TestReadMultilineResponseAsyncMethod()
    {
        // Arrange: Start a local TCP listener
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        // Background task simulating the NNTP server sending a multi-line response
        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            using var serverWriter = new StreamWriter(serverClient.GetStream()) { AutoFlush = true, NewLine = "\r\n" };

            await serverWriter.WriteLineAsync("101 Capability list follows");
            await serverWriter.WriteLineAsync("VERSION 2");
            await serverWriter.WriteLineAsync("."); // Standard NNTP multi-line termination marker
        });

        var service = new TcpConnectionService();
        await service.ConnectAsync("127.0.0.1", port);

        // Act: Call the method under test
        List<string> responseLines = await service.ReadMultilineResponseAsync();

        // Assert: Verify that all lines were read correctly
        Assert.AreEqual(3, responseLines.Count);
        Assert.AreEqual("101 Capability list follows", responseLines[0]);
        Assert.AreEqual("VERSION 2", responseLines[1]);
        Assert.AreEqual(".", responseLines[2]);

        // Clean up
        await serverTask;
        await service.DisconnectAsync();
        listener.Stop();
    }

    [TestMethod]
    public async Task TestSendLineAsyncMethod()
    {
        try
        {
            // Arrange: Start a local TCP listener to act as the server
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var service = new TcpConnectionService();
            await service.ConnectAsync("127.0.0.1", port);

            // Accept the client connection on the "server" side asynchronously
            var serverClientTask = listener.AcceptTcpClientAsync();

            // Act: Send a command line through your service
            string testMessage = "authinfo user test";
            await service.SendLineAsync(testMessage);

            // Assert: Read what the server actually received from the stream
            using var serverClient = await serverClientTask;
            using var serverReader = new StreamReader(serverClient.GetStream());
            string receivedLine = await serverReader.ReadLineAsync();

            Assert.AreEqual(testMessage, receivedLine);

            // Clean up
            await service.DisconnectAsync();
            listener.Stop();
        }
        catch (Exception e)
        {
            throw new Exception("TestSendLineAsyncMethod Exception", e);
        }
    }
}