# Power Quality Monitoring Platform

A real-time industrial monitoring platform built with **C# / .NET 8** for ingesting, processing, storing, and visualizing electrical telemetry from power meters.

The project is intentionally designed as a backend-engineering portfolio project: it demonstrates REST APIs, SQL design and indexing, LINQ, `async/await`, layered architecture, dependency injection, background processing, idempotency, reliability patterns, logging, monitoring, unit/integration testing, and a **WPF/MVVM** desktop client.

## Tech stack

- **Backend:** ASP.NET Core 8 Web API, C#
- **Persistence:** SQL Server, Entity Framework Core
- **Caching:** Redis
- **Real-time:** SignalR
- **Background processing:** `Channel<T>` + `BackgroundService`
- **Reliability:** Polly retry + circuit breaker, idempotent ingestion, restart recovery
- **Logging/observability:** Serilog, correlation IDs, Health Checks, OpenTelemetry
- **Desktop:** WPF + MVVM
- **Testing:** xUnit, FluentAssertions, ASP.NET Core integration tests
- **DevOps:** Docker Compose, GitHub Actions

## Main capabilities

- Manage industrial sites and power-meter devices.
- Receive telemetry through `POST /api/telemetry`.
- Validate electrical measurements and reject invalid input.
- Prevent duplicate ingestion using a unique `ExternalEventId`.
- Persist telemetry in SQL Server as the source of truth.
- Cache each device's latest live state in Redis.
- Process telemetry asynchronously through a bounded `Channel<T>`.
- Detect over-voltage, under-voltage, frequency deviations, and low power factor.
- Create power events and alerts.
- Detect devices that stop reporting and generate `DeviceOffline` alerts.
- Push live measurements, alerts, and device-state changes through SignalR.
- Query historical telemetry and aggregated statistics using LINQ.
- Acknowledge alerts with an audit trail.
- Retry optional external webhook notifications with exponential backoff and circuit breaker.
- Recover persisted-but-unprocessed telemetry after a process restart.
- Monitor SQL Server and Redis through readiness health checks.
- Visualize devices, live readings, and alerts in a WPF/MVVM desktop client.

## Architecture

```text
Power Meter / Simulator
        |
        | POST /api/telemetry
        v
ASP.NET Core API
        |
        +--> Validation + idempotency
        +--> SQL Server
        +--> Redis latest-state cache
        +--> SignalR live update
        |
        v
Channel<MeasurementEnvelope>
        |
        v
TelemetryProcessingWorker
        |
        v
Power Quality Rule Engine
        |
        +--> PowerEvent
        +--> Alert
        +--> SignalR
        +--> optional webhook (Polly)

OfflineDetectionWorker
        |
        +--> DeviceOffline detection

WPF / MVVM Desktop Client
        |
        +--> REST API
        +--> SignalR
```

More detail: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Project structure

```text
src/
  PowerQuality.Domain/          Entities and enums
  PowerQuality.Application/     DTOs, interfaces, validation, rule engine
  PowerQuality.Infrastructure/  EF Core, SQL Server, Redis, services, health checks
  PowerQuality.Worker/          Channel<T> and background workers
  PowerQuality.Api/             REST API, SignalR, middleware, Swagger, telemetry
  PowerQuality.Desktop/         WPF/MVVM monitoring client

tests/
  PowerQuality.UnitTests/
  PowerQuality.IntegrationTests/

docs/
scripts/
```

## Quick start with Docker

### Requirements

- Docker Desktop
- Docker Compose

### 1. Start the backend

From the repository root:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Or use:

```powershell
./scripts/start.ps1
```

The API waits/retries while SQL Server is starting, then creates and seeds the database automatically.

### 2. Open the API

- Swagger: `http://localhost:8080/swagger`
- Liveness: `http://localhost:8080/health/live`
- Readiness: `http://localhost:8080/health/ready`
- SignalR hub: `http://localhost:8080/hubs/monitoring`

Two demo devices are seeded automatically.

### 3. Run the smoke test

```powershell
./scripts/smoke-test.ps1
```

The smoke test verifies:

1. seeded devices exist;
2. telemetry is accepted;
3. duplicate telemetry is idempotent;
4. an over-voltage alert is created;
5. statistics are queryable.

### 4. Generate live telemetry

```powershell
./scripts/generate-telemetry.ps1 -Count 120 -DelayMs 500
```

The simulator periodically creates voltage spikes and low-power-factor samples so you can see alerts in real time.

## WPF / MVVM client

The desktop application is intentionally separate from the API and demonstrates WPF architecture rather than putting business logic in the UI.

Requirements:

- Windows 10/11
- .NET 8 SDK
- backend running on `http://localhost:8080`

Run:

```powershell
dotnet run --project src/PowerQuality.Desktop/PowerQuality.Desktop.csproj
```

The client displays:

