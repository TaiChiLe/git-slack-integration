# GitLab-Slack Integration

A .NET 8 web application that seamlessly integrates GitLab issues with Slack, allowing team members to fetch and display GitLab issue information directly in Slack channels using slash commands.

## 🚀 Features

- **Slash Command Integration**: Use `/gitlab <issue_id>` to fetch GitLab issue details instantly
- **Rich Issue Display**: Shows comprehensive issue information including:
  - Issue title with direct link
  - Current state (Open/Closed)
  - Creation date
  - Description (auto-truncated for readability)
  - Labels/tags
- **Interactive Comments**: Browse through issue comments with pagination
- **Error Handling**: Graceful error handling with user-friendly messages
- **HTML Sanitization**: Cleans HTML content from GitLab for optimal Slack display
- **Ephemeral Messages**: Error messages are sent privately to avoid channel clutter

## 📋 Prerequisites

- .NET 8 SDK
- GitLab account with Personal Access Token
- Slack workspace with appropriate bot permissions

## 📦 Dependencies

The application uses the following NuGet packages:

```xml
<PackageReference Include="HtmlAgilityPack" Version="1.12.3" />
<PackageReference Include="NgitLab" Version="10.2.0" />
<PackageReference Include="SlackNet" Version="0.17.4" />
<PackageReference Include="SlackNet.AspNetCore" Version="0.17.1" />
```

## ⚙️ Configuration

### Required Configuration Values

Configure the following settings in your `appsettings.json` or user secrets:

```json
{
  "GitLab": {
    "Url": "https://gitlab.com",
    "ProjectPath": "your-namespace/your-project",
    "PersonalAccessToken": "your-gitlab-token"
  },
  "Slack": {
    "BotToken": "xoxb-your-slack-bot-token",
    "SigningSecret": "your-slack-signing-secret"
  }
}
```

### Setting Up User Secrets (Recommended for Development)

```bash
dotnet user-secrets init
dotnet user-secrets set "GitLab:PersonalAccessToken" "your-gitlab-token"
dotnet user-secrets set "Slack:BotToken" "xoxb-your-slack-bot-token"
dotnet user-secrets set "Slack:SigningSecret" "your-slack-signing-secret"
```

## 🔧 Slack App Setup

1. **Create a Slack App**

   - Go to https://api.slack.com/apps
   - Click "Create New App" → "From scratch"
   - Name your app and select your workspace

2. **Configure OAuth Scopes**

   - Navigate to "OAuth & Permissions"
   - Add the following Bot Token Scopes:
     - `chat:write` - Post messages to channels
     - `commands` - Add slash commands

3. **Add Slash Command**

   - Go to "Slash Commands"
   - Click "Create New Command"
   - Command: `/gitlab`
   - Request URL: `https://your-domain.com/slack/events`
   - Short Description: "Fetch GitLab issue information"

4. **Enable Interactive Components**

   - Go to "Interactivity & Shortcuts"
   - Enable Interactivity
   - Request URL: `https://your-domain.com/slack/events`

5. **Install App to Workspace**
   - Go to "Install App"
   - Click "Install to Workspace"
   - Copy the Bot User OAuth Token for configuration

## 🦊 GitLab Setup

1. **Generate Personal Access Token**

   - Go to GitLab → User Settings → Access Tokens
   - Create token with `api` scope
   - Ensure the token has access to your target project

2. **Project Configuration**
   - Update `appsettings.json` with your project path
   - Format: `namespace/project-name`

## 🏃‍♂️ Running the Application

### Development

```bash
cd git-slack-integration
dotnet restore
dotnet run
```

The application will start on `http://localhost:5000` by default.

### Production

```bash
dotnet publish -c Release -o ./publish
# Deploy the published files to your hosting environment
```

## 📱 Usage

### Fetching GitLab Issues

In any Slack channel where the bot is present, use:

```
/gitlab 123
```

This will display:

- 📌 Issue title with clickable link
- Current state (Open/Closed)
- Creation date
- Description (truncated if over 300 characters)
- Labels/tags
- "Load Comments" button for additional details

### Loading Comments

Click the "Load Comments" button to view issue comments. The system supports pagination, allowing you to browse through all comments one by one.

## 🏗️ Architecture

The application follows a clean, service-oriented architecture:

### Core Services

- **`SlackSlashCommandHandler`** - Handles `/gitlab` slash commands
- **`LoadMoreCommentsHandler`** - Manages comment loading interactions
- **`GitLabApiService`** - Interfaces with GitLab API
- **`IssueLookupOrchestratorService`** - Orchestrates issue lookup operations

### Project Structure

```
git-slack-integration/
├── Services/
│   ├── SlackSlashCommandHandler.cs      # Slash command processing
│   ├── LoadMoreCommentsHandler.cs       # Comment pagination
│   ├── GitLabApiService.cs              # GitLab API integration
│   └── IssueLookupOrchestratorService.cs # Business logic orchestration
├── Program.cs                           # Application startup & DI
├── appsettings.json                     # Configuration
├── appsettings.Development.json         # Development settings
└── git-slack-integration.csproj        # Project file
```

### Dependency Injection Setup

The application uses built-in .NET DI container with the following service lifetimes:

- **Singleton**: GitLab client, Slack API client, core services
- **Scoped**: Request handlers (per HTTP request)

## 🛡️ Error Handling

Comprehensive error handling includes:

- **Invalid Issue Numbers**: Returns helpful error messages
- **Missing Issues**: Shows "not found" messages
- **API Errors**: Logged with user-friendly messages displayed
- **Private Error Messages**: All errors sent as ephemeral messages to avoid channel spam

## 🚦 Example Workflow

1. User types `/gitlab 42` in Slack channel
2. Bot responds with "Fetching GitLab issue, I'll post it here shortly..."
3. Application fetches issue #42 from configured GitLab project
4. Rich message posted with issue details and "Load Comments" button
5. User can click button to paginate through issue comments

## 🔒 Security Considerations

- Store sensitive tokens in user secrets or environment variables
- Use HTTPS for all webhook endpoints
- Validate Slack request signatures using signing secret
- Implement rate limiting for production deployments

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### Common Issues

1. **"GitLab token missing from configuration"**

   - Ensure `GitLab:PersonalAccessToken` is set in user secrets or config

2. **"Slack BotToken missing from configuration"**

   - Verify `Slack:BotToken` is properly configured

3. **Slash command not responding**

   - Check that your webhook URL is accessible from the internet
   - Verify Slack signing secret configuration

4. **Issue not found errors**
   - Confirm the project path in configuration matches your GitLab project
   - Ensure the access token has permissions for the specified project

### Debug Mode

Run with debug logging enabled:

```bash
dotnet run --environment Development
```

---

Built with ❤️ using .NET 8, SlackNet, and NGitLab
