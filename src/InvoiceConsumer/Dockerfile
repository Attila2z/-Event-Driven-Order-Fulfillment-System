FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/Contracts/Contracts.csproj shared/Contracts/
COPY src/InvoiceConsumer/InvoiceConsumer.csproj src/InvoiceConsumer/
RUN dotnet restore src/InvoiceConsumer/InvoiceConsumer.csproj
COPY shared/ shared/
COPY src/InvoiceConsumer/ src/InvoiceConsumer/
WORKDIR /src/src/InvoiceConsumer
RUN dotnet publish InvoiceConsumer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceConsumer.dll"]
