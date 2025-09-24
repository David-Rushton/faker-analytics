[CmdletBinding()]
param()

$errors = 0

# We need an .env file.
$envFile = "$PSScriptRoot/../.env"
if (-not (Test-Path $envFile)) {
    Write-Warning "Required .env file not found.  Creating."
    New-Item $envFile -ItemType File -Value "GeminiClient:ApiKey=<Your-Key-Here>"

    $errors++
}

# It must contain a Gemini API key.
$envFileContent = Get-Content -Path $envFile -Raw | Select-String "GeminiClient:ApiKey=[A-z0-9]{32,}"
if ($null -eq $envFileContent) {
    Write-Warning "Required Gemini API key is missing from .env file."
    Write-Warning " - Log into Google Cloud https://cloud.google.com/?hl=en"
    Write-Warning " - Create a key"
    Write-Warning " - Add it to .env: GeminiClient:ApiKey=<Your-Key-Here>"

    $errors++
}

# We need docker.
try {
    docker info *>$null
} catch {
    Write-Warning "Docker is required"
    Write-Warning " - Visit: https://docs.docker.com/engine/install/"
    $errors++
}


# Wrap up.
if ($errors -eq 0) {
    Write-Host "Your local environment is ready to go"
}
