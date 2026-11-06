# Stage 1: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AlertService.csproj", "./"]
RUN dotnet restore "AlertService.csproj"

COPY . .
RUN dotnet publish "AlertService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AlertService.dll"]