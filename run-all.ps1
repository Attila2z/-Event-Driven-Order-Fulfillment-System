$services = @(
    "src/OrderService",
    "src/StockConsumer",
    "src/PaymentConsumer",
    "src/InvoiceConsumer",
    "src/WarehouseConsumer",
    "src/ShippingConsumer",
    "src/NotificationConsumer"
)

foreach ($svc in $services) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project $svc"
}