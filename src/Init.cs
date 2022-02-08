using System.Management.Automation;
using System.Management.Automation.Subsystem;
using System.Management.Automation.Subsystem.Prediction;

namespace Microsoft.PowerShell.Predictor;

public class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    private const string Id = "77bb0bd8-2d8b-4210-ad14-79fb91a75eab";

    public void OnImport()
    {
        var predictor = new CompletionPredictor(Id);
        SubsystemManager.RegisterSubsystem<ICommandPredictor, CompletionPredictor>(predictor);
    }

    public void OnRemove(PSModuleInfo psModuleInfo)
    {
        SubsystemManager.UnregisterSubsystem<ICommandPredictor>(new Guid(Id));
    }
}