- devices and online/offline state;
- latest voltage/current/frequency/power factor;
- active/recent alerts;
- real-time SignalR updates.

## Example telemetry

First get a device ID from `GET /api/devices`, then send:

```json
{
  "eventId": "evt-982734",
  "deviceId": "REPLACE-WITH-DEVICE-GUID",
  "voltage": 257.4,
  "current": 18.7,
  "frequency": 49.98,
  "powerFactor": 0.91,
  "activePower": 4.21,
  "timestampUtc": "2026-09-28T12:41:00Z"
}
```

A voltage above `250 V` creates an `OverVoltage` critical event and alert.

## REST API

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/sites` | List sites |
| POST | `/api/sites` | Create site |
| GET | `/api/devices` | List devices |
| GET | `/api/devices/{id}` | Device details |
| POST | `/api/devices` | Create device |
| GET | `/api/devices/{id}/live` | Latest live measurement |
| GET | `/api/devices/{id}/measurements` | Historical telemetry |
| GET | `/api/devices/{id}/statistics?hours=24` | Aggregated statistics |
| POST | `/api/telemetry` | Ingest telemetry |
| GET | `/api/events` | Power-quality events |
| GET | `/api/alerts` | Alerts |
| POST | `/api/alerts/{id}/acknowledge` | Acknowledge alert |

## Rule engine

The application uses small polymorphic rules implementing `IPowerQualityRule`:

- `OverVoltageRule`: voltage > 250 V
- `UnderVoltageRule`: voltage < 210 V
- `FrequencyDeviationRule`: frequency > 50.5 Hz or < 49.5 Hz
- `LowPowerFactorRule`: power factor < 0.85

Adding another rule does not require changing the telemetry controller or worker.

## SQL and indexing

Important indexes are configured with EF Core and documented in [`docs/sql/indexes.sql`](docs/sql/indexes.sql).

The most important telemetry index is conceptually:

```sql
CREATE INDEX IX_Measurements_DeviceId_TimestampUtc
ON Measurements(DeviceId, TimestampUtc DESC);
```

It supports the common access pattern: query one device over a time range ordered by newest measurement.

`ExternalEventId` has a **unique** index to enforce ingestion idempotency at database level.

## Reliability decisions

### Idempotent ingestion

Meters may retry on network failure. If `evt-123` arrives twice, only one `Measurement` is stored and the second request returns a duplicate response.

### Fast API + asynchronous processing

The API persists telemetry, updates the cache, and enqueues the measurement. Rule evaluation runs in a `BackgroundService` consuming a bounded `Channel<T>`.

### Restart recovery

Each measurement has `ProcessedAtUtc`. When the application starts, the worker re-enqueues persisted measurements that were never processed.

### Redis is not the source of truth

Redis stores latest live state only. A cache outage does not lose historical telemetry because SQL Server remains authoritative.

### External-service resilience

Optional webhook notifications use Polly with:

- retries after 1s, 2s, and 4s;
- circuit breaker after repeated transient failures.

## Logging and monitoring

- Serilog structured console logs.
- `X-Correlation-ID` accepted/generated per request and returned to the caller.
- `/health/live` checks that the process is alive.
- `/health/ready` checks SQL Server and Redis.
- OpenTelemetry instruments ASP.NET Core requests, outgoing HTTP calls, and custom platform counters.

Custom metrics:

- `telemetry_received_total`
- `telemetry_rejected_total`
- `alerts_created_total`

## Tests

Run unit and integration tests:

```powershell
dotnet test tests/PowerQuality.UnitTests/PowerQuality.UnitTests.csproj
dotnet test tests/PowerQuality.IntegrationTests/PowerQuality.IntegrationTests.csproj
```

Unit tests cover the rule engine and validation. Integration tests start the API with an in-memory database and verify seeded devices plus idempotent telemetry ingestion.

## Build manually

Backend:

```powershell
dotnet restore src/PowerQuality.Api/PowerQuality.Api.csproj
dotnet build src/PowerQuality.Api/PowerQuality.Api.csproj -c Release
```

WPF client on Windows:

```powershell
dotnet build src/PowerQuality.Desktop/PowerQuality.Desktop.csproj -c Release
```

## GitHub portfolio description

Suggested repository description:

> Real-time industrial power-quality monitoring platform built with ASP.NET Core, SQL Server, Redis, SignalR and WPF/MVVM, featuring idempotent telemetry ingestion, async event processing, alerting, health monitoring and automated tests.

Suggested topics:

`csharp` `dotnet` `aspnet-core` `sql-server` `entity-framework-core` `redis` `signalr` `wpf` `mvvm` `xunit` `docker` `backend` `real-time` `power-quality`

## Interview preparation

See [`docs/INTERVIEW_NOTES.md`](docs/INTERVIEW_NOTES.md) for concise explanations of architectural decisions and the engineering trade-offs this project demonstrates.
