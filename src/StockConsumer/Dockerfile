FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/StockConsumer/StockConsumer.csproj src/StockConsumer/
RUN dotnet restore src/StockConsumer/StockConsumer.csproj
COPY shared/ shared/
COPY src/StockConsumer/ src/StockConsumer/
WORKDIR /src/src/StockConsumer
RUN dotnet publish StockConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockConsumer.dll"]
