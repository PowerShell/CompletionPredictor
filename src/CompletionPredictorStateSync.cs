using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Predictor;

using System.Management.Automation;

public partial class CompletionPredictor
{
    private readonly HashSet<string> _loadedModules = new(StringComparer.Ordinal);
    private readonly List<PSModuleInfo> _modulesToImport = new();

    private readonly HashSet<string> _builtInVariables = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<PSVariable> _varsToSet = new();
    private Dictionary<string, int> _userVariables = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, int> _newUserVars = new(StringComparer.OrdinalIgnoreCase);

    private long _lastHistoryId = -1;


    private static readonly System.Diagnostics.Stopwatch _watch = new();
    private static readonly CmdletInfo s_getHistoryCommand = new("Get-History", typeof(GetHistoryCommand));
    private static readonly CmdletInfo s_getModuleCommand = new("Get-Module", typeof(GetModuleCommand));
    private static readonly CmdletInfo s_importModuleCommand = new("Import-Module", typeof(ImportModuleCommand));

    private void PopulateInitialState()
    {
        PSObject value = _runspace.SessionStateProxy.InvokeProvider.Item.Get(@"Variable:\")[0];
        var builtInVars = (IEnumerable<PSVariable>)value.BaseObject;
        foreach (PSVariable variable in builtInVars)
        {
            _builtInVariables.Add(variable.Name);
        }

        using var ps = PowerShell.Create();
        ps.Runspace = _runspace;
        Collection<PSModuleInfo> modules = ps.AddCommand(s_getModuleCommand).InvokeAndCleanup<PSModuleInfo>();
        foreach (PSModuleInfo module in modules)
        {
            _loadedModules.Add(module.Path);
        }
    }

    /// <summary>
    /// Sync Runspace states between the default Runspace and predictor Runspace.
    /// </summary>
    private void SyncRunspaceState(object? sender, RunspaceAvailabilityEventArgs e)
    {
        if (sender is null || e.RunspaceAvailability != RunspaceAvailability.Available)
        {
            return;
        }

        _watch.Restart();

        // It's safe to get states of the PowerShell Runspace now because it's available and this event
        // is handled synchronously.
        // We may want to invoke command or script here, and we have to unregister ourself before doing
        // that, because the invocation would change the availability of the Runspace, which will cause
        // the 'AvailabilityChanged' to be fired again and re-enter our handler.
        // We register ourself back after we are done with the processing.
        var pwshRunspace = (Runspace)sender;
        pwshRunspace.AvailabilityChanged -= SyncRunspaceState;

        try
        {
            using var ps = PowerShell.Create();
            ps.Runspace = pwshRunspace;

            HistoryInfo? lastHistory = ps
                .AddCommand(s_getHistoryCommand)
                .AddParameter("Count", 1)
                .InvokeAndCleanup<HistoryInfo>()
                .FirstOrDefault();

            if (lastHistory is not null)
            {
                if (lastHistory.Id == _lastHistoryId)
                {
                    _watch.Stop();
                    Console.WriteLine($"--- Early return: {_watch.ElapsedTicks} ticks");
                    return;
                }

                _lastHistoryId = lastHistory.Id;
            }

            SyncCurrentPath(pwshRunspace);
            SyncVariables(pwshRunspace);
            SyncModules(ps);

            _watch.Stop();
            Console.WriteLine($"+++ Sync states: {_watch.ElapsedTicks} ticks");
        }
        finally
        {
            pwshRunspace.AvailabilityChanged += SyncRunspaceState;
        }
    }

    private void SyncModules(PowerShell pwsh)
    {
        Collection<PSModuleInfo> sourceModules = pwsh.AddCommand(s_getModuleCommand).InvokeAndCleanup<PSModuleInfo>();

        foreach (PSModuleInfo module in sourceModules)
        {
            if (!_loadedModules.Contains(module.Path))
            {
                _loadedModules.Add(module.Path);

                if (!module.Name.Contains("predictor", StringComparison.OrdinalIgnoreCase))
                {
                    // The completion predictor should not be imported in more than 1 Runspace,
                    // due to the same 'Id'. I assume that's the same for all other predictors,
                    // so we skip all modules with 'predictor' in the name.
                    _modulesToImport.Add(module);
                }
            }
        }

        if (_modulesToImport.Count > 0)
        {
            string[] name = new string[1];
            using var target = PowerShell.Create();
            target.Runspace = _runspace;

            foreach (PSModuleInfo module in _modulesToImport)
            {
                name[0] = module.Path;
                if (module.ModuleType != ModuleType.Manifest && module.RootModule is not null)
                {
                    string manifest = Path.Combine(module.ModuleBase, $"{module.Name}.psd1");
                    if (File.Exists(manifest))
                    {
                        name[0] = manifest;
                    }
                }

                try
                {
                    target.AddCommand(s_importModuleCommand)
                        .AddParameter("Name", name)
                        .InvokeAndCleanup();
                }
                catch
                {
                    // It's possible a module cannot be imported in more than 1 Runspace.
                    // Ignore any failures in such case.
                }
            }

            _modulesToImport.Clear();
        }
    }

    private void SyncCurrentPath(Runspace source)
    {
        PathInfo currentPath = source.SessionStateProxy.Path.CurrentLocation;
        _runspace.SessionStateProxy.Path.SetLocation(currentPath.Path);
    }

    private void SyncVariables(Runspace source)
    {
        var globalVars = (ICollection<PSVariable>)source
            .SessionStateProxy
            .InvokeProvider
            .Item.Get(@"Variable:\")[0].BaseObject;

        // Figure out which ones require a change in the predictor Runspace.
        foreach (PSVariable variable in globalVars)
        {
            if (_builtInVariables.Contains(variable.Name))
            {
                // Ignore built-in variables.
                continue;
            }

            // Get the hashcode of the value and add to the new-user-var dictionary.
            int newHashCode = variable.Value is null ? 0 : variable.Value.GetHashCode();
            _newUserVars.Add(variable.Name, newHashCode);

            if (_userVariables.TryGetValue(variable.Name, out int oldHashCode))
            {
                if (newHashCode != oldHashCode)
                {
                    // Value of the variable changed, so we will re-set the variable.
                    _varsToSet.Add(variable);
                }

                // Remove the found variable.
                // After the loop, all remaining ones would be those already removed from the PowerShell session.
                _userVariables.Remove(variable.Name);
            }
            else
            {
                // Newly added variable.
                _varsToSet.Add(variable);
            }
        }

        // Now we will be updating the variables in the predictor runspace.
        // The 'AvailabilityChanged' event is handled synchronously and hence it's safe to change state of the
        // predictor Runspace, because the PowerShell hasn't call back to PSReadLine yet.
        PSVariableIntrinsics predictorPSVariable = _runspace.SessionStateProxy.PSVariable;

        foreach (string varName in _userVariables.Keys)
        {
            predictorPSVariable.Remove(varName);
        }

        foreach (PSVariable var in _varsToSet)
        {
            predictorPSVariable.Set(var.Name, var.Value);
        }

        _varsToSet.Clear();
        _userVariables.Clear();

        var temp = _userVariables;
        _userVariables = _newUserVars;
        _newUserVars = temp;
    }

    /// <summary>
    /// Register the 'AvailabilityChanged' event.
    /// </summary>
    private void RegisterEvents()
    {
        Runspace.DefaultRunspace.AvailabilityChanged += SyncRunspaceState;
    }

    /// <summary>
    /// Un-register the 'AvailabilityChanged' event.
    /// </summary>
    private void UnregisterEvents()
    {
        Runspace.DefaultRunspace.AvailabilityChanged -= SyncRunspaceState;
    }
}

internal static class PowerShellExtensions
{
    internal static Collection<T> InvokeAndCleanup<T>(this PowerShell ps)
    {
        var results = ps.Invoke<T>();
        ps.Commands.Clear();

        return results;
    }

    internal static void InvokeAndCleanup(this PowerShell ps)
    {
        ps.Invoke();
        ps.Commands.Clear();
    }
}
