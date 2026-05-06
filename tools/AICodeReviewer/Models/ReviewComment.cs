namespace AICodeReviewer.Models;

public class ReviewComment
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public string Severity { get; set; } = "info"; // info, warning, error
    public string Category { get; set; } = string.Empty; // architecture, performance, security, maintainability, best-practices
    public string Comment { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
}

public class CodeReviewResult
{
    public string Summary { get; set; } = string.Empty;
    public List<ReviewComment> Comments { get; set; } = new();
    public string OverallAssessment { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> AreasForImprovement { get; set; } = new();
}
