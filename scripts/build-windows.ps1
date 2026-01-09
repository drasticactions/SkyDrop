<#
.SYNOPSIS
    Builds SkyDrop.Desktop for Windows x64 and arm64 using NativeAOT cross-compilation.
.DESCRIPTION
    This script builds the desktop app for both Windows architectures from a single machine.
    Requires VS 2022 C++ build tools for both x64 and ARM64 to be installed.
.NOTES
    VS Components needed:
    - "VS 2022 C++ x64/x86 build tools (Latest)"
    - "VS 2022 C++ ARM64/ARM64EC build tools (Latest)"
#>

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$Project = Join-Path $RepoRoot "src\SkyDrop.Desktop\SkyDrop.Desktop.csproj"
$ArtifactsDir = Join-Path $RepoRoot "artifacts"

Write-Host "Building SkyDrop.Desktop for Windows (x64 and arm64)..."
Write-Host "Repository root: $RepoRoot"

# Clean previous artifacts
$OutputX64 = Join-Path $ArtifactsDir "SkyDrop-Windows-x64"
$OutputArm64 = Join-Path $ArtifactsDir "SkyDrop-Windows-arm64"

if (Test-Path $OutputX64) { Remove-Item -Recurse -Force $OutputX64 }
if (Test-Path $OutputArm64) { Remove-Item -Recurse -Force $OutputArm64 }

New-Item -ItemType Directory -Force -Path $OutputX64 | Out-Null
New-Item -ItemType Directory -Force -Path $OutputArm64 | Out-Null

# Build for x64
Write-Host ""
Write-Host "=== Building for win-x64 ===" -ForegroundColor Cyan
dotnet publish $Project -c Release -r win-x64 -o $OutputX64
if ($LASTEXITCODE -ne 0) { throw "x64 build failed" }

# Build for arm64
Write-Host ""
Write-Host "=== Building for win-arm64 ===" -ForegroundColor Cyan
dotnet publish $Project -c Release -r win-arm64 -o $OutputArm64
if ($LASTEXITCODE -ne 0) { throw "arm64 build failed" }

# Verify builds
Write-Host ""
Write-Host "=== Build complete ===" -ForegroundColor Green

Write-Host ""
Write-Host "x64 output:" -ForegroundColor Yellow
Get-ChildItem $OutputX64 -Filter "*.exe" | ForEach-Object { Write-Host "  $($_.Name) - $([math]::Round($_.Length / 1MB, 2)) MB" }

Write-Host ""
Write-Host "arm64 output:" -ForegroundColor Yellow
Get-ChildItem $OutputArm64 -Filter "*.exe" | ForEach-Object { Write-Host "  $($_.Name) - $([math]::Round($_.Length / 1MB, 2)) MB" }

Write-Host ""
Write-Host "Artifacts location: $ArtifactsDir"
