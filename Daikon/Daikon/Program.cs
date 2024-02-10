using Daikon.Services.Users;
using Daikon.Services.Utils;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();    
    builder.Services.AddSingleton<IAuthService, AuthService>();
    builder.Services.AddHostedService<TokenCleaner>();
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
