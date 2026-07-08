<#
.SYNOPSIS
    Find duplicate files (and size hot-spots) to help organize a Windows laptop.

.DESCRIPTION
    REPORT-ONLY by design - it never deletes or moves anything. It finds true
    duplicates (identical content) efficiently: it groups by file size first and
    only hashes files whose sizes collide, so a huge tree scans quickly.

    Outputs:
      * a console summary (duplicate groups, wasted space, largest files),
      * an optional CSV of every duplicate file for review.

    To actually delete/move, review the CSV, then use the commented -WhatIf
    examples at the bottom - deliberately left manual so nothing is destructive.

.PARAMETER Path
    Root folder to scan. Default: current directory.

.PARAMETER MinSizeKB
    Ignore files smaller than this (KB). Default 10 - tiny files rarely matter
    and dominate the count. Set 0 to include everything.

.PARAMETER OutputCsv
    Optional path to write the duplicate report as CSV.

.PARAMETER TopLargest
    How many biggest files to list. Default 20.

.PARAMETER IncludeHidden
    Include hidden/system files. Off by default.

.EXAMPLE
    .\Organize-Laptop.ps1 -Path "C:\Users\Jeff" -OutputCsv "$HOME\Desktop\dupes.csv"

.EXAMPLE
    .\Organize-Laptop.ps1 -Path "D:\Consulting" -MinSizeKB 100 -TopLargest 40
#>
[CmdletBinding()]
param(
    [string]$Path = ".",
    [int]$MinSizeKB = 10,
    [string]$OutputCsv,
    [int]$TopLargest = 20,
    [switch]$IncludeHidden
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $Path).Path
Write-Host "Scanning $root ..." -ForegroundColor Cyan

# 1) Enumerate files (skip reparse points to avoid symlink loops).
$gciParams = @{ LiteralPath = $root; File = $true; Recurse = $true; ErrorAction = 'SilentlyContinue' }
if ($IncludeHidden) { $gciParams.Force = $true }
$minBytes = $MinSizeKB * 1KB

$files = Get-ChildItem @gciParams |
    Where-Object { -not ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -and $_.Length -ge $minBytes }

Write-Host ("Found {0:N0} files >= {1} KB." -f $files.Count, $MinSizeKB) -ForegroundColor Cyan

# 2) Only hash within same-size groups (cheap pre-filter).
$sizeGroups = $files | Group-Object Length | Where-Object { $_.Count -gt 1 }

$dupeRecords = New-Object System.Collections.Generic.List[object]
$hashed = 0
foreach ($sg in $sizeGroups) {
    $byHash = @{}
    foreach ($f in $sg.Group) {
        try   { $h = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash }
        catch { continue }
        $hashed++
        if (-not $byHash.ContainsKey($h)) { $byHash[$h] = New-Object System.Collections.Generic.List[object] }
        $byHash[$h].Add($f)
    }
    foreach ($h in $byHash.Keys) {
        $grp = $byHash[$h]
        if ($grp.Count -gt 1) {
            # Keep the oldest as the suggested original; the rest are redundant copies.
            $ordered = $grp | Sort-Object LastWriteTime
            for ($i = 0; $i -lt $ordered.Count; $i++) {
                $dupeRecords.Add([pscustomobject]@{
                    Hash        = $h.Substring(0,12)
                    SizeMB      = [math]::Round($ordered[$i].Length / 1MB, 3)
                    Role        = if ($i -eq 0) { 'KEEP (oldest)' } else { 'redundant copy' }
                    LastWrite   = $ordered[$i].LastWriteTime
                    FullName    = $ordered[$i].FullName
                })
            }
        }
    }
}

# 3) Summaries.
$groups        = ($dupeRecords | Group-Object Hash).Count
$redundant     = $dupeRecords | Where-Object Role -eq 'redundant copy'
$wastedMB      = [math]::Round(($redundant | Measure-Object SizeMB -Sum).Sum, 1)

Write-Host ""
Write-Host "===== DUPLICATE SUMMARY =====" -ForegroundColor Yellow
Write-Host ("Hashed {0:N0} same-size files" -f $hashed)
Write-Host ("Duplicate groups : {0:N0}" -f $groups)
Write-Host ("Redundant copies : {0:N0}" -f $redundant.Count)
Write-Host ("Reclaimable space: {0:N1} MB" -f $wastedMB) -ForegroundColor Green

Write-Host ""
Write-Host "===== TOP $TopLargest LARGEST FILES =====" -ForegroundColor Yellow
$files | Sort-Object Length -Descending | Select-Object -First $TopLargest |
    Select-Object @{n='SizeMB';e={[math]::Round($_.Length/1MB,1)}}, LastWriteTime, FullName |
    Format-Table -AutoSize

Write-Host "===== SPACE BY EXTENSION (top 15) =====" -ForegroundColor Yellow
$files | Group-Object Extension |
    Select-Object Name, Count, @{n='TotalMB';e={[math]::Round((($_.Group|Measure-Object Length -Sum).Sum)/1MB,1)}} |
    Sort-Object TotalMB -Descending | Select-Object -First 15 | Format-Table -AutoSize

if ($OutputCsv) {
    $dupeRecords | Sort-Object Hash, Role | Export-Csv -LiteralPath $OutputCsv -NoTypeInformation -Encoding UTF8
    Write-Host "Duplicate detail written to $OutputCsv" -ForegroundColor Cyan
}

<#  ---- DESTRUCTIVE ACTIONS (manual, opt-in) --------------------------------
    Review the CSV first. Then, to preview deleting the redundant copies:

        Import-Csv .\dupes.csv | Where-Object Role -eq 'redundant copy' |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -WhatIf }

    Remove -WhatIf to actually delete. Or move them to a review folder instead:

        $bin = "$HOME\Desktop\_DupeReview"; New-Item $bin -ItemType Directory -Force | Out-Null
        Import-Csv .\dupes.csv | Where-Object Role -eq 'redundant copy' |
            ForEach-Object { Move-Item -LiteralPath $_.FullName -Destination $bin -WhatIf }
    -------------------------------------------------------------------------- #>
