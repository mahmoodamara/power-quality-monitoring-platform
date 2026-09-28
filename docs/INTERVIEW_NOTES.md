# Interview notes

## Why SQL Server?
Telemetry history, device relationships, alerts, and audit data need strong relational consistency and queryable history. Composite indexes are used for the dominant access pattern: one device over a time range.

## Why Redis?
Redis stores only the latest live measurement per device. A live dashboard should not repeatedly query and sort a large historical table just to show the latest state.

## Why Channel<T>?
The API should persist and acknowledge telemetry quickly. Rule evaluation and alert generation run asynchronously through a bounded producer/consumer queue. Persisted `ProcessedAtUtc` provides restart recovery.

## Why idempotency?
Industrial devices retry when networks are unstable. `ExternalEventId` is unique so the same telemetry event can safely arrive more than once.

## Why WPF + MVVM?
The WPF client demonstrates desktop architecture separately from the backend. Views bind to view models; API/SignalR clients are services. The UI does not contain business rules.

## SQL index to explain
`Measurements(DeviceId, TimestampUtc DESC)` supports the common query: latest/history for one device over a time range. `ExternalEventId` is unique for ingestion idempotency.

## Reliability features to mention
- validation and typed application exceptions
- global exception middleware
- SQL transaction boundaries on relational providers
- worker recovery of unprocessed measurements
- retry/circuit-breaker for external webhook notifications
- structured Serilog logs and correlation IDs
- liveness/readiness endpoints
- OpenTelemetry tracing and metrics
- unit and integration tests
