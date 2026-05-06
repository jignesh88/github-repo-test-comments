# RetailService - AI-Enabled Code Review Demo

A .NET 8.0 microservice demonstrating AI-powered code reviews using Claude API. This project showcases how to integrate Claude as a Principal/Staff Engineer-level code reviewer in your CI/CD pipeline.

## Features

- **REST API**: Product management service with CRUD operations
- **Clean Architecture**: Separation of concerns with Core, Infrastructure, and API layers
- **Entity Framework Core**: SQL Server database with code-first migrations
- **Docker Support**: Fully containerized with Docker Compose
- **AI Code Review**: Automated PR reviews using Claude Sonnet 4
- **GitHub Actions**: CI/CD pipeline with automated code review

## Project Structure

```
├── src/
│   ├── RetailService.API/          # Web API layer
│   ├── RetailService.Core/         # Domain entities and interfaces
│   └── RetailService.Infrastructure/ # Data access and repositories
├── tests/
│   └── RetailService.Tests/        # Unit and integration tests
├── tools/
│   └── AICodeReviewer/             # AI code review agent
├── .github/
│   └── workflows/                  # GitHub Actions workflows
├── Dockerfile
└── docker-compose.yml
```

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- Claude API key (get from [Anthropic Console](https://console.anthropic.com/))
- GitHub account with repository access

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd github-repo-test-comments
```

### 2. Configure Environment Variables

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` and add your API keys:

```env
ANTHROPIC_API_KEY=your_claude_api_key_here
GITHUB_TOKEN=your_github_token_here
```

### 3. Run with Docker Compose

```bash
docker-compose up -d
```

This will start:
- SQL Server on port 1433
- RetailService API on port 5000

### 4. Access the API

- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health

### 5. Run Locally (Without Docker)

```bash
# Restore dependencies
dotnet restore

# Update database connection string in appsettings.json
# Then run migrations
dotnet ef database update --project src/RetailService.Infrastructure --startup-project src/RetailService.API

# Run the application
dotnet run --project src/RetailService.API
```

## API Endpoints

### Products

- `GET /api/products` - Get all products
- `GET /api/products?category={category}` - Filter by category
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Soft delete product

### Example Request

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "sku": "LAP-001",
    "stockQuantity": 50,
    "category": "Electronics"
  }'
```

## AI Code Review Setup

### 1. GitHub Secrets Configuration

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

- `ANTHROPIC_API_KEY`: Your Claude API key
- `GITHUB_TOKEN`: Automatically provided by GitHub Actions

### 2. How It Works

When a pull request is created or updated:

1. GitHub Actions triggers the `ai-code-review.yml` workflow
2. The workflow builds the AICodeReviewer tool
3. AICodeReviewer fetches changed files from the PR
4. Claude analyzes the code with Principal/Staff Engineer perspective
5. Review comments are posted directly on the PR

### 3. Review Focus Areas

The AI reviewer evaluates:

- **Architecture**: SOLID principles, design patterns, separation of concerns
- **Performance**: Query optimization, memory usage, async/await patterns
- **Security**: SQL injection, XSS, authentication, authorization
- **Code Quality**: Naming conventions, complexity, readability
- **Best Practices**: .NET/C# standards, error handling, logging
- **Testing**: Testability, test coverage suggestions
- **Database**: EF Core usage, migrations, indexing

### 4. Manual Code Review

You can also run the reviewer locally:

```bash
# Build the tool
dotnet build tools/AICodeReviewer/AICodeReviewer.csproj

# Run review
dotnet run --project tools/AICodeReviewer/AICodeReviewer.csproj -- \
  --pr-number 123 \
  --owner your-username \
  --repo github-repo-test-comments \
  --commit-sha abc123def456
```

## Testing the AI Reviewer

### Create a Test PR

1. Create a new branch:
   ```bash
   git checkout -b feature/add-order-service
   ```

2. Make some changes (e.g., add a new controller or service)

3. Commit and push:
   ```bash
   git add .
   git commit -m "Add order service"
   git push origin feature/add-order-service
   ```

4. Create a PR on GitHub

5. Watch the AI review comments appear automatically!

## Example Review Output

The AI reviewer provides:

```markdown
## AI Code Review (Principal Engineer Level)

### Summary
This PR adds order management functionality with proper separation of concerns...

### Overall Assessment: `NEEDS_CHANGES`

### Strengths
- Clean architecture with proper layering
- Good use of async/await patterns
- Comprehensive DTO validation

### Areas for Improvement
- Missing transaction handling in order creation
- Potential N+1 query issue in order details endpoint
- Error responses lack detailed error codes

---
**Review Statistics**
- Total Comments: 12
- Errors: 2
- Warnings: 6
- Info: 4
```

Inline comments appear on specific lines with:
- Category (architecture, performance, security, etc.)
- Severity (error, warning, info)
- Detailed explanation
- Code suggestions

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Building for Production

```bash
# Build release
dotnet build --configuration Release

# Publish
dotnet publish src/RetailService.API -c Release -o ./publish

# Build Docker image
docker build -t retailservice:1.0.0 .
```

## Troubleshooting

### Database Connection Issues

If the API can't connect to SQL Server:

```bash
# Check if SQL Server is running
docker ps | grep sqlserver

# View SQL Server logs
docker logs retailservice-sqlserver
```

### AI Review Not Working

1. Verify GitHub secrets are set correctly
2. Check workflow logs in Actions tab
3. Ensure ANTHROPIC_API_KEY has sufficient credits
4. Verify repository permissions for GitHub token

## Architecture Decisions

### Why Clean Architecture?

- **Testability**: Core logic independent of infrastructure
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Easy to swap implementations

### Why Claude for Code Review?

- **Context Understanding**: Claude excels at understanding large codebases
- **Senior-Level Insights**: Provides architectural and design pattern feedback
- **Actionable Suggestions**: Goes beyond linting to provide meaningful improvements

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Push and create a PR
5. Watch the AI review your code!

## License

MIT License - feel free to use this for your own projects.

## Resources

- [Claude API Documentation](https://docs.anthropic.com/)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [GitHub Actions](https://docs.github.com/en/actions)

## Questions?

Open an issue or reach out to the maintainers.

---

**Built with Claude Code** - Demonstrating AI-powered development workflows
