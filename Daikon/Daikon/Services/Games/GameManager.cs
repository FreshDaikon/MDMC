
using Daikon.Models.Games;
using Daikon.Services.Games;

public class GameManager : IGameManager
{
    private static List<Game> GameList = new();

    public void AddGame(Game game)
    {
        GameList.Add(game);
    }

    public void CleanupGames()
    {
        foreach(var game in GameList )
        {
            TimeSpan duration = new TimeSpan(game.LastHeartBeat.Ticks - DateTime.Now.Ticks);
            if(duration.TotalMinutes > 2)
            {
                GameList.Remove(game);                
            }
        }
    }

    public void NewHeartBeat(string session)
    {
        var game = GetGame(session);
        if(game != null)
        {
            game.LastHeartBeat = DateTime.Now;
        }
    }

    public Game? GetGame(string session)
    {
        if(GameList.Count > 0)
        {
            var game = GameList.Find(g => g.SessionId == session);
            return game; 
        }
        return null;
    }
}