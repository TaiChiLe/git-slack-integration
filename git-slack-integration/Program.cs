using git_slack_integration.Services;
using SlackNet;
using SlackNet.AspNetCore;
using SlackNet.Blocks;
using NGitLab;

var builder = WebApplication.CreateBuilder(args);

// App configuration
var configuration = builder.Configuration;

// Validate GitLab config at startup
var gitLabUrl = configuration["GitLab:Url"];
var gitLabToken = configuration["GitLab:PersonalAccessToken"];

if (string.IsNullOrWhiteSpace(gitLabUrl))
    throw new InvalidOperationException("GitLab URL missing from configuration");
if (string.IsNullOrWhiteSpace(gitLabToken))
    throw new InvalidOperationException("GitLab token missing from configuration");

// 1. GitLab client (singleton)
var gitLabClient = new GitLabClient(gitLabUrl, gitLabToken);
builder.Services.AddSingleton<IGitLabClient>(gitLabClient);

// 2. Slack API client (singleton)
var slackToken = configuration["Slack:BotToken"];
if (string.IsNullOrWhiteSpace(slackToken))
    throw new InvalidOperationException("Slack BotToken missing from configuration");

builder.Services.AddSingleton<ISlackApiClient>(new SlackApiClient(slackToken));

// 3. Your services
builder.Services.AddSingleton<GitLabApiService>();
builder.Services.AddSingleton<IssueLookupOrchestratorService>();
builder.Services.AddScoped<LoadMoreCommentsHandler>(); // Scoped because it handles requests
builder.Services.AddScoped<SlackSlashCommandHandler>();

// 4. SlackNet registration
builder.Services.AddSlackNet(c => c
    .UseApiToken(slackToken)
    .UseSigningSecret(configuration["Slack:SigningSecret"])
    .RegisterSlashCommandHandler<SlackSlashCommandHandler>(SlackSlashCommandHandler.SlashCommand)
    .RegisterBlockActionHandler<ButtonAction, LoadMoreCommentsHandler>()
);

var app = builder.Build();
app.UseSlackNet(c => c.UseSocketMode(false));

// Run the app
app.Run();
