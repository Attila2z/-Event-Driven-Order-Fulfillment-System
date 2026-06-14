FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/PaymentConsumer/PaymentConsumer.csproj src/PaymentConsumer/
RUN dotnet restore src/PaymentConsumer/PaymentConsumer.csproj
COPY shared/ shared/
COPY src/PaymentConsumer/ src/PaymentConsumer/
WORKDIR /src/src/PaymentConsumer
RUN dotnet publish PaymentConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentConsumer.dll"]
