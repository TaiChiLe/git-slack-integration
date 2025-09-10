using SlackNet.Interaction;
using SlackNet.WebApi;
using git_slack_integration.Services;
using SlackNet;
using NGitLab.Models;
using ILogger = Microsoft.Extensions.Logging.ILogger;

class SlackSlashCommandHandler : ISlashCommandHandler
{
    public const string SlashCommand = "/gitlab";
    private readonly ISlackApiClient _slack;
    private readonly IConfiguration _config;
    private readonly IssueLookupOrchestratorService _issueLookupOrchestrator;
    private readonly ILogger _logger;

    public SlackSlashCommandHandler(IssueLookupOrchestratorService issueLookupOrchestrator, IConfiguration config, ISlackApiClient slack)
    {
        _issueLookupOrchestrator = issueLookupOrchestrator;
        _config = config;
        _slack = slack;
    }

    public async Task<SlashCommandResponse> Handle(SlashCommand command)
    {
        int issueId = 0;
        if (!int.TryParse(command.Text, out int number))
        {
            _logger.LogWarning($"Invalid issue number provided in command: {command.Text}");
            return new SlashCommandResponse
            {
                Message = new Message
                {
                    Text = "Please provide a valid issue number after the command. Example: `/gitlab 42`"
                }
            };
        } else
        {
            issueId = number;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var projectPath = _config.GetValue<string>("GitLab:ProjectPath");
                var result = await _issueLookupOrchestrator.LookupIssueAsync(projectPath, issueId);

                if (!result.Found || result.Issue == null)
                {
                    await _slack.Chat.PostEphemeral( command.UserId ,new Message
                    {
                        Channel = command.ChannelId,
                        Text = $"Issue with ID {issueId} not found in project {projectPath}."

                    });
                    _logger.LogWarning($"Issue with ID {issueId} not found in project {projectPath}");
                    return;
                }

                Issue issue = result.Issue;
                string description = "";
                string tags = "";

                if (issue.Description.Length > 0)
                {
                    description =issue.Description;
                } else
                {
                    description = "No description provided";
                }

                if (issue.Labels.Length > 0)
                {
                    tags = string.Join(", ", issue.Labels);
                } else
                {
                    tags = "No tags";
                }

                if (description.Length > 300)
                {
                    description = description.Substring(0, 300) + "...";
                }

                await _slack.Chat.PostMessage(new Message
                {
                    Channel = command.ChannelId,
                    Text = $":pushpin: *{issue.Title}*\n" +
                    $"<{issue.WebUrl}| Link To Issue #{issueId}>\n" +
                    $"*State:* {issue.State}\n" +
                    $"*Description:* {description}\n" +
                    $"*Tags:* {tags}"

                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing slash command: {ex.Message}");
                await _slack.Chat.PostEphemeral(command.UserId, new Message
                {
                    Channel = command.ChannelId,
                    Text = "An error occured while fetching the issue. Please try again later."

                });
            }
        });

        return new SlashCommandResponse
        {
            ResponseType = ResponseType.Ephemeral,
            Message = new Message
            {
                Text = "Fetching GitLab issue, I’ll post it here shortly..."
            }
        };
        
    }
}