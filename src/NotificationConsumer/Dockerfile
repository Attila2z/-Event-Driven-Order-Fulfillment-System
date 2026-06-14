FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/NotificationConsumer/NotificationConsumer.csproj src/NotificationConsumer/
RUN dotnet restore src/NotificationConsumer/NotificationConsumer.csproj
COPY shared/ shared/
COPY src/NotificationConsumer/ src/NotificationConsumer/
WORKDIR /src/src/NotificationConsumer
RUN dotnet publish NotificationConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationConsumer.dll"]
