using System.CommandLine;
using AICodeReviewer.Services;
using Microsoft.Extensions.Configuration;

namespace AICodeReviewer;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var rootCommand = new RootCommand("AI-powered code reviewer using Claude API");

        var prNumberOption = new Option<int>(
            name: "--pr-number",
            description: "Pull request number to review");
        prNumberOption.IsRequired = true;

        var ownerOption = new Option<string>(
            name: "--owner",
            description: "Repository owner");
        ownerOption.IsRequired = true;

        var repoOption = new Option<string>(
            name: "--repo",
            description: "Repository name");
        repoOption.IsRequired = true;

        var commitShaOption = new Option<string>(
            name: "--commit-sha",
            description: "Commit SHA to review");
        commitShaOption.IsRequired = true;

        var claudeApiKeyOption = new Option<string>(
            name: "--claude-api-key",
            description: "Claude API key (or use ANTHROPIC_API_KEY env var)",
            getDefaultValue: () => configuration["ANTHROPIC_API_KEY"] ?? string.Empty);

        var githubTokenOption = new Option<string>(
            name: "--github-token",
            description: "GitHub token (or use GITHUB_TOKEN env var)",
            getDefaultValue: () => configuration["GITHUB_TOKEN"] ?? string.Empty);

        rootCommand.AddOption(prNumberOption);
        rootCommand.AddOption(ownerOption);
        rootCommand.AddOption(repoOption);
        rootCommand.AddOption(commitShaOption);
        rootCommand.AddOption(claudeApiKeyOption);
        rootCommand.AddOption(githubTokenOption);

        rootCommand.SetHandler(async (prNumber, owner, repo, commitSha, claudeApiKey, githubToken) =>
        {
            await ReviewPullRequest(prNumber, owner, repo, commitSha, claudeApiKey, githubToken);
        }, prNumberOption, ownerOption, repoOption, commitShaOption, claudeApiKeyOption, githubTokenOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ReviewPullRequest(
        int prNumber,
        string owner,
        string repo,
        string commitSha,
        string claudeApiKey,
        string githubToken)
    {
        try
        {
            Console.WriteLine($"Starting AI code review for PR #{prNumber} in {owner}/{repo}");

            if (string.IsNullOrEmpty(claudeApiKey))
            {
                Console.WriteLine("Error: Claude API key not provided. Use --claude-api-key or set ANTHROPIC_API_KEY environment variable.");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(githubToken))
            {
                Console.WriteLine("Error: GitHub token not provided. Use --github-token or set GITHUB_TOKEN environment variable.");
                Environment.Exit(1);
            }

            // Initialize services
            var githubService = new GitHubService(githubToken, owner, repo);
            var claudeService = new ClaudeService(claudeApiKey);

            // Get PR changes
            Console.WriteLine("Fetching PR changes...");
            var changedFiles = await githubService.GetPullRequestChangesAsync(prNumber);
            Console.WriteLine($"Found {changedFiles.Count} changed files");

            if (!changedFiles.Any())
            {
                Console.WriteLine("No code files to review");
                return;
            }

            // Review with Claude
            Console.WriteLine("Analyzing code with Claude AI...");
            var review = await claudeService.ReviewCodeChangesAsync(changedFiles);
            Console.WriteLine($"Review completed: {review.OverallAssessment}");

            // Post review to GitHub
            Console.WriteLine("Posting review to GitHub...");
            await githubService.PostReviewCommentsAsync(prNumber, review, commitSha);
            Console.WriteLine("Review posted successfully!");

            // Print summary
            Console.WriteLine("\n=== Review Summary ===");
            Console.WriteLine($"Overall: {review.OverallAssessment}");
            Console.WriteLine($"Comments: {review.Comments.Count}");
            Console.WriteLine($"  - Errors: {review.Comments.Count(c => c.Severity == "error")}");
            Console.WriteLine($"  - Warnings: {review.Comments.Count(c => c.Severity == "warning")}");
            Console.WriteLine($"  - Info: {review.Comments.Count(c => c.Severity == "info")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during code review: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
