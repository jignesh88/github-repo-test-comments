FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["RetailService.sln", "./"]
COPY ["src/RetailService.API/RetailService.API.csproj", "src/RetailService.API/"]
COPY ["src/RetailService.Core/RetailService.Core.csproj", "src/RetailService.Core/"]
COPY ["src/RetailService.Infrastructure/RetailService.Infrastructure.csproj", "src/RetailService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "RetailService.sln"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/RetailService.API"
RUN dotnet build "RetailService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RetailService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RetailService.API.dll"]
