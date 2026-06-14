FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/ShippingConsumer/ShippingConsumer.csproj src/ShippingConsumer/
RUN dotnet restore src/ShippingConsumer/ShippingConsumer.csproj
COPY shared/ shared/
COPY src/ShippingConsumer/ src/ShippingConsumer/
WORKDIR /src/src/ShippingConsumer
RUN dotnet publish ShippingConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShippingConsumer.dll"]
