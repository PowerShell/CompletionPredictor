#
# Module manifest for module 'CompletionPredictor'
#

@{
    ModuleVersion = '0.1.0'
    GUID = 'dab36133-7065-440d-ac9a-821187afc400'
    Author = 'PowerShell'
    CompanyName = "Microsoft Corporation"
    Copyright = "Copyright (c) Microsoft Corporation."
    Description = 'Command-line intellisense based on PowerShell auto-completion'
    PowerShellVersion = '7.2'

    NestedModules = @('PowerShell.Predictor.dll')
    FunctionsToExport = @()
    CmdletsToExport = @()
    VariablesToExport = '*'
    AliasesToExport = @()
}
