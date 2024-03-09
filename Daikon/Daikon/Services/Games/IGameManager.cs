using Daikon.Models;

namespace Daikon.Services.Games;

public interface IGameManager 
{
    void AddGame(Game game); 

    Game? GetGame(string session);

    void NewHeartBeat(string session);
    
    void CleanupGames();
}