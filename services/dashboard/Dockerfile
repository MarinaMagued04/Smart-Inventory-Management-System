# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first and restore packages.
# Docker caches this layer, so packages are only re-downloaded when the .csproj changes.
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code and publish a Release build.
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
# The runtime image is much smaller than the SDK image (no compiler, no build tools).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

# Run as the built-in non-root user that ships with .NET 8 images.
USER app

# Change "Dashboard.dll" if your project name is different.
ENTRYPOINT ["dotnet", "Dashboard.dll"]
