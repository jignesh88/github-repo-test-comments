using Octokit;
using AICodeReviewer.Models;

namespace AICodeReviewer.Services;

public class GitHubService
{
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;

    public GitHubService(string token, string owner, string repo)
    {
        _client = new GitHubClient(new ProductHeaderValue("AICodeReviewer"))
        {
            Credentials = new Credentials(token)
        };
        _owner = owner;
        _repo = repo;
    }

    public async Task<Dictionary<string, string>> GetPullRequestChangesAsync(int prNumber)
    {
        // Get PR details to get the head ref
        var pullRequest = await _client.PullRequest.Get(_owner, _repo, prNumber);
        var headRef = pullRequest.Head.Ref;

        var files = await _client.PullRequest.Files(_owner, _repo, prNumber);
        var changedFiles = new Dictionary<string, string>();

        Console.WriteLine($"PR head ref: {headRef}");
        Console.WriteLine($"Found {files.Count} files in PR");

        foreach (var file in files)
        {
            // Skip deleted files and non-code files
            if (file.Status == "removed" || !IsCodeFile(file.FileName))
            {
                Console.WriteLine($"Skipping {file.FileName} (status: {file.Status})");
                continue;
            }

            try
            {
                Console.WriteLine($"Fetching content for: {file.FileName}");

                // Get the file content from the PR's head branch
                var fileContent = await _client.Repository.Content.GetAllContentsByRef(
                    _owner,
                    _repo,
                    file.FileName,
                    headRef
                );

                if (fileContent.Count > 0)
                {
                    var content = fileContent[0].Content;
                    changedFiles[file.FileName] = content;
                    Console.WriteLine($"Successfully fetched {file.FileName} ({content.Length} chars)");
                }
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine($"File not found: {file.FileName} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching {file.FileName}: {ex.Message}");
            }
        }

        Console.WriteLine($"Total files fetched: {changedFiles.Count}");
        return changedFiles;
    }

    public async Task PostReviewCommentsAsync(int prNumber, CodeReviewResult review, string commitSha)
    {
        // Post overall review comment
        var reviewBody = FormatReviewComment(review);

        var pullRequestReviewCreate = new PullRequestReviewCreate
        {
            CommitId = commitSha,
            Body = reviewBody,
            Event = review.OverallAssessment switch
            {
                "APPROVED" => PullRequestReviewEvent.Approve,
                "REJECTED" => PullRequestReviewEvent.RequestChanges,
                _ => PullRequestReviewEvent.Comment
            }
        };

        // Note: Inline comments are not included because GitHub requires diff positions,
        // not file line numbers. All detailed comments are included in the review body instead.

        await _client.PullRequest.Review.Create(_owner, _repo, prNumber, pullRequestReviewCreate);
    }

    private string FormatReviewComment(CodeReviewResult review)
    {
        var comment = $@"## AI Code Review (Principal Engineer Level)

### Summary
{review.Summary}

### Overall Assessment: `{review.OverallAssessment}`

";

        if (review.Strengths.Any())
        {
            comment += "### Strengths\n";
            foreach (var strength in review.Strengths)
            {
                comment += $"- {strength}\n";
            }
            comment += "\n";
        }

        if (review.AreasForImprovement.Any())
        {
            comment += "### Areas for Improvement\n";
            foreach (var area in review.AreasForImprovement)
            {
                comment += $"- {area}\n";
            }
            comment += "\n";
        }

        // Add detailed comments grouped by file
        if (review.Comments.Any())
        {
            comment += "### Detailed Code Review Comments\n\n";

            var commentsByFile = review.Comments.GroupBy(c => c.FilePath);
            foreach (var fileGroup in commentsByFile)
            {
                comment += $"#### 📄 `{fileGroup.Key}`\n\n";

                foreach (var reviewComment in fileGroup)
                {
                    var severityEmoji = reviewComment.Severity switch
                    {
                        "error" => "🔴",
                        "warning" => "⚠️",
                        _ => "ℹ️"
                    };

                    comment += $"{severityEmoji} **{reviewComment.Category}** (Line {reviewComment.Line})\n";
                    comment += $"{reviewComment.Comment}\n";

                    if (!string.IsNullOrEmpty(reviewComment.Suggestion))
                    {
                        comment += $"\n**Suggestion:**\n```csharp\n{reviewComment.Suggestion}\n```\n";
                    }

                    comment += "\n";
                }
            }
        }

        comment += $@"
---
**Review Statistics**
- Total Comments: {review.Comments.Count}
- Errors: {review.Comments.Count(c => c.Severity == "error")}
- Warnings: {review.Comments.Count(c => c.Severity == "warning")}
- Info: {review.Comments.Count(c => c.Severity == "info")}

*This review was generated by Claude AI (Staff Engineer persona)*
";

        return comment;
    }

    private bool IsCodeFile(string fileName)
    {
        var codeExtensions = new[] { ".cs", ".csproj", ".sln", ".json", ".yaml", ".yml", ".sql" };
        return codeExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
