#requires -Version 7.0
<#
.SYNOPSIS
KML parsel sınırlarını dosya adındaki taşınmaz ID'lerine göre önizler veya aktarır.
.DESCRIPTION
Varsayılan mod önizlemedir. Bağlantı verilmezse yalnızca dosyalar doğrulanır.
SQL bağlantısı için -BaglantiDizesi veya TBYS_SQL_CONNECTION_STRING kullanın.
-Uygula eşleşen satırları tek işlemde günceller. Eşleşmeyen ID'ler raporlanır ve
atlanır. Geçersiz dosya veya aynı veritabanı kaydının birden fazla dosyada
yer alması aktarımı durdurur. Tireli aralıklar iki uç dahil genişletilir.
Virgül ve "ve" ile ayrılan ID'ler veya aralıklar ayrı ayrı işlenir.
Önce 2026-11-19-TBYS-ParselGeometrisi.sql dosyasını hedef veritabanında çalıştırın.
.EXAMPLE
./Import-TBYS-ParselGeometrisi.ps1 -KmlKlasoru 'C:\Veri\KML Dosyalar Tümü'
.EXAMPLE
./Import-TBYS-ParselGeometrisi.ps1 -KmlKlasoru 'C:\Veri\KML Dosyalar Tümü' -Uygula
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $KmlKlasoru,

    [string] $BaglantiDizesi = $env:TBYS_SQL_CONNECTION_STRING,

    [switch] $Uygula
)

$ErrorActionPreference = 'Stop'
$kmlNs = [System.Xml.Linq.XNamespace]::Get('http://www.opengis.net/kml/2.2')
$invariant = [System.Globalization.CultureInfo]::InvariantCulture
$floatStyle = [System.Globalization.NumberStyles]::Float

function Read-KmlRing([System.Xml.Linq.XElement] $Boundary, [bool] $Outer) {
    $linearRing = $Boundary.Element($kmlNs.GetName('LinearRing'))
    $coordinates = ${linearRing}?.Element($kmlNs.GetName('coordinates'))
    if ([string]::IsNullOrWhiteSpace(${coordinates}?.Value)) {
        throw 'Poligon halkasının koordinatları eksik.'
    }

    $positions = [System.Collections.Generic.List[object]]::new()
    foreach ($tuple in ($coordinates.Value.Trim() -split '\s+')) {
        $parts = $tuple.Split(',')
        $lon = $lat = 0.0
        if ($parts.Length -lt 2 -or $parts.Length -gt 3 -or
            -not [double]::TryParse($parts[0], $floatStyle, $invariant, [ref] $lon) -or
            -not [double]::TryParse($parts[1], $floatStyle, $invariant, [ref] $lat) -or
            -not [double]::IsFinite($lon) -or -not [double]::IsFinite($lat) -or
            [Math]::Abs($lon) -gt 180 -or [Math]::Abs($lat) -gt 90) {
            throw "Geçersiz boylam veya enlem: $tuple"
        }
        $positions.Add(@($lon, $lat))
    }

    $first = $positions[0]
    $last = $positions[$positions.Count - 1]
    if ($first[0] -ne $last[0] -or $first[1] -ne $last[1]) {
        $positions.Add($first)
    }

    $area = 0.0
    for ($i = 1; $i -lt $positions.Count - 1; $i++) {
        $left = $positions[$i]
        $right = $positions[$i + 1]
        $area += ($left[0] - $first[0]) * ($right[1] - $first[1]) -
            ($right[0] - $first[0]) * ($left[1] - $first[1])
    }
    if ($positions.Count -lt 4 -or $area -eq 0) {
        throw 'Poligon halkası alan oluşturmuyor.'
    }
    if (($area -gt 0) -ne $Outer) {
        $positions.Reverse()
    }
    return ,$positions.ToArray()
}

function Convert-KmlGeometry([string] $Path) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try { $document = [System.Xml.Linq.XDocument]::Load($reader) }
    finally { $reader.Dispose() }

    if ($document.Root.Name -ne $kmlNs.GetName('kml')) {
        throw 'KML 2.2 kök öğesi bulunamadı.'
    }
    $polygons = [System.Collections.Generic.List[object]]::new()
    foreach ($polygon in $document.Descendants($kmlNs.GetName('Polygon'))) {
        $outer = $polygon.Element($kmlNs.GetName('outerBoundaryIs'))
        if ($null -eq $outer) {
            throw 'Poligonun dış halkası eksik.'
        }
        $rings = [System.Collections.Generic.List[object]]::new()
        $rings.Add((Read-KmlRing $outer $true))
        foreach ($hole in $polygon.Elements($kmlNs.GetName('innerBoundaryIs'))) {
            $rings.Add((Read-KmlRing $hole $false))
        }
        $polygons.Add($rings.ToArray())
    }
    if ($polygons.Count -eq 0) {
        throw 'KML dosyasında Polygon bulunamadı.'
    }
    if ($polygons.Count -eq 1) {
        $geometry = [ordered]@{ type = 'Polygon'; coordinates = $polygons[0] }
    } else {
        $geometry = [ordered]@{ type = 'MultiPolygon'; coordinates = $polygons.ToArray() }
    }
    return ConvertTo-Json -InputObject $geometry -Depth 10 -Compress
}

if ($Uygula -and [string]::IsNullOrWhiteSpace($BaglantiDizesi)) {
    throw '-Uygula için hedef SQL bağlantısı gerekli.'
}
$files = @(Get-ChildItem -LiteralPath $KmlKlasoru -File -Filter '*.kml' | Sort-Object Name)
if ($files.Count -eq 0) {
    throw 'Klasörde KML dosyası bulunamadı.'
}

$stage = [System.Data.DataTable]::new()
[void] $stage.Columns.Add('TasinmazId', [int])
[void] $stage.Columns.Add('DosyaAdi', [string])
[void] $stage.Columns.Add('GeoJson', [string])

$issues = [System.Collections.Generic.List[string]]::new()
# Örnekler: "2-Mersin", "109 İstanbul", "19, 199 ve 200-İzmir", "532-560-Ankara".
$idOrRange = '[0-9]+(?:\s*-\s*[0-9]+)?'
$fileNamePattern = "^\s*(?<ids>$idOrRange(?:\s*(?:,|ve\s+)\s*$idOrRange)*)\s*(?:-\s*|\s+)\p{L}"
foreach ($file in $files) {
    try {
        if ($file.BaseName -notmatch $fileNamePattern) {
            throw 'Dosya adının başında taşınmaz ID listesi bulunamadı.'
        }
        $ids = @($Matches.ids -split '\s*(?:,|ve)\s*' | ForEach-Object {
            $bounds = $_ -split '\s*-\s*'
            $firstId = [int]::Parse($bounds[0], $invariant)
            $lastId = [int]::Parse($bounds[-1], $invariant)
            if ($firstId -le 0 -or $lastId -le 0) { throw 'Taşınmaz ID pozitif olmalı.' }
            $firstId..$lastId
        } | Sort-Object -Unique)
        $geoJson = Convert-KmlGeometry $file.FullName
        foreach ($id in $ids) {
            [void] $stage.Rows.Add($id, $file.Name, $geoJson)
        }
    } catch {
        $issues.Add("$($file.Name) - $($_.Exception.Message)")
    }
}

if ($issues.Count -gt 0) {
    $issues | ForEach-Object { Write-Warning $_ }
    throw 'Dosya önizlemesi hatalı. Aktarım yapılmadı.'
}

$idGroups = @($stage.Rows | Group-Object TasinmazId)
Write-Host "$($files.Count) KML dosyasından $($idGroups.Count) farklı taşınmaz ID'si ve $($stage.Rows.Count) dosya-ID eşleşmesi çıkarıldı."
foreach ($duplicate in ($idGroups | Where-Object Count -gt 1)) {
    Write-Warning "ID $($duplicate.Name) birden fazla dosyada: $($duplicate.Group.DosyaAdi -join ' | ')"
}
if ([string]::IsNullOrWhiteSpace($BaglantiDizesi)) {
    $stage.Rows | Sort-Object TasinmazId | Format-Table TasinmazId, DosyaAdi
    Write-Host 'Dosya önizlemesi tamamlandı. Veritabanı eşleşmesi için SQL bağlantısı verin.'
    return
}

