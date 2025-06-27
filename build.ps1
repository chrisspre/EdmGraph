# Build and Package Script
param(
    [string]$Configuration = "Release",
    [string]$Version = "",  # Empty means auto-increment
    [switch]$PublishToLocal = $false,
    [switch]$Clean = $false,
    [switch]$IncrementMinor = $false,
    [switch]$IncrementPatch = $false,
    [switch]$CleanLocalNuGet = $false
)

$ErrorActionPreference = "Stop"

# Function to read current version from Directory.Build.props
function Get-CurrentVersion {
    $propsFile = "Directory.Build.props"
    if (Test-Path $propsFile) {
        $xml = [xml](Get-Content $propsFile)
        $versionPrefix = $xml.Project.PropertyGroup.VersionPrefix
        return $versionPrefix
    }
    return "1.0.0"
}

# Function to increment version
function Update-Version {
    param([string]$currentVersion, [bool]$incrementMinor, [bool]$incrementPatch)
    
    $parts = $currentVersion.Split('.')
    $major = [int]$parts[0]
    $minor = [int]$parts[1] 
    $patch = [int]$parts[2]
    
    if ($incrementMinor) {
        $minor++
        $patch = 0
    } elseif ($incrementPatch) {
        $patch++
    }
    
    return "$major.$minor.$patch"
}

# Function to update Directory.Build.props with new version
function Set-VersionInProps {
    param([string]$newVersion)
    
    $propsFile = "Directory.Build.props"
    $content = Get-Content $propsFile -Raw
    $content = $content -replace '<VersionPrefix>[\d\.]+</VersionPrefix>', "<VersionPrefix>$newVersion</VersionPrefix>"
    Set-Content $propsFile $content -NoNewline
}

# Determine version to use
if ([string]::IsNullOrEmpty($Version)) {
    $currentVersion = Get-CurrentVersion
    
    if ($IncrementMinor -or $IncrementPatch) {
        $Version = Update-Version $currentVersion $IncrementMinor $IncrementPatch
        Write-Host "Auto-incrementing version from $currentVersion to $Version" -ForegroundColor Yellow
        Set-VersionInProps $Version
    } else {
        $Version = $currentVersion
    }
} else {
    # Manual version specified, update props file
    Set-VersionInProps $Version
}

Write-Host "=== Building and Packaging EdmGraph Libraries ===" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration
    Remove-Item -Path "artifacts" -Recurse -Force -ErrorAction SilentlyContinue
}

# Create artifacts directory
New-Item -ItemType Directory -Path "artifacts\packages" -Force | Out-Null

# Clean artifacts packages folder to ensure clean build
Write-Host "Cleaning artifacts packages..." -ForegroundColor Yellow
Remove-Item "artifacts\packages\*.nupkg" -Force -ErrorAction SilentlyContinue

# Set version for build
$env:VersionPrefix = $Version

# Build order (dependencies first)
$projects = @(
    "src/MermaidGen.Net/MermaidGen.Net.csproj",
    "src/LabeledPropertyGraph.Net/LabeledPropertyGraph.Net.csproj", 
    "src/EdmGraph/EdmGraph.csproj"
)

Write-Host "`nBuilding projects in dependency order..." -ForegroundColor Yellow

foreach ($project in $projects) {
    Write-Host "Building $project..." -ForegroundColor Cyan
    dotnet build $project --configuration $Configuration --no-restore -p:GeneratePackageOnBuild=false
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for $project"
    }
}

Write-Host "`nPackaging projects..." -ForegroundColor Yellow

foreach ($project in $projects) {
    Write-Host "Packaging $project..." -ForegroundColor Cyan
    dotnet pack $project --configuration $Configuration --no-build --output "artifacts\packages"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Pack failed for $project"
    }
}

# Publish to local NuGet if requested
if ($PublishToLocal) {
    Write-Host "`nPublishing to local NuGet..." -ForegroundColor Yellow
    
    $localNuGetPath = "D:\LocalNuGet"
    
    # Ensure local NuGet directory exists
    if (!(Test-Path $localNuGetPath)) {
        New-Item -ItemType Directory -Path $localNuGetPath -Force
    }
    
    # Clean local NuGet if requested
    if ($CleanLocalNuGet) {
        Write-Host "Cleaning local NuGet packages..." -ForegroundColor Yellow
        $packageNames = @("MermaidGen.Net", "LabeledPropertyGraph.Net", "EdmGraph")
        foreach ($packageName in $packageNames) {
            Get-ChildItem "$localNuGetPath\$packageName.*.nupkg" -ErrorAction SilentlyContinue | Remove-Item -Force
            Write-Host "  - Removed old $packageName packages" -ForegroundColor Gray
        }
    }
    
    # Copy packages to local NuGet
    Get-ChildItem "artifacts\packages\*.nupkg" | ForEach-Object {
        Write-Host "Publishing $($_.Name)..." -ForegroundColor Cyan
        Copy-Item $_.FullName $localNuGetPath -Force
    }
    
    Write-Host "Packages published to $localNuGetPath" -ForegroundColor Green
}

Write-Host "`n=== Build Complete ===" -ForegroundColor Green
Write-Host "Packages created in: artifacts\packages" -ForegroundColor Cyan

# List created packages
Write-Host "`nCreated packages:" -ForegroundColor Yellow
Get-ChildItem "artifacts\packages\*.nupkg" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}
