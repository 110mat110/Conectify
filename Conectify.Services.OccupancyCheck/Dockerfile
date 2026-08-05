#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Conectify.Services.TPLinkScraper/Conectify.Services.TPLinkScraper.csproj", "Conectify.Services.TPLinkScraper/"]
RUN dotnet restore "Conectify.Services.TPLinkScraper/Conectify.Services.TPLinkScraper.csproj"
COPY . .
WORKDIR "/src/Conectify.Services.TPLinkScraper"
RUN dotnet build "Conectify.Services.TPLinkScraper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.TPLinkScraper.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:80/health || curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Conectify.Services.TPLinkScraper.dll"]