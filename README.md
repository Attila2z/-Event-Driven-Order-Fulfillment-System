# Event-Driven Order Fulfillment System

A .NET 10 demo of an **event-driven microservices** architecture built with **MassTransit** and **RabbitMQ**. A single order placement fans out asynchronously across seven independent services.

---

## Architecture

```text
Client
  │
  ▼ POST /orders
OrderService (API)
  │ publishes OrderPlaced
  ▼
RabbitMQ (exchange / topic)
  ├──▶ StockConsumer          → StockReserved or StockUnavailable
  │         │
  │         ▼ StockReserved
  │    PaymentConsumer        → PaymentSucceeded or PaymentFailed
  │         │
  │         ▼ PaymentSucceeded
  │    OrderService           → OrderConfirmed (fan-out)
  │         ├──▶ InvoiceConsumer      (generates invoice)
  │         ├──▶ WarehouseConsumer    (picks items)
  │         ├──▶ ShippingConsumer     (creates shipping label)
  │         └──▶ NotificationConsumer (sends confirmation email)
  │
  │    PaymentFailed → OrderService → OrderCancelled
  │    StockUnavailable → OrderService → OrderCancelled
  └──▶ NotificationConsumer  (sends cancellation email on OrderCancelled)
```

**Shared Contracts** (`shared/Contracts/`) define all events as C# records, ensuring compile-time agreement between producers and consumers.

---

## Services

| Service | Role |
| --- | --- |
| `OrderService` | REST API — accepts orders, publishes `OrderPlaced`, listens for outcomes |
| `StockConsumer` | Checks inventory; publishes `StockReserved` or `StockUnavailable` |
| `PaymentConsumer` | Charges the customer; publishes `PaymentSucceeded` or `PaymentFailed` |
| `InvoiceConsumer` | Generates an invoice when an order is confirmed |
| `WarehouseConsumer` | Triggers warehouse picking when an order is confirmed |
| `ShippingConsumer` | Creates a shipping label when an order is confirmed |
| `NotificationConsumer` | Emails the customer on confirmation or cancellation |

---

## Scenarios Demonstrated

### 1. Happy path

`OrderPlaced` → `StockReserved` → `PaymentSucceeded` → `OrderConfirmed` → fan-out to Invoice, Warehouse, Shipping, Notification.

### 2. Idempotency (duplicate message delivery)
`StockConsumer` records each processed `MessageId` in a `ProcessedMessages` table (Postgres). A unique constraint rejects the second insert, and the consumer silently discards the duplicate without re-publishing.

Test endpoint: `POST /test/duplicate` publishes the same `OrderPlaced` twice with an identical `MessageId`.

### 3. Compensation saga (payment failure)
`PaymentFailed` triggers two compensating actions in parallel:
- `StockConsumer` releases the held reservation (`StockReleased`)
- `OrderService` cancels the order (`OrderCancelled` → `NotificationConsumer`)

Toggle via: `PAYMENT_FAILS=true` on `payment-consumer`.

### 4. Retries + Dead-Letter Queue
`ShippingConsumer` retries 3 times (2-second intervals). After three failures the message is dead-lettered to the `*_error` queue in RabbitMQ.

Toggle via: `SHIPPING_FAILS=true` on `shipping-consumer`.

### 5. Fan-out (parallel processing)
`OrderConfirmed` is consumed by four independent services simultaneously — each has its own named queue so no message is lost if one service is slow or restarting.

### 6. No-stock path
An item with `ProductId = "OUT-OF-STOCK"` causes `StockConsumer` to publish `StockUnavailable`. `OrderService` cancels the order immediately — Payment is never invoked.

---

## Running Locally

### Prerequisites
- .NET 10 SDK
- Docker + Docker Compose

### With Docker Compose (all services)
```bash
docker compose up --build
```

### Locally (separate terminals)
```powershell
./run-all.ps1
```
RabbitMQ and Postgres must be running first:
```bash
docker compose up rabbitmq postgres
```

### API

```http
POST http://localhost:5000/orders
Content-Type: application/json

{
  "customerId": "cust-42",
  "items": [{ "productId": "prod-1", "quantity": 2 }],
  "totalAmount": 49.99
}
```

Trigger the no-stock path:
```json
{ "customerId": "cust-1", "items": [{ "productId": "OUT-OF-STOCK", "quantity": 1 }], "totalAmount": 9.99 }
```

Test idempotency:

```http
POST http://localhost:5000/test/duplicate
```

RabbitMQ management UI: [http://localhost:15672](http://localhost:15672) (guest / guest)

---

## Testing

```bash
dotnet test
```

| Test file | Tool | What it covers |
| --- | --- | --- |
| `OrderPlacedConsumerTests.cs` | xUnit + Moq | Routing logic (in-stock / out-of-stock), idempotency record written |
| `IdempotencyIntegrationTests.cs` | xUnit + Testcontainers (real Postgres) | Unique constraint rejects duplicate `MessageId`; consumer discards second delivery |

> **WireMock.NET** would be used if any service called an external HTTP API (e.g. a payment gateway). The pattern would be: start a `WireMockServer`, register stub routes, inject its URL into the service under test, assert it received the expected request.

---

## Key Design Patterns

| Pattern | Where |
| --- | --- |
| **Saga (choreography)** | Distributed transaction across Stock → Payment → Order |
| **Compensating transaction** | Stock releases reservation on `PaymentFailed` |
| **Idempotent consumer** | `ProcessedMessages` table with unique constraint |
| **Dead-letter queue** | ShippingConsumer retries 3× then dead-letters |
| **Fan-out** | `OrderConfirmed` delivered to 4 services in parallel via named queues |
| **Event-driven choreography** | No central orchestrator — services react to events |
