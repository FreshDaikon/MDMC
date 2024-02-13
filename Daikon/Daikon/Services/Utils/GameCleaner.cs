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
            Console.WriteLine("Checking Games to cleanup..");
            _gameManager.CleanupGames();
        }
    }
}