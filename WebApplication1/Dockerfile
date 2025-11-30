# ========== BUILD STAGE ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Disable fallback folders (fix for Visual Studio paths)
ENV RestoreAdditionalProjectFallbackFolders=
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_NOLOGO=true
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_HOME=/tmp

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ========== RUNTIME STAGE ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
