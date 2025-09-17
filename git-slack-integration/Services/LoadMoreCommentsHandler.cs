using NGitLab;
using NGitLab.Models;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;
using HtmlAgilityPack;

namespace git_slack_integration.Services
{
    public class LoadMoreCommentsHandler : IBlockActionHandler<ButtonAction>
    {
        private readonly IGitLabClient _gitlab;
        private readonly ISlackApiClient _slack;
        private readonly GitLabApiService _gitlabapi;
        private readonly IConfiguration _config;
        private readonly IssueLookupOrchestratorService _issueLookupOrchestrator;

        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.InnerText.Trim();
        }
        public LoadMoreCommentsHandler(IGitLabClient gitlab, ISlackApiClient slack, GitLabApiService gitlabapi, IConfiguration config, IssueLookupOrchestratorService issueLookupOrchestrator)
        {
            _gitlab = gitlab;
            _slack = slack;
            _gitlabapi = gitlabapi;
            _config = config;
            _issueLookupOrchestrator = issueLookupOrchestrator;
        }

        public async Task Handle(ButtonAction action, BlockActionRequest request)
        {
            if (action.ActionId != "load_more_comments") return;
            String[] valueString = action.Value.Split(',');
            int issueId = int.Parse(valueString[0].Split(':')[1]);
            int commentId = int.Parse(valueString[1].Split(':')[1]);
            var projectPath = _config.GetValue<string>("GitLab:ProjectPath");

            Console.WriteLine($"Issue:{issueId}, comment:{commentId}");
            var project = _gitlabapi.GetProjectByPath(projectPath);
            var notesClient = _gitlab.GetProjectIssueNoteClient(project.Id);

            var notes = notesClient.ForIssue(issueId).ToList();

            if (!notes.Any() || commentId >= notes.Count)
            {
                await _slack.Chat.PostMessage(new Message
                {
                    Channel = request.Container.ChannelId,
                    Text = "No more comments found for this issue."
                });
                return;
            } else { 
                var note = notes.ElementAt(commentId);
                var commentText = SanitizeHtml(note.Body);

                await _slack.Chat.PostMessage(new Message
                {
                    Channel = request.Container.ChannelId,
                    Blocks = new Block[]
                    {
                        new SectionBlock
                        {
                            Text = new Markdown 
                            { 
                                Text = $"*Author:* {note.Author.Name}" +
                                       $"\n*Comment:* {commentText}"
                            },
                        },

                        new ActionsBlock
                        {
                            Elements = new IActionElement[]
                            {
                                new SlackNet.Blocks.Button
                                {
                                    Text = new PlainText { Text = "Load next comment" },
                                    ActionId = "load_more_comments",
                                    Value = $"issue:{issueId},comment:{commentId + 1}" // increment for pagination
                                }
                            }
                        }

                    }
                });
            }
        }
    }

}
