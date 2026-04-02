$baseUrl = "http://localhost:5166"
$jobCount = 100
$successCount = 0
$failCount = 0

Write-Host "Starting load test - sending $jobCount jobs..." -ForegroundColor Cyan

$startTime = Get-Date

for ($i = 1; $i -le $jobCount; $i++) {
    $body = @{
        type = "SendEmail"
        payload = "{`"to`": `"user$i@gmail.com`", `"subject`": `"Test $i`"}"
        priority = Get-Random -Minimum 1 -Maximum 10
        maxRetries = 3
        scheduledAt = $null
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/jobs" -Method POST -Body $body -ContentType "application/json"
        $successCount++
        if ($i % 10 -eq 0) {
            Write-Host "Sent $i/$jobCount jobs..." -ForegroundColor Green
        }
    }
    catch {
        $failCount++
    }
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "`n--- Load Test Results ---" -ForegroundColor Cyan
Write-Host "Total jobs sent: $jobCount" -ForegroundColor White
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Duration: $([math]::Round($duration, 2)) seconds" -ForegroundColor White
Write-Host "Throughput: $([math]::Round($successCount / $duration, 2)) jobs/sec" -ForegroundColor Yellow

Write-Host "`nChecking metrics..." -ForegroundColor Cyan
Start-Sleep -Seconds 2
$metrics = Invoke-RestMethod -Uri "$baseUrl/api/jobs/metrics" -Method GET
Write-Host "Pending: $($metrics.pendingJobs)" -ForegroundColor White
Write-Host "Running: $($metrics.runningJobs)" -ForegroundColor White
Write-Host "Completed: $($metrics.completedJobs)" -ForegroundColor White
Write-Host "Total: $($metrics.totalJobs)" -ForegroundColor White