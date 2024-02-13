using Daikon.Services.Games;
using Daikon.Services.Users;
using Daikon.Services.Utils;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();    
    builder.Services.AddSingleton<IAuthService, AuthService>();
    builder.Services.AddSingleton<IGameManager, GameManager>();
    builder.Services.AddHostedService<TokenCleaner>();
    builder.Services.AddHostedService<GameCleaner>();
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
