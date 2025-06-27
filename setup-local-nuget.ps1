# Setup script for local NuGet feed
$localNuGetPath = "D:\LocalNuGet"

# Create local NuGet directory if it doesn't exist
if (!(Test-Path $localNuGetPath)) {
    New-Item -ItemType Directory -Path $localNuGetPath -Force
    Write-Host "Created local NuGet directory at $localNuGetPath" -ForegroundColor Green
}

# Add local source to NuGet (if not already added)
$sourceName = "Local"
$existingSource = dotnet nuget list source | Select-String $sourceName

if (!$existingSource) {
    dotnet nuget add source $localNuGetPath --name $sourceName
    Write-Host "Added local NuGet source '$sourceName' pointing to $localNuGetPath" -ForegroundColor Green
} else {
    Write-Host "Local NuGet source '$sourceName' already exists" -ForegroundColor Yellow
}

# List all sources
Write-Host "`nCurrent NuGet sources:" -ForegroundColor Cyan
dotnet nuget list source
