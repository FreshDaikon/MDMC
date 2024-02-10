
using Daikon.Models;

namespace Daikon.Services.Users;

public interface IAuthService  
{
    void AddNewToken(Guid token, DateTime startTime);

    bool ValidateToken(Guid token);

    void CleanTokens(DateTime time);
}