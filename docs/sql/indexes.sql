-- The same indexes are configured in EF Core. This file documents the SQL design explicitly.
CREATE UNIQUE INDEX IX_Devices_SerialNumber ON Devices(SerialNumber);
CREATE UNIQUE INDEX IX_Measurements_ExternalEventId ON Measurements(ExternalEventId);
CREATE INDEX IX_Measurements_DeviceId_TimestampUtc ON Measurements(DeviceId, TimestampUtc DESC);
CREATE INDEX IX_Measurements_ProcessedAtUtc ON Measurements(ProcessedAtUtc);
CREATE INDEX IX_PowerEvents_DeviceId_CreatedAtUtc ON PowerEvents(DeviceId, CreatedAtUtc DESC);
CREATE INDEX IX_Alerts_Status_CreatedAtUtc ON Alerts(Status, CreatedAtUtc DESC);
CREATE INDEX IX_Alerts_DeviceId_Type_Status ON Alerts(DeviceId, Type, Status);
