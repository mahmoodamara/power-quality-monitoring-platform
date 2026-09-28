param([int]$Count = 120, [int]$DelayMs = 500)
$ErrorActionPreference = "Stop"
$base = "http://localhost:8080"
$devices = Invoke-RestMethod "$base/api/devices"
if (-not $devices -or $devices.Count -eq 0) { throw "No devices found." }
$device = $devices[0]
Write-Host "Generating $Count samples for $($device.name)..."
for ($i=0; $i -lt $Count; $i++) {
  $spike = ($i % 25 -eq 0)
  $voltage = if ($spike) { 257 + (Get-Random -Minimum 0 -Maximum 3) } else { 228 + (Get-Random -Minimum 0 -Maximum 8) }
  $pf = if ($i % 40 -eq 0) { 0.78 } else { 0.93 + ((Get-Random -Minimum 0 -Maximum 5) / 100) }
  $body = @{
    eventId = "sim-$([guid]::NewGuid().ToString('N'))"
    deviceId = $device.id
    voltage = [decimal]$voltage
    current = [decimal](10 + (Get-Random -Minimum 0 -Maximum 10))
    frequency = [decimal](49.90 + ((Get-Random -Minimum 0 -Maximum 20) / 100))
    powerFactor = [decimal]$pf
    activePower = [decimal](2 + ((Get-Random -Minimum 0 -Maximum 30) / 10))
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
  } | ConvertTo-Json
  Invoke-RestMethod -Method Post -Uri "$base/api/telemetry" -ContentType "application/json" -Body $body | Out-Null
  Start-Sleep -Milliseconds $DelayMs
}
Write-Host "Done. Open the WPF client or Swagger to inspect alerts and statistics."
