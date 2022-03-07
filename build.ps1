param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

$srcDir = Join-Path $PSScriptRoot 'src'
dotnet publish $srcDir

Write-Host "`nThe module 'CompleterPredictor' is published to 'bin\CompleterPredictor'`n" -ForegroundColor Green
