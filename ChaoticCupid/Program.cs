using Server.Hubs;
using Server.Services;
using Server.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();
builder.Services.AddHostedService<CupidWorker>();

var app = builder.Build();

app.MapHub<CupidHub>("/cupidHub");

app.Run();