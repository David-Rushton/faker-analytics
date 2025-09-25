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
$geminiApiKey = Get-Content -Path $envFile -Raw | Select-String "GeminiClient:ApiKey=[A-z0-9]{32,}"
if ($null -eq $geminiApiKey) {
    Write-Warning "Required Gemini API key is missing from .env file."
    Write-Warning " - Log into Google Cloud https://cloud.google.com/?hl=en"
    Write-Warning " - Create a key"
    Write-Warning " - Add it to .env: GeminiClient:ApiKey=<Your-Key-Here>"

    $errors++
}

# We need Docker.
try {
    docker info *>$null
} catch {
    Write-Warning "Docker is required"
    Write-Warning " - Visit: https://docs.docker.com/engine/install/"
    $errors++
}

# We need .Net.
try {
    dotnet *>$null

    $sdkFound = dotnet --list-sdks | Select-String 10\.0\.100-rc
    if ($null -eq $sdkFound) {
        Write-Warning ".Net 10.0.100 or higher is required"
        Write-Warning " - Visit: https://dotnet.microsoft.com/en-us/download"

        $errors++
    }

    # Aspire needs our Gemini key.
    if (-not ($null -eq $geminiApiKey)) {
        try {
            Push-Location "$PSScriptRoot/../src/dotnet/faker-apphost/"
            dotnet user-secrets set "GeminiClient:ApiKey" $geminiApiKey.Matches[0].Value.Replace("GeminiClient:ApiKey", "") | Out-Null
            Pop-Location
        }
        catch {
            Write-Warning "Cannot write Gemini API to dotnet secrets because $_"
            $errors++
        }
    }

} catch {
    Write-Warning ".Net is required"
    Write-Warning " - Visit: https://dotnet.microsoft.com/en-us/download"

    $errors++
}

# Wrap up.
if ($errors -eq 0) {
    Write-Host "✅ You are ready to go!"
} else {
    Write-Error "❌ Please fix the warnings above"
}
