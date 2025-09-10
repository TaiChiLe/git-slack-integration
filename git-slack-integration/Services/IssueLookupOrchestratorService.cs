namespace git_slack_integration.Services;
using NGitLab.Models;
using System.Threading.Tasks;
public class IssueLookupOrchestratorService
    {
        private readonly GitLabApiService _gitlabapi;
        public class IssueLookupResult
        {
            public bool Found { get; set; }
            public Issue? Issue { get; set; }
        }
        public IssueLookupOrchestratorService(GitLabApiService gitlabapi)
        {
            _gitlabapi = gitlabapi;
        }
        public async Task<IssueLookupResult> LookupIssueAsync(string projectPath, int issueIid)
        {
            var issue = await _gitlabapi.GetIssueByPath(projectPath, issueIid);
            if (issue == null)
            {
            return new IssueLookupResult
                {
                    Found = false,
                    Issue = null,
                };
            }

            return new IssueLookupResult

            {
                Found = true,
                Issue = issue,
            };
        }
    }

