$ErrorActionPreference = "Stop"
$base = "http://localhost:8080"
$devices = Invoke-RestMethod "$base/api/devices"
if (-not $devices -or $devices.Count -eq 0) { throw "No seeded devices found." }
$device = $devices[0]
$eventId = "smoke-$([guid]::NewGuid().ToString('N'))"
$body = @{
  eventId = $eventId
  deviceId = $device.id
  voltage = 258.4
  current = 14.2
  frequency = 50.01
  powerFactor = 0.94
  activePower = 3.2
  timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json
Write-Host "Sending telemetry to $($device.name)..."
$r1 = Invoke-RestMethod -Method Post -Uri "$base/api/telemetry" -ContentType "application/json" -Body $body
if ($r1.duplicate) { throw "First telemetry call should not be duplicate." }
$r2 = Invoke-RestMethod -Method Post -Uri "$base/api/telemetry" -ContentType "application/json" -Body $body
if (-not $r2.duplicate) { throw "Second telemetry call should be idempotent duplicate." }
Start-Sleep -Seconds 2
$alerts = Invoke-RestMethod "$base/api/alerts?limit=20"
if (-not ($alerts | Where-Object { $_.deviceId -eq $device.id -and $_.type -eq "OverVoltage" })) { throw "Expected OverVoltage alert was not created." }
$stats = Invoke-RestMethod "$base/api/devices/$($device.id)/statistics?hours=24"
Write-Host "Smoke test passed. Samples=$($stats.sampleCount), Events=$($stats.eventCount)"
