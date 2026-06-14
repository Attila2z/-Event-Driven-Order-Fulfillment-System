## Start

```
cd ~/Documents/GitHub/-Event-Driven-Order-Fulfillment-System && docker compose up --build -d && sleep 15 && curl -s -X POST http://localhost:5000/orders -H "Content-Type: application/json" -d '{"customerId":"CUST-42","items":[{"productId":"PROD-001","quantity":2}],"totalAmount":49.99}'
```

http://localhost:15672 (guest / guest)

---

## Traffic

```
while true; do curl -s -X POST http://localhost:5000/orders -H "Content-Type: application/json" -d '{"customerId":"CUST-42","items":[{"productId":"PROD-001","quantity":2}],"totalAmount":49.99}' > /dev/null; sleep 2; done
```

---

## Kill

```
sudo snap restart docker && docker compose -f ~/Documents/GitHub/-Event-Driven-Order-Fulfillment-System/docker-compose.yml rm -f
```
