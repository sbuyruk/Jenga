#Requires -Version 5.1
param(
    [string]$IisWebRoot = "\\10.150.8.25\c$\inetpub\wwwroot\Jenga",
    [string]$PublishOutput = "$PSScriptRoot\Jenga.BlazorUI\bin\Release\net10.0\publish\jenga_publish"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$offlineFile  = Join-Path $IisWebRoot "app_offline.htm"
$templateFile = Join-Path $PSScriptRoot "Jenga.BlazorUI\app_offline.template.htm"
$projectFile  = Join-Path $PSScriptRoot "Jenga.BlazorUI\Jenga.BlazorUI.csproj"

# 1. Bakim moduna al (503, auth challenge yok)
Write-Host "Bakim modu aktiflesiriliyor..." -ForegroundColor Cyan
Copy-Item -Path $templateFile -Destination $offlineFile -Force
Write-Host "  app_offline.htm birakildi -> IIS tum isteklere 503 donecek"

try {
    # 2. Build & Publish
    Write-Host "`ndotnet publish baslatiliyor..." -ForegroundColor Cyan
    dotnet publish $projectFile -c Release -o $PublishOutput --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish basarisiz oldu (exit $LASTEXITCODE)" }

    # 3. Dosyalari hedefe kopyala
    Write-Host "`nDosyalar $IisWebRoot dizinine kopyalaniyor..." -ForegroundColor Cyan
    robocopy $PublishOutput $IisWebRoot /E /XF app_offline.htm /NFL /NDL /NJH /NP
    if ($LASTEXITCODE -gt 7) { throw "robocopy basarisiz oldu (exit $LASTEXITCODE)" }

    Write-Host "`nPublish tamamlandi." -ForegroundColor Green
}
finally {
    # 4. Bakim modunu kapat (hata olsa bile)
    Write-Host "`nBakim modu kaldiriliyor..." -ForegroundColor Cyan
    if (Test-Path $offlineFile) {
        Remove-Item $offlineFile -Force
        Write-Host "  app_offline.htm silindi -> Site tekrar canlida"
    }
}
