using Daikon.Models;

namespace Daikon.Services.Users;


public class AuthService : IAuthService
{
    private static readonly Dictionary<Guid, DateTime> ValidTokens = new();

    public void AddNewToken(Guid token, DateTime startTime)
    {
        ValidTokens.Add(token, startTime);
    }

    public void CleanTokens(DateTime time)
    {
        Console.WriteLine("Cleaning Tokens :" + ValidTokens.Count);
        foreach(KeyValuePair<Guid, DateTime> entry in ValidTokens)
        {
            TimeSpan duration = new TimeSpan(time.Ticks - entry.Value.Ticks);
            if(duration.TotalMinutes > 5)
            {
                ValidTokens.Remove(entry.Key);
            }
        }
    }

    public bool ValidateToken(Guid token)
    {
       if(ValidTokens.ContainsKey(token))
       {
            return true;
       }
       else
       {
            return false;
       }
    }
}