#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MagiCache/MagiCache.csproj", "MagiCache/"]
RUN dotnet restore "MagiCache/MagiCache.csproj"
COPY . .
WORKDIR "/src/MagiCache"
RUN dotnet build "MagiCache.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MagiCache.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagiCache.dll"]

EXPOSE 80/tcp