using git_slack_integration.Services;
using SlackNet.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GitLabApiService>();
builder.Services.AddSingleton<IssueLookupOrchestratorService>();

builder.Services.AddSlackNet(c => c
    .UseApiToken(builder.Configuration["Slack:BotToken"])
    .UseSigningSecret(builder.Configuration["Slack:SigningSecret"])
    .RegisterSlashCommandHandler<SlackSlashCommandHandler>(SlackSlashCommandHandler.SlashCommand)
);

var app = builder.Build();

app.UseSlackNet(c => c
    .UseSocketMode(false)
);

app.Run();
