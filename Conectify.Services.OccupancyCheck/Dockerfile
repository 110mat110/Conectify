#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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
ENTRYPOINT ["dotnet", "Conectify.Services.TPLinkScraper.dll"]