FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/StubPaymentGateway/StubPaymentGateway.csproj src/StubPaymentGateway/
RUN dotnet restore src/StubPaymentGateway/StubPaymentGateway.csproj
COPY src/StubPaymentGateway/ src/StubPaymentGateway/
WORKDIR /src/src/StubPaymentGateway
RUN dotnet publish StubPaymentGateway.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StubPaymentGateway.dll"]
