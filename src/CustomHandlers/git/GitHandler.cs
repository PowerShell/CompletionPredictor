using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Language;
using System.Management.Automation.Subsystem.Prediction;

namespace Microsoft.PowerShell.Predictor;

internal partial class GitHandler
{
    private readonly ConcurrentDictionary<string, RepoInfo> _repos;
    private readonly Dictionary<string, GitNode> _gitCmds;

    internal GitHandler()
    {
        _repos = new(StringComparer.Ordinal);
        _gitCmds = new(StringComparer.Ordinal)
        {
            { "merge", new Merge() },
            { "branch", new Branch() },
            { "checkout", new Checkout() },
            { "push", new Push() },
        };
    }

    internal void SignalCheckForRepoUpdate()
    {
        foreach (var repoInfo in _repos.Values)
        {
            repoInfo.NeedCheckForUpdate();
        }
    }

    internal SuggestionPackage GetGitResult(CommandAst gitAst, string? cwd, PredictionContext context, CancellationToken token)
    {
        var elements = gitAst.CommandElements;
        if (cwd is null || elements.Count is 1 || !TryConvertToText(elements, out List<string>? textElements))
        {
            return default;
        }

        RepoInfo? repoInfo = GetRepoInfo(cwd);
        if (repoInfo is null || token.IsCancellationRequested)
        {
            return default;
        }

        string gitCmd = textElements[1];
        string? textAtCursor = context.TokenAtCursor?.Text;
        bool cursorAtGitCmd = textElements.Count is 2 && textAtCursor is not null;

        if (!_gitCmds.TryGetValue(gitCmd, out GitNode? node))
        {
            if (cursorAtGitCmd)
            {
                foreach (var entry in _gitCmds)
                {
                    if (entry.Key.StartsWith(textAtCursor!))
                    {
                        node = entry.Value;
                        break;
                    }
                }
            }
        }

        if (node is not null)
        {
            return node.Predict(textElements, textAtCursor, context.InputAst.Extent.Text, repoInfo, cursorAtGitCmd);
        }

        return default;
    }

    private bool TryConvertToText(
        ReadOnlyCollection<CommandElementAst> elements,
        [NotNullWhen(true)] out List<string>? textElements)
    {
        textElements = new(elements.Count);
        foreach (var e in elements)
        {
            switch (e)
            {
                case StringConstantExpressionAst str:
                    textElements.Add(str.Value);
                    break;
                case CommandParameterAst param:
                    textElements.Add(param.Extent.Text);
                    break;
                default:
                    textElements = null;
                    return false;
            }
        }

        return true;
    }

    private RepoInfo? GetRepoInfo(string cwd)
    {
        if (_repos.TryGetValue(cwd, out RepoInfo? repoInfo))
        {
            return repoInfo;
        }

        foreach (var entry in _repos)
        {
            string root = entry.Key;
            if (cwd.StartsWith(root) && cwd[root.Length] == Path.DirectorySeparatorChar)
            {
                repoInfo = entry.Value;
                break;
            }
        }

        if (repoInfo is null)
        {
            string? repoRoot = FindRepoRoot(cwd);
            if (repoRoot is not null)
            {
                repoInfo = _repos.GetOrAdd(repoRoot, new RepoInfo(repoRoot));
            }
        }

        return repoInfo;
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
