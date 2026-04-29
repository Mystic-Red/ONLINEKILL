using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

int port = args.Length > 0 && int.TryParse(args[0], out int parsed) ? parsed : 27040;
using UdpClient udp = new(port);
ConcurrentDictionary<string, ClientState> clients = new();
int nextId = 1;

Console.WriteLine($"OnlineKill dedicated server listening on UDP 0.0.0.0:{port}");
Console.WriteLine("Forward this UDP port if players are joining over the internet.");

while (true)
{
    UdpReceiveResult result = await udp.ReceiveAsync();
    string key = result.RemoteEndPoint.ToString();
    string packet = Encoding.UTF8.GetString(result.Buffer);
    string[] parts = packet.Split('|');

    if (parts.Length == 0)
    {
        continue;
    }

    if (parts[0] == "HELLO" && parts.Length >= 2)
    {
        ClientState client = clients.GetOrAdd(key, _ => new ClientState(nextId++, result.RemoteEndPoint));
        client.Name = Unescape(parts[1]);
        client.LastSeen = DateTimeOffset.UtcNow;
        await Send(udp, result.RemoteEndPoint, $"WELCOME|{client.Id}");
        Console.WriteLine($"JOIN #{client.Id} {client.Name} from {key}");
        continue;
    }

    if (!clients.TryGetValue(key, out ClientState? sender))
    {
        continue;
    }

    sender.LastSeen = DateTimeOffset.UtcNow;

    if (parts[0] == "BYE")
    {
        clients.TryRemove(key, out _);
        Console.WriteLine($"LEAVE #{sender.Id} {sender.Name}");
        await BroadcastLeave(udp, clients.Values, sender.Id);
        continue;
    }

    if (parts[0] == "POSE" && parts.Length >= 9)
    {
        string snap = string.Create(CultureInfo.InvariantCulture, $"SNAP|{sender.Id}|{Escape(sender.Name)}|{parts[1]}|{parts[2]}|{parts[3]}|{parts[4]}|{parts[5]}|{parts[6]}|{parts[7]}|{parts[8]}");
        foreach (ClientState client in clients.Values)
        {
            if (client.Id != sender.Id)
            {
                await Send(udp, client.EndPoint, snap);
            }
        }
    }

    foreach (KeyValuePair<string, ClientState> stale in clients.Where(x => DateTimeOffset.UtcNow - x.Value.LastSeen > TimeSpan.FromSeconds(15)).ToArray())
    {
        clients.TryRemove(stale.Key, out _);
        Console.WriteLine($"TIMEOUT #{stale.Value.Id} {stale.Value.Name}");
        await BroadcastLeave(udp, clients.Values, stale.Value.Id);
    }
}

static Task Send(UdpClient udp, IPEndPoint endPoint, string text)
{
    byte[] data = Encoding.UTF8.GetBytes(text);
    return udp.SendAsync(data, data.Length, endPoint);
}

static async Task BroadcastLeave(UdpClient udp, IEnumerable<ClientState> clients, int id)
{
    string packet = $"LEAVE|{id}";
    foreach (ClientState client in clients)
    {
        await Send(udp, client.EndPoint, packet);
    }
}

static string Escape(string value)
{
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
}

static string Unescape(string value)
{
    try
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }
    catch
    {
        return "Player";
    }
}

sealed class ClientState
{
    public ClientState(int id, IPEndPoint endPoint)
    {
        Id = id;
        EndPoint = endPoint;
    }

    public int Id { get; }
    public IPEndPoint EndPoint { get; }
    public string Name { get; set; } = "Player";
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
}
