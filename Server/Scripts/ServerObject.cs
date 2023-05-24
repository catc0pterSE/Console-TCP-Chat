using System.Net;
using System.Net.Sockets;

namespace Server.Scripts;

public class ServerObject
{
    private readonly TcpListener _tcpListener = new TcpListener(IPAddress.Any, 8888);
    private readonly List<ClientObject> _clients = new List<ClientObject>();

    protected internal void RemoveConnection(string id)
    {
        ClientObject? client = _clients.FirstOrDefault(client => client.Id == id);

        if (client != null)
            _clients.Remove(client);

        client?.Close();
    }

    protected internal async Task ListenAsync()
    {
        try
        {
            _tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                ClientObject client = new ClientObject(tcpClient, this);
                _clients.Add(client);
                Task.Run(client.ProcessAsync);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Disconnect();
        }
    }

    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        foreach (ClientObject client in _clients)
        {
            if (client.Id == id)
                continue;

            await client.Writer.WriteLineAsync(message);
            await client.Writer.FlushAsync();
        }
    }
    
    protected internal void Disconnect()
    {
        foreach (ClientObject client in _clients)
        {
            client.Close();
        }
        
        _tcpListener.Stop();
    }
}