using System.Net.NetworkInformation;
using Daikon.Controllers;
using Daikon.Services.Users;

namespace Daikon.Services.Utils;


public class TokenCleaner : BackgroundService
{
    private readonly PeriodicTimer _tokenTimer = new(TimeSpan.FromMinutes(5));
    private readonly IAuthService _authService;

    public TokenCleaner(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _tokenTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            CheckTokens();    
        }
    }

    private void CheckTokens()
    {
        Console.WriteLine("Checking Tokens");
        _authService.CleanTokens(DateTime.Now);
    }
}