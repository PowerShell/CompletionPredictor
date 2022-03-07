# CompletionPredictor

`CompletionPredictor` is a PowerShell module that provides the auto-completion based [predictive intellisense](https://devblogs.microsoft.com/powershell/announcing-psreadline-2-1-with-predictive-intellisense/) in the PowerShell command line.
It's a predictor plugin built on top of the [Subsystem Plugin Model](https://docs.microsoft.com/powershell/scripting/learn/experimental-features#pssubsystempluginmodel) available with PowerShell 7.2, and it requires the [PSReadLine 2.2.2](https://www.powershellgallery.com/packages/PSReadLine/2.2.2) or above versions to display the suggestions.

## Build

Make sure the [latest .NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) is installed and available in your `PATH` environment variable.
Run `.\build.ps1` from PowerShell to build the project. The module will be published to `.\bin\CompletionPredictor` by a successful build.

## Use the predictor

> NOTE: Make sure you use PowerShell 7.2 with PSReadLine 2.2.2.

1. Import the module by `Import-Module .\bin\CompletionPredictor`.
2. Enable prediction from the plugin source for PSReadLine: `Set-PSReadLineOption -PredictionSource Plugin`.
3. Switch between the `Inline` and `List` prediction views, by pressing <kbd>F2</kbd>.
