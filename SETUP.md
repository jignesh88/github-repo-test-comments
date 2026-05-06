# Setup Guide - AI Code Review with Claude

This guide will help you set up the AI-enabled code review system in your GitHub repository.

## Prerequisites

1. **Anthropic Claude API Key**
   - Sign up at https://console.anthropic.com/
   - Navigate to API Keys section
   - Generate a new API key
   - Keep it secure - you'll need it for GitHub Secrets

2. **GitHub Repository**
   - You need admin access to configure secrets
   - Repository must allow GitHub Actions

3. **Development Tools**
   - .NET 8.0 SDK: https://dotnet.microsoft.com/download
   - Docker Desktop: https://www.docker.com/products/docker-desktop
   - Git

## Step-by-Step Setup

### 1. Configure GitHub Secrets

Go to your repository on GitHub:

1. Click on **Settings** tab
2. Navigate to **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secret:
   - Name: `ANTHROPIC_API_KEY`
   - Value: Your Claude API key from Anthropic Console

Note: `GITHUB_TOKEN` is automatically provided by GitHub Actions.

### 2. Verify GitHub Actions Permissions

1. In repository **Settings** → **Actions** → **General**
2. Under "Workflow permissions", ensure:
   - ✅ Read and write permissions
   - ✅ Allow GitHub Actions to create and approve pull requests

### 3. Push Code to GitHub

```bash
# Initialize git if not already done
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit with AI code review"

# Add remote (replace with your repository URL)
git remote add origin https://github.com/your-username/your-repo.git

# Push to main branch
git push -u origin main
```

### 4. Test the AI Code Review

#### Create a Test Pull Request

```bash
# Create a new branch
git checkout -b test/ai-review

# Make a small change (e.g., add a comment to a controller)
# Edit src/RetailService.API/Controllers/ProductsController.cs
# Add a comment or modify a method

# Commit and push
git add .
git commit -m "Test: Add logging to product controller"
git push origin test/ai-review
```

#### Create the PR on GitHub

1. Go to your repository on GitHub
2. Click "Pull requests" tab
3. Click "New pull request"
4. Select `test/ai-review` as the compare branch
5. Click "Create pull request"
6. Fill in title and description
7. Click "Create pull request"

#### Watch the Magic Happen

1. Navigate to the "Actions" tab
2. You should see "AI Code Review" workflow running
3. Wait for it to complete (usually 1-2 minutes)
4. Go back to your PR
5. You'll see AI-generated review comments!

### 5. Local Testing (Optional)

You can test the AI reviewer locally before pushing:

```bash
# Set environment variables
export ANTHROPIC_API_KEY="your-key-here"
export GITHUB_TOKEN="your-github-token"

# Build the reviewer
dotnet build tools/AICodeReviewer/AICodeReviewer.csproj

# Run it (replace values with your PR details)
dotnet run --project tools/AICodeReviewer/AICodeReviewer.csproj -- \
  --pr-number 1 \
  --owner your-username \
  --repo your-repo \
  --commit-sha abc123def456
```

### 6. Running the Application

#### Using Docker (Recommended)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

Access the API:
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

#### Using .NET CLI

```bash
# Update connection string in appsettings.json
# Point to your SQL Server instance

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update --project src/RetailService.Infrastructure --startup-project src/RetailService.API

# Run the API
dotnet run --project src/RetailService.API
```

## Customizing the AI Reviewer

### Modify Review Criteria

Edit `tools/AICodeReviewer/Services/ClaudeService.cs`:

```csharp
private string BuildReviewPrompt(Dictionary<string, string> changedFiles)
{
    // Customize the prompt to focus on specific areas
    // Add your company's coding standards
    // Include specific frameworks or patterns to check
}
```

### Change Review Severity

Edit `tools/AICodeReviewer/Services/GitHubService.cs`:

```csharp
Event = review.OverallAssessment switch
{
    "APPROVED" => PullRequestReviewEvent.Approve,
    "REJECTED" => PullRequestReviewEvent.RequestChanges,
    _ => PullRequestReviewEvent.Comment  // Change this behavior
}
```

