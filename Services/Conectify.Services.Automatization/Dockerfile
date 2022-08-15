#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/Conectify.Services.Automatization/Conectify.Services.Automatization.csproj", "Services/Conectify.Services.Automatization/"]
RUN dotnet restore "Services/Conectify.Services.Automatization/Conectify.Services.Automatization.csproj"
COPY . .
WORKDIR "/src/Services/Conectify.Services.Automatization"
RUN dotnet build "Conectify.Services.Automatization.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.Automatization.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conectify.Services.Automatization.dll"]