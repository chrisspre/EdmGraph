param (
    [string]$Path = "."
)

# Get all .cs files recursively
Get-ChildItem -Path $Path -Recurse -Filter *.cs | ForEach-Object {
    $file = $_.FullName
    Write-Host "Processing $file"

    # Read file content
    $content = Get-Content -Path $file -Raw

    # Remove trailing whitespace
    $content = [Regex]::Replace($content, '\s+$', '', 'Multiline')

    # Collapse multiple empty lines into one
    $content = [Regex]::Replace($content, '(\r?\n){3,}', "`r`n`r`n")

    # Write cleaned content back to file
    Set-Content -Path $file -Value $content
}

Write-Host "Cleanup complete."