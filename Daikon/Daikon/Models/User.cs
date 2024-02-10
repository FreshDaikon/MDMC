namespace Daikon.Models;

public class User
{
    public Guid Token { get; }
    public DateTime AuthTime { get; }
    public string Steamid { get; }
    public bool IsBan { get; }

    public User(Guid token, DateTime authTime, string steamid, bool isBan)
    {
        Token = token;
        AuthTime = authTime;
        Steamid = steamid;
        IsBan = isBan;
    }
}