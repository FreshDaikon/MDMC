using System.Collections.Concurrent;
using System.Net.WebSockets;

public static class OrchGameList 
{
    public static ConcurrentDictionary<string, OrchGame> Games { get; }

    static OrchGameList()
    {
        Games = new ConcurrentDictionary<string, OrchGame>();
    }
}
