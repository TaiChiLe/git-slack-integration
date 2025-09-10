using NGitLab;
using NGitLab.Models;
public class GitLabApiService
{
    private readonly GitLabClient _gitLabClient;
    public GitLabApiService(IConfiguration configuration)
    {
        var gitlabUrl = configuration["Gitlab:Url"];
        var gitlabToken = configuration["Gitlab:PersonalAccessToken"];
        _gitLabClient = new GitLabClient(gitlabUrl, gitlabToken);
    }

    private Project? GetProjectByPath(string projectPath)
    {
        try
        {
            return _gitLabClient.Projects.GetByNamespacedPathAsync(projectPath).Result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Issue?> GetIssueByPath(string projectPath, int issueId)
    {
        try
        {
            var project = GetProjectByPath(projectPath);
            var issue = await _gitLabClient.Issues.GetAsync(project.Id, issueId);

            if (project == null) return null;

            return issue;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ProjectIssueNote>> GetIssueCommentsByPath(string projectPath, int issueId)
    {
        try
        {
            var project = GetProjectByPath(projectPath);
            var noteClient = _gitLabClient.GetProjectIssueNoteClient(project.Id);

            if (project == null) return new List<ProjectIssueNote>();

            return await Task.Run(() => noteClient.ForIssue(issueId).ToList());
        }
        catch
        {
            return new List<ProjectIssueNote>();
        }
    }

}

