using AICodeReviewer.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AICodeReviewer.Services;

public class ClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string CLAUDE_API_URL = "https://api.anthropic.com/v1/messages";
    private const string MODEL = "claude-sonnet-4-20250514";
    private const string ANTHROPIC_VERSION = "2023-06-01";

    public ClaudeService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", ANTHROPIC_VERSION);
    }

    public async Task<CodeReviewResult> ReviewCodeChangesAsync(Dictionary<string, string> changedFiles)
    {
        var prompt = BuildReviewPrompt(changedFiles);

        var requestBody = new
        {
            model = MODEL,
            max_tokens = 4096,
            temperature = 0.3,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(CLAUDE_API_URL, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Claude API error: {response.StatusCode} - {responseBody}");
        }

        var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseBody);
        var reviewText = claudeResponse?.content?.FirstOrDefault()?.text ?? string.Empty;

        return ParseReviewResponse(reviewText);
    }

    private class ClaudeApiResponse
    {
        public List<ContentBlock>? content { get; set; }
    }

    private class ContentBlock
    {
        public string? type { get; set; }
        public string? text { get; set; }
    }

    private string BuildReviewPrompt(Dictionary<string, string> changedFiles)
    {
        var filesContent = string.Join("\n\n", changedFiles.Select(kvp =>
            $"### File: {kvp.Key}\n```\n{kvp.Value}\n```"));

        return $@"You are a Principal/Staff Software Engineer conducting a thorough code review. Analyze the following code changes and provide a comprehensive review.

Review these files with focus on:
1. **Architecture & Design Patterns**: Assess adherence to SOLID principles, DRY, separation of concerns
2. **Performance**: Identify potential bottlenecks, inefficient queries, memory leaks
3. **Security**: Look for vulnerabilities (SQL injection, XSS, authentication issues, data exposure)
4. **Code Quality**: Check naming conventions, code complexity, readability
5. **Best Practices**: Ensure industry standards for .NET/C# are followed
6. **Testing**: Evaluate testability and suggest test cases
7. **Error Handling**: Verify proper exception handling and logging
8. **Database**: Check EF Core usage, migrations, query optimization

{filesContent}

Provide your review in the following JSON format:
{{
  ""summary"": ""Brief overview of the changes (2-3 sentences)"",
  ""overallAssessment"": ""APPROVED | NEEDS_CHANGES | REJECTED"",
  ""strengths"": [""List key strengths""],
  ""areasForImprovement"": [""List areas needing improvement""],
  ""comments"": [
    {{
      ""filePath"": ""path/to/file"",
      ""line"": 0,
      ""severity"": ""info|warning|error"",
      ""category"": ""architecture|performance|security|maintainability|best-practices"",
      ""comment"": ""Detailed comment explaining the issue"",
      ""suggestion"": ""Specific code suggestion or recommendation""
    }}
  ]
}}

Be thorough, specific, and constructive. Provide code examples in suggestions where appropriate.";
    }

    private CodeReviewResult ParseReviewResponse(string response)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<CodeReviewResult>(jsonContent, options);
                return result ?? new CodeReviewResult { Summary = "Failed to parse review" };
            }

            // Fallback if JSON parsing fails
            return new CodeReviewResult
            {
                Summary = response,
                OverallAssessment = "NEEDS_REVIEW",
                Comments = new List<ReviewComment>()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing Claude response: {ex.Message}");
            return new CodeReviewResult
            {
                Summary = $"Review completed but failed to parse: {response}",
                OverallAssessment = "NEEDS_MANUAL_REVIEW",
                Comments = new List<ReviewComment>()
            };
        }
    }
}
