using System.Collections.Concurrent;
using System.Management.Automation.Language;
using System.Management.Automation.Subsystem.Prediction;

namespace Microsoft.PowerShell.Predictor;

internal record GitContext(CommandAst GitCmd, string? WordToComplete, string Input, RepoInfo RepoInfo);

internal partial class GitHandler
{
    private readonly ConcurrentDictionary<string, RepoInfo> _repos;
    private readonly ConcurrentDictionary<string, GitNode> _cmds;

    internal GitHandler()
    {
        _repos = new ConcurrentDictionary<string, RepoInfo>(StringComparer.Ordinal);
    }

    internal SuggestionPackage GetGitResult(
        CommandAst gitCmd,
        string? currentLocation,
        string wordToComplete,
        string input,
        CancellationToken token)
    {
        if (gitCmd.CommandElements.Count is 1)
        {
            return CompleteOnSubCommand(wordToComplete, input);
        }

        if (currentLocation is null || gitCmd.CommandElements.Count is 1 ||
            gitCmd.CommandElements[1] is not StringConstantExpressionAst subCommand)
        {
            return default;
        }

        string? repoRoot = null;
        RepoInfo? repoInfo = null;

        foreach (var pair in _repos)
        {
            string root = pair.Key;
            if (currentLocation.StartsWith(root, StringComparison.Ordinal) &&
                (currentLocation.Length == root.Length || currentLocation[root.Length] == Path.DirectorySeparatorChar))
            {
                repoRoot = root;
                repoInfo = pair.Value;
                break;
            }
        }

        if (repoInfo is null)
        {
            repoRoot = FindRepoRoot(currentLocation);
            if (repoRoot is null)
            {
                // It's not in a git repo.
                return default;
            }

            repoInfo = _repos.GetOrAdd(repoRoot, new RepoInfo(repoRoot));
        }

        if (token.IsCancellationRequested)
        {
            return default;
        }

        GitContext gitContext = new(gitCmd, wordToComplete, input, repoInfo);
        return subCommand.Value.ToLower() switch
        {
            "checkout" => HandleCheckout(gitContext),
            "branch"   => HandleBranch(gitContext),
            "rebase"   => HandleRebase(gitContext),
            "merge"    => HandleMerge(gitContext),
            "push"     => HandlePush(gitContext),
            "reset"    => HandleReset(gitContext),
            _ => default,
        };
    }

    private SuggestionPackage HandleCheckout(GitContext context)
    {
        return default;
    }

    private SuggestionPackage HandleBranch(GitContext context)
    {
        return default;
    }

    private SuggestionPackage HandleRebase(GitContext context)
    {
        return default;
    }

    private SuggestionPackage HandleMerge(GitContext context)
    {
        return default;
    }

    private SuggestionPackage HandlePush(GitContext context)
    {
        return default;
    }

    private SuggestionPackage HandleReset(GitContext context)
    {
        return default;
    }

    private string? FindRepoRoot(string currentLocation)
    {
        string? root = currentLocation;
        while (root is not null)
        {
            string gitDir = Path.Join(root, ".git", "refs");
            if (Directory.Exists(gitDir))
            {
                return root;
            }

            root = Path.GetDirectoryName(root);
        }

        return null;
    }
}
