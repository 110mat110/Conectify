#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# The aspnet image listens on 8080 by default; this service is pinned to 5013, the same port it uses
# when run from the IDE.
ENV ASPNETCORE_HTTP_PORTS=5013
EXPOSE 5013

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/Conectify.Services.Weather/Conectify.Services.Weather.csproj", "Services/Conectify.Services.Weather/"]
COPY ["Conectify.Database/Conectify.Database.csproj", "Conectify.Database/"]
COPY ["Conectify.Shared.Library/Conectify.Shared.Library.csproj", "Conectify.Shared.Library/"]
COPY ["Conectify.Shared.Maps/Conectify.Shared.Maps.csproj", "Conectify.Conectify.Shared.Maps/"]
COPY ["Conectify.Shared.Services/Conectify.Shared.Services.csproj", "Conectify.Conectify.Shared.Services/"]
COPY ["Services/Conectify.Services.Library/Conectify.Services.Library.csproj", "Conectify.Services.Library.Services/"]
RUN dotnet restore "Services/Conectify.Services.Weather/Conectify.Services.Weather.csproj"
COPY . .
WORKDIR "/src/Services/Conectify.Services.Weather"
RUN dotnet build "Conectify.Services.Weather.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.Weather.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5013/health || exit 1

ENTRYPOINT ["dotnet", "Conectify.Services.Weather.dll"]
