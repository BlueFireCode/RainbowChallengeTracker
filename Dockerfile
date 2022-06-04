FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RainbowChallengeTracker/RainbowChallengeTracker.csproj", "RainbowChallengeTracker/"]
RUN dotnet restore "RainbowChallengeTracker/RainbowChallengeTracker.csproj"
COPY . .
WORKDIR "/src/RainbowChallengeTracker"
RUN dotnet build "RainbowChallengeTracker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RainbowChallengeTracker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RainbowChallengeTracker.dll"]