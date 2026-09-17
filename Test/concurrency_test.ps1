
$ErrorActionPreference = "Continue"

$baseUrl = "http://localhost:5000"

Write-Host "Obteniendo tokens para comprador1 y comprador2..." -ForegroundColor Cyan


$login1 = @{ email = "comprador1@test.com"; password = "123456" } | ConvertTo-Json
$res1 = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $login1 -ContentType "application/json"
$token1 = $res1.token


$login2 = @{ email = "comprador2@test.com"; password = "123456" } | ConvertTo-Json
$res2 = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $login2 -ContentType "application/json"
$token2 = $res2.token


$subasta = Invoke-RestMethod -Uri "$baseUrl/api/subastas/1" -Method Get
$montoOferta = [decimal]$subasta.precioActual + [decimal]$subasta.incrementoMinimo + 5000
$payload = (@{ amount = $montoOferta } | ConvertTo-Json)

Write-Host "Subasta 1 - Precio actual: $($subasta.precioActual). Lanzando 2 ofertas simultaneas de $$montoOferta..." -ForegroundColor Yellow

$url = "$baseUrl/api/auctions/1/bids"

$job1 = Start-Job -ScriptBlock {
    param($url, $token, $payload)
    try {
        $headers = @{ 
            Authorization  = "Bearer $token"
            "Content-Type" = "application/json"
        }
        $resp = Invoke-WebRequest -Uri $url -Method Post -Headers $headers -Body $payload -UseBasicParsing
        return "HTTP $($resp.StatusCode) - $($resp.Content)"
    }
    catch {
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            $code = [int]$_.Exception.Response.StatusCode
            return "HTTP $($code) - $($body)"
        }
        else {
            return "Error: $($_.Exception.Message)"
        }
    }
} -ArgumentList $url, $token1, $payload

$job2 = Start-Job -ScriptBlock {
    param($url, $token, $payload)
    try {
        $headers = @{ 
            Authorization  = "Bearer $token"
            "Content-Type" = "application/json"
        }
        $resp = Invoke-WebRequest -Uri $url -Method Post -Headers $headers -Body $payload -UseBasicParsing
        return "HTTP $($resp.StatusCode) - $($resp.Content)"
    }
    catch {
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            $code = [int]$_.Exception.Response.StatusCode
            return "HTTP $($code) - $($body)"
        }
        else {
            return "Error: $($_.Exception.Message)"
        }
    }
} -ArgumentList $url, $token2, $payload

$result1 = Receive-Job -Job $job1 -Wait
$result2 = Receive-Job -Job $job2 -Wait

Write-Host "`n--- RESULTADOS DE LA PRUEBA DE CONCURRENCIA ---" -ForegroundColor Green
Write-Host "Respuesta Peticion 1: $result1"
Write-Host "Respuesta Peticion 2: $result2"
Write-Host "`nResultado esperado: Una peticion 200 (OK) y otra 409 (Conflict)." -ForegroundColor Cyan
