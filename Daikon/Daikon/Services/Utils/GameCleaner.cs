using System.Net.NetworkInformation;
using Daikon.Controllers;
using Daikon.Services.Games;
using Daikon.Services.Users;

namespace Daikon.Services.Utils;


public class GameCleaner : BackgroundService
{
    private readonly PeriodicTimer _tokenTimer = new(TimeSpan.FromMinutes(2));
    private readonly IGameManager _gameManager;

    public GameCleaner(IGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _tokenTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            CheckGames();    
        }
    }

    private void CheckGames()
    {
        Console.WriteLine("Checking Games");
        _gameManager.CleanupGames();
    }
}