using System.Net.Sockets;

namespace Server.Scripts;

public class ClientObject
{
    private readonly TcpClient _tcpClient;
    private readonly ServerObject _server;

    public ClientObject(TcpClient tcpClient, ServerObject server)
    {
        _tcpClient = tcpClient;
        _server = server;

        Stream stream = _tcpClient.GetStream();

        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }
    
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    public async Task ProcessAsync()
    {
        try
        {
            string? userName = await Reader.ReadLineAsync();
            string? message = $"{userName} вошел в чат";
            await _server.BroadcastMessageAsync(message, Id);
            Console.WriteLine(message);

            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null)
                        continue;

                    message = $"{userName}: {message}";
                    Console.WriteLine(message);
                    await _server.BroadcastMessageAsync(message, Id);
                }
                catch
                {
                    message = $"{userName} покинул чат";
                    Console.WriteLine(message);
                    await _server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            _server.RemoveConnection(Id);
        }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        _tcpClient.Close();
    }
    
    
}