-- One database per service. Each service owns its data; no service can
-- read another's tables. This enforces service boundaries at the data layer.
CREATE DATABASE orderdb;
CREATE DATABASE stockdb;
CREATE DATABASE paymentdb;
CREATE DATABASE notificationdb;
CREATE DATABASE invoicedb;
CREATE DATABASE warehousedb;
CREATE DATABASE shippingdb;