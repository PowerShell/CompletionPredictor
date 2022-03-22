[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(ParameterSetName = 'Build')]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [Parameter(ParameterSetName = 'Bootstrap')]
    [switch] $Bootstrap
)

Import-Module "$PSScriptRoot/tools/helper.psm1"

if ($Bootstrap) {
    Write-Log "Validate and install missing prerequisits for building ..."
    Install-Dotnet
    return
}

$srcDir = Join-Path $PSScriptRoot 'src'
dotnet publish -c $Configuration $srcDir

Write-Host "`nThe module 'CompleterPredictor' is published to 'bin\CompleterPredictor'`n" -ForegroundColor Green
