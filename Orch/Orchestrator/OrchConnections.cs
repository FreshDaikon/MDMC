using System.Collections.Concurrent;
using System.Net.WebSockets;

public static class OrchConnections
{
    public static ConcurrentDictionary<WebSocket, OrchClient> Connections { get;}

    static OrchConnections()
    {
        Connections = new ConcurrentDictionary<WebSocket, OrchClient>();
    }
}