$connection = [System.Data.SqlClient.SqlConnection]::new($BaglantiDizesi)
try {
    $connection.Open()
    Write-Host "Hedef: $($connection.DataSource) / $($connection.Database)"
    $transaction = if ($Uygula) { $connection.BeginTransaction([System.Data.IsolationLevel]::Serializable) }
    else { $connection.BeginTransaction() }
    try {
        $command = $connection.CreateCommand()
        try {
            $command.Transaction = $transaction
            $command.CommandText = @'
IF COL_LENGTH(N'dbo.Tasinmaz_Table', N'ParselGeometrisiGeoJson') IS NULL
    THROW 51000, N'ParselGeometrisiGeoJson alanı bulunamadı. Önce şema SQL dosyasını çalıştırın.', 1;
CREATE TABLE #ParselAktarim
(
    TasinmazId INT NOT NULL,
    DosyaAdi NVARCHAR(260) NOT NULL,
    GeoJson NVARCHAR(MAX) NOT NULL,
    PRIMARY KEY (TasinmazId, DosyaAdi)
);
'@
            [void] $command.ExecuteNonQuery()
            $bulkCopy = [System.Data.SqlClient.SqlBulkCopy]::new(
                $connection, [System.Data.SqlClient.SqlBulkCopyOptions]::Default, $transaction)
            try {
                $bulkCopy.DestinationTableName = '#ParselAktarim'
                $bulkCopy.WriteToServer($stage)
            } finally { $bulkCopy.Dispose() }

            $command.CommandText = @'
SELECT s.TasinmazId, s.DosyaAdi, t.EmlakSicilNo,
    CASE WHEN t.Id IS NULL THEN N'Eşleşmedi'
         WHEN COUNT(*) OVER (PARTITION BY s.TasinmazId) > 1 THEN N'Çakışma'
         WHEN CONVERT(VARBINARY(MAX), t.ParselGeometrisiGeoJson) = CONVERT(VARBINARY(MAX), s.GeoJson)
             THEN N'Aynı'
         WHEN t.ParselGeometrisiGeoJson IS NULL THEN N'Eklenecek'
         ELSE N'Güncellenecek'
    END AS Durum
FROM #ParselAktarim s
LEFT JOIN dbo.Tasinmaz_Table t ON t.Id = s.TasinmazId
ORDER BY s.TasinmazId, s.DosyaAdi;
'@
            $reader = $command.ExecuteReader()
            $report = [System.Data.DataTable]::new()
            try { $report.Load($reader) }
            finally { $reader.Dispose() }
            $report.Rows | Format-Table TasinmazId, EmlakSicilNo, Durum, DosyaAdi | Out-Host
            $report.Rows | Group-Object Durum | Format-Table Name, Count | Out-Host

            if ($Uygula) {
                if ($report.Select("Durum = 'Çakışma'").Length -gt 0) {
                    throw 'Aynı taşınmaz birden fazla dosyada yer alıyor. Aktarım yapılmadı.'
                }
                $command.CommandText = @'
UPDATE t
SET t.ParselGeometrisiGeoJson = s.GeoJson
FROM dbo.Tasinmaz_Table t
JOIN #ParselAktarim s ON s.TasinmazId = t.Id
WHERE t.ParselGeometrisiGeoJson IS NULL
    OR CONVERT(VARBINARY(MAX), t.ParselGeometrisiGeoJson) <> CONVERT(VARBINARY(MAX), s.GeoJson);
SELECT @@ROWCOUNT;
'@
                $updated = $command.ExecuteScalar()
                $transaction.Commit()
                Write-Host "$updated taşınmazın parsel geometrisi yazıldı."
            } else {
                $transaction.Rollback()
                Write-Host 'Veritabanı önizlemesi tamamlandı. Yazmak için -Uygula kullanın.'
            }
        } finally { $command.Dispose() }
    } finally { $transaction.Dispose() }
} finally { $connection.Dispose() }