### Customize Review Format

Edit the `FormatReviewComment` method in `GitHubService.cs` to change how reviews are displayed.

## Troubleshooting

### Workflow Fails with "API Key Invalid"

- Verify `ANTHROPIC_API_KEY` is set correctly in GitHub Secrets
- Check if your API key has available credits
- Ensure the key hasn't expired

### No Review Comments Appear

- Check if the PR contains code file changes (.cs, .csproj, etc.)
- View workflow logs for errors
- Verify GitHub token has write permissions

### "Rate Limit Exceeded" Error

- Claude API has rate limits based on your plan
- Consider adding delays between requests
- Check your Anthropic Console for rate limit info

### Docker Container Fails to Start

```bash
# Check logs
docker-compose logs sqlserver
docker-compose logs retailservice-api

# Verify ports aren't in use
lsof -i :1433  # SQL Server
lsof -i :5000  # API

# Reset everything
docker-compose down -v
docker-compose up -d
```

### Database Connection Errors

- Ensure SQL Server container is healthy: `docker ps`
- Wait 30 seconds after starting for SQL Server to initialize
- Check connection string in `appsettings.json`

## Advanced Configuration

### Using Different Claude Models

Edit `ClaudeService.cs`:

```csharp
private const string MODEL = "claude-sonnet-4-20250514";  // Change to different model
```

Available models:
- `claude-sonnet-4-20250514` - Latest, most capable
- `claude-opus-4-20250514` - Highest intelligence
- `claude-haiku-4-20250514` - Fast and economical

### Adding Custom Prompts

Create custom review templates in `ClaudeService.cs`:

```csharp
private string BuildSecurityReviewPrompt(...)
{
    // Focus on security vulnerabilities
}

private string BuildPerformanceReviewPrompt(...)
{
    // Focus on performance optimization
}
```

### Integrating with Other CI/CD

The AI reviewer can be integrated with:

- **Azure DevOps**: Create a pipeline task
- **GitLab CI**: Add to `.gitlab-ci.yml`
- **Jenkins**: Create a pipeline stage
- **CircleCI**: Add to `.circleci/config.yml`

Example for Azure DevOps:

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run AI Code Review'
  inputs:
    command: 'run'
    projects: 'tools/AICodeReviewer/AICodeReviewer.csproj'
    arguments: '--pr-number $(System.PullRequest.PullRequestNumber) --owner $(Build.Repository.Name) --repo $(Build.Repository.Name) --commit-sha $(Build.SourceVersion)'
  env:
    ANTHROPIC_API_KEY: $(AnthropicApiKey)
    AZURE_DEVOPS_TOKEN: $(System.AccessToken)
```

## Cost Considerations

Claude API pricing (as of 2024):

- Input: ~$3 per million tokens
- Output: ~$15 per million tokens

Typical PR review (500 lines):
- Approximate cost: $0.05 - $0.20 per review
- Monthly cost (100 PRs): $5 - $20

Tips to reduce costs:
- Use Haiku model for simpler reviews
- Filter file types (exclude generated code)
- Set up review triggers (only on specific labels)

## Security Best Practices

1. **Never commit API keys** - Always use GitHub Secrets
2. **Limit file access** - Review only necessary files
3. **Validate inputs** - Sanitize PR data before sending to Claude
4. **Monitor usage** - Track API usage in Anthropic Console
5. **Rotate keys** - Regularly update API keys

## Next Steps

- ✅ Customize review prompts for your team
- ✅ Add more test coverage
- ✅ Integrate with Slack/Teams for notifications
- ✅ Create dashboards for review metrics
- ✅ Train team on interpreting AI feedback

## Support

For issues:
1. Check workflow logs in GitHub Actions
2. Review Anthropic API status: https://status.anthropic.com/
3. Consult Claude API docs: https://docs.anthropic.com/

---

Happy coding with AI-powered reviews! 🚀
