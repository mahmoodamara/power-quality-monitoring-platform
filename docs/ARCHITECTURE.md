# Architecture

## Processing flow

```text
Power Meter / Simulator
        |
        | POST /api/telemetry
        v
ASP.NET Core API
        |
        +--> validation + idempotency
        +--> SQL Server (source of truth)
        +--> Redis (latest device state)
        +--> SignalR measurement update
        |
        v
Channel<MeasurementEnvelope>
        |
        v
TelemetryProcessingWorker
        |
        v
Rule Engine
  |     |      |       |
  |     |      |       +--> LowPowerFactor
  |     |      +----------> FrequencyDeviation
  |     +-----------------> UnderVoltage
  +-----------------------> OverVoltage
        |
        +--> PowerEvent + Alert in SQL
        +--> SignalR alert
        +--> optional webhook with Polly retry/circuit breaker

OfflineDetectionWorker
        |
        +--> marks silent devices offline
        +--> creates DeviceOffline alert
```

## Projects

- `PowerQuality.Domain`: entities and enums only.
- `PowerQuality.Application`: contracts, DTOs, validation, and rule abstractions.
- `PowerQuality.Infrastructure`: EF Core, SQL Server, Redis, services, health checks, notification reliability.
- `PowerQuality.Worker`: bounded `Channel<T>`, event-processing worker, offline detector.
- `PowerQuality.Api`: REST API, SignalR, middleware, Swagger, Serilog, OpenTelemetry.
- `PowerQuality.Desktop`: WPF/MVVM monitoring client.
- `PowerQuality.UnitTests`: deterministic rule and validation tests.
- `PowerQuality.IntegrationTests`: API + EF Core in-memory end-to-end checks.

## Reliability decisions

- `ExternalEventId` has a unique index, so device retries do not create duplicate measurements.
- SQL Server is the historical source of truth; Redis is only a performance cache for live state.
- Measurements have `ProcessedAtUtc`; unprocessed rows are recovered by the worker after restart.
- Worker failures are retried in memory, while persisted pending rows protect against process restarts.
- External webhook delivery uses exponential retry plus circuit breaker via Polly.
- Correlation IDs are returned in `X-Correlation-ID` and included in structured logs.
- Readiness health checks verify both SQL Server and Redis.
