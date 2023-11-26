#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/Conectify.Services.Dashboard/Conectify.Services.Dashboard.csproj", "Services/Conectify.Services.Dashboard/"]
RUN dotnet restore "Services/Conectify.Services.Dashboard/Conectify.Services.Dashboard.csproj"
COPY . .
WORKDIR "/src/Services/Conectify.Services.Dashboard"
RUN dotnet build "Conectify.Services.Dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conectify.Services.Dashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conectify.Services.Dashboard.dll"]