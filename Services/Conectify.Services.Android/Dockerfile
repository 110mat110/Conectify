FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/Conectify.Services.Android/Conectify.Services.Android.csproj", "Services/Conectify.Services.Android/"]
RUN dotnet restore "Services/Conectify.Services.Android/Conectify.Services.Android.csproj"
COPY . .
WORKDIR "/src/Services/Conectify.Services.Android"
RUN dotnet build "Conectify.Services.Android.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.Android.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conectify.Services.Android.dll"]
