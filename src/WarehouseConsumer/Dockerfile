FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/WarehouseConsumer/WarehouseConsumer.csproj src/WarehouseConsumer/
RUN dotnet restore src/WarehouseConsumer/WarehouseConsumer.csproj
COPY shared/ shared/
COPY src/WarehouseConsumer/ src/WarehouseConsumer/
WORKDIR /src/src/WarehouseConsumer
RUN dotnet publish WarehouseConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WarehouseConsumer.dll"]
