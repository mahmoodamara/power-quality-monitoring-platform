$ErrorActionPreference = "Stop"
Write-Host "Starting SQL Server, Redis and ASP.NET Core API..."
docker compose up --build -d
Write-Host "Waiting for API..."
for ($i=0; $i -lt 60; $i++) {
  try {
    $r = Invoke-RestMethod -Uri "http://localhost:8080/health/live" -TimeoutSec 2
    if ($r.status -eq "healthy") { Write-Host "API is ready: http://localhost:8080/swagger"; exit 0 }
  } catch {}
  Start-Sleep -Seconds 2
}
Write-Error "API did not become ready. Run: docker compose logs api"
