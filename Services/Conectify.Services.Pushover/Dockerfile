#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/Conectify.Services.Pushover/Conectify.Services.Pushover.csproj", "Services/Conectify.Services.Pushover/"]
COPY ["Conectify.Database/Conectify.Database.csproj", "Conectify.Database/"]
COPY ["Conectify.Shared.Library/Conectify.Shared.Library.csproj", "Conectify.Shared.Library/"]
COPY ["Conectify.Shared.Maps/Conectify.Shared.Maps.csproj", "Conectify.Conectify.Shared.Maps/"]
COPY ["Conectify.Shared.Services/Conectify.Shared.Services.csproj", "Conectify.Conectify.Shared.Services/"]
COPY ["Services/Conectify.Services.Library/Conectify.Services.Library.csproj", "Conectify.Services.Library.Services/"]
RUN dotnet restore "Services/Conectify.Services.Pushover/Conectify.Services.Pushover.csproj"
COPY . .
WORKDIR "/src/Services/Conectify.Services.Pushover"
RUN dotnet build "Conectify.Services.Pushover.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.Pushover.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conectify.Services.Pushover.dll"]