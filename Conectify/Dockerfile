
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5000

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Conectify/Conectify.Server.csproj", "Conectify/"]
COPY ["Conectify.Database/Conectify.Database.csproj", "Conectify.Database/"]
COPY ["Conectify.Shared.Library/Conectify.Shared.Library.csproj", "Conectify.Shared.Library/"]
COPY ["Conectify.Shared.Maps/Conectify.Shared.Maps.csproj", "Conectify.Conectify.Shared.Maps/"]
COPY ["Conectify.Shared.Services/Conectify.Shared.Services.csproj", "Conectify.Conectify.Shared.Services/"]
RUN dotnet restore "Conectify/Conectify.Server.csproj"
COPY . .
WORKDIR "/src/Conectify"
RUN dotnet build "Conectify.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Conectify.Server.dll"]