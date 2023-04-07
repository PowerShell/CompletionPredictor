using System.Management.Automation.Subsystem.Prediction;

namespace Microsoft.PowerShell.Predictor;

internal abstract class GitNode
{
    internal readonly string Name;

    protected GitNode(string name)
    {
        Name = name;
    }

    internal abstract SuggestionPackage Predict(List<string> textElements, string? textAtCursor, string origInput, RepoInfo repoInfo, bool cursorAtGitCmd);
}

internal sealed class Merge : GitNode
{
    internal Merge() : base("merge") { }

    internal override SuggestionPackage Predict(
        List<string> textElements,
        string? textAtCursor,
        string origInput,
        RepoInfo repoInfo,
        bool cursorAtGitCmd)
    {
        if (textAtCursor is not null && textAtCursor.StartsWith('-'))
        {
            // We don't predict flag/option today, but may support it in future.
            return default;
        }

        bool predictArg = true;
        for (int i = 2; i < textElements.Count; i++)
        {
            if (textElements[i] is "--continue" or "--abort" or "--quit")
            {
                predictArg = false;
                break;
            }
        }

        if (predictArg)
        {
            string filter = (cursorAtGitCmd ? null : textAtCursor) ?? string.Empty;
            List<string>? args = PredictArgument(filter, repoInfo);
            if (args is not null)
            {
                List<PredictiveSuggestion> list = new(args.Count);
                foreach (string arg in args)
                {
                    if (textAtCursor is null)
                    {
                        list.Add(new PredictiveSuggestion($"{origInput}{arg}"));
                    }
                    else if (cursorAtGitCmd)
                    {
                        var remainingPortionInCmd = Name.AsSpan(textAtCursor.Length);
                        list.Add(new PredictiveSuggestion($"{origInput}{remainingPortionInCmd} {arg}"));
                    }
                    else
                    {
                        var remainingPortionInArg = arg.AsSpan(textAtCursor.Length);
                        list.Add(new PredictiveSuggestion($"{origInput}{remainingPortionInArg}"));
                    }
                }

                return new SuggestionPackage(list);
            }
        }

        return default;
    }

    private List<string>? PredictArgument(string filter, RepoInfo repoInfo)
    {
        List<string>? ret = null;
        string activeBranch = repoInfo.ActiveBranch;

        if (filter.Length is 0 || !filter.Contains('/'))
        {
            foreach (RemoteInfo remote in repoInfo.Remotes)
            {
                string remoteName = remote.Name;
                if (remote.Branches is null || !remoteName.StartsWith(filter, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (string branch in remote.Branches)
                {
                    if (branch != activeBranch)
                    {
                        continue;
                    }

                    ret ??= new List<string>();
                    string candidate = $"{remoteName}/{branch}";
                    if (remoteName == "upstream")
                    {
                        ret.Insert(index: 0, candidate);
                    }
                    else
                    {
                        ret.Add(candidate);
                    }

                    break;
                }
            }

            foreach (string localBranch in repoInfo.Branches)
            {
                if (localBranch != activeBranch && localBranch.StartsWith(filter, StringComparison.Ordinal))
                {
                    ret ??= new List<string>();
                    ret.Add(localBranch);
                }
            }
        }
        else
        {
            int slashIndex = filter.IndexOf('/');
            if (slashIndex > 0)
            {
                var remoteName = filter.AsSpan(0, slashIndex);
                var branchName = filter.AsSpan(slashIndex + 1);

                foreach (RemoteInfo remote in repoInfo.Remotes)
                {
                    if (remote.Branches is null || !MemoryExtensions.Equals(remote.Name, remoteName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    foreach (string branch in remote.Branches)
                    {
                        if (branch.AsSpan().StartsWith(branchName, StringComparison.Ordinal) && branch.Length > branchName.Length)
                        {
                            ret ??= new List<string>();
                            string candidate = $"{remoteName}/{branch}";
                            if (branch == activeBranch)
                            {
                                ret.Insert(0, candidate);
                            }
                            else
                            {
                                ret.Add(candidate);
                            }
                        }
                    }

                    break;
                }
            }

            if (ret is null)
            {
                foreach (string localBranch in repoInfo.Branches)
                {
                    if (localBranch == activeBranch)
                    {
                        continue;
                    }

                    if (localBranch.StartsWith(filter, StringComparison.Ordinal) && localBranch.Length > filter.Length)
                    {
                        ret ??= new List<string>();
                        ret.Add(localBranch);
                    }
                }
            }
        }

        return ret;
    }
}

internal sealed class Branch : GitNode
{
    internal Branch() : base("branch") { }

    internal override SuggestionPackage Predict(
        List<string> textElements,
        string? textAtCursor,
        string origInput,
        RepoInfo repoInfo,
        bool cursorAtGitCmd)
    {
        ReadOnlySpan<char> autoFill = null;
        bool predictArg = false;

        if (cursorAtGitCmd)
        {
            return default;
        }

        if (textElements.Count is 2 && textAtCursor is null)
        {
            autoFill = "-D ".AsSpan();
            predictArg = true;
        }

        if (textAtCursor is not null && textAtCursor.StartsWith('-'))
        {
            if (textAtCursor is "-" or "-d" or "-D")
            {
                autoFill = "-D ".AsSpan(textAtCursor.Length);
                predictArg = true;
                textAtCursor = null;
            }
            else
            {
                // We don't predict flag/option today, but may support it in future.
                return default;
            }
        }

        if (!predictArg)
        {
            for (int i = 2; i < textElements.Count; i++)
            {
                if (textElements[i] is "-d" or "-D")
                {
                    predictArg = true;
                    break;
                }
            }
        }

        if (predictArg)
        {
            List<string>? args = PredictArgument(textAtCursor ?? string.Empty, repoInfo);
            if (args is not null)
            {
                List<PredictiveSuggestion> list = new(args.Count);
                foreach (string arg in args)
                {
                    if (textAtCursor is null)
                    {
                        list.Add(new PredictiveSuggestion($"{origInput}{autoFill}{arg}"));
                    }
                    else
                    {
                        var remainingPortionInArg = arg.AsSpan(textAtCursor.Length);
                        list.Add(new PredictiveSuggestion($"{origInput}{remainingPortionInArg}"));
                    }
                }

                return new SuggestionPackage(list);
            }
        }

        return default;
    }

    private List<string>? PredictArgument(string filter, RepoInfo repoInfo)
    {
        List<string>? ret = null;
        List<string>? originBranches = null;

        foreach (var remote in repoInfo.Remotes)
        {
            if (remote.Name is "origin")
            {
                originBranches = remote.Branches;
                break;
            }
        }

        if (originBranches is not null)
        {
            // The 'origin' remote exists, so do a smart check to find those local branches
            // that are not available in the 'origin' remote branches.
            HashSet<string> localBranches = new(repoInfo.Branches);
            localBranches.ExceptWith(originBranches);

            if (localBranches.Count > 0)
            {
                foreach (string branch in localBranches)
                {
                    if (branch.StartsWith(filter, StringComparison.Ordinal) &&
                        branch != repoInfo.ActiveBranch)
                    {
                        ret ??= new List<string>();
                        ret.Add(branch);
                    }
                }
            }
        }
        else
        {
            // No 'origin' remote, so just list the local branches, except for the default branch
            // and the current active branch.
            foreach (string branch in repoInfo.Branches)
            {
                if (branch.StartsWith(filter, StringComparison.Ordinal) &&
                    branch != repoInfo.ActiveBranch &&
                    branch != repoInfo.DefaultBranch)
                {
                    ret ??= new List<string>();
                    ret.Add(branch);
                }
            }
        }

        return ret;
    }
}

internal sealed class Checkout : GitNode
{
    internal Checkout() : base("checkout") { }

    internal override SuggestionPackage Predict(
        List<string> textElements,
        string? textAtCursor,
        string origInput,
        RepoInfo repoInfo,
        bool cursorAtGitCmd)
    {
        if (textAtCursor is not null && textAtCursor.StartsWith('-'))
        {
            // We don't predict flag/option today, but may support it in future.
            return default;
        }

        int argCount = 0;
        bool predictArg = true;
        bool hasDashB = false;

        for (int i = 2; i < textElements.Count; i++)
        {
            if (textElements[i] is "-b" or "-B")
            {
                hasDashB = true;
                continue;
            }

            if (hasDashB && !textElements[i].StartsWith('-'))
            {
                argCount += 1;
            }
        }

        if (hasDashB)
        {
            predictArg = (argCount is 1 && textAtCursor is null)
                || (argCount is 2 && textAtCursor is not null);
        }

        if (predictArg)
        {
            string filter = (cursorAtGitCmd ? null : textAtCursor) ?? string.Empty;
            List<string>? args = PredictArgument(filter, repoInfo, hasDashB ? false : true);
            if (args is not null)
            {
                List<PredictiveSuggestion> list = new(args.Count);
                foreach (string arg in args)
                {
                    if (textAtCursor is null)
                    {
                        list.Add(new PredictiveSuggestion($"{origInput}{arg}"));
                    }
                    else if (cursorAtGitCmd)
                    {
                        var remainingPortionInCmd = Name.AsSpan(textAtCursor!.Length);
                        list.Add(new PredictiveSuggestion($"{origInput}{remainingPortionInCmd} {arg}"));
                    }
                    else
                    {
                        var remainingPortionInArg = arg.AsSpan(textAtCursor.Length);
                        list.Add(new PredictiveSuggestion($"{origInput}{remainingPortionInArg}"));
                    }
                }

                return new SuggestionPackage(list);
            }
        }

        return default;
    }

    private List<string>? PredictArgument(string filter, RepoInfo repoInfo, bool excludeActiveBranch)
    {
        List<string>? ret = null;

        foreach (string localBranch in repoInfo.Branches)
        {
            if (excludeActiveBranch && localBranch == repoInfo.ActiveBranch)
            {
                continue;
            }

            if (localBranch.StartsWith(filter, StringComparison.Ordinal) &&
                localBranch.Length > filter.Length)
            {
                ret ??= new List<string>();
                ret.Add(localBranch);
            }
        }

        return ret;
    }
}

internal sealed class Push : GitNode
{
    internal Push() : base("push") { }

    internal override SuggestionPackage Predict(
        List<string> textElements,
        string? textAtCursor,
        string origInput,
        RepoInfo repoInfo,
        bool cursorAtGitCmd)
    {
        ReadOnlySpan<char> autoFill = null;
        bool hasAutoFill = false;

        if (cursorAtGitCmd)
        {
            hasAutoFill = true;
            autoFill = Name.AsSpan(textAtCursor!.Length);
            textAtCursor = null;
        }

        if (textAtCursor is not null && textAtCursor.StartsWith('-'))
        {
            const string forceWithLease = "--force-with-lease ";
            if (forceWithLease.StartsWith(textAtCursor, StringComparison.Ordinal))
            {
                hasAutoFill = true;
                autoFill = forceWithLease.AsSpan(textAtCursor.Length);
                textAtCursor = null;
            }
            else
            {
                // We don't predict flag/option today, but may support it in future.
                return default;
            }
        }

        int argCount = 0;
        for (int i = 2; i < textElements.Count; i++)
        {
            if (!textElements[i].StartsWith('-'))
            {
                argCount += 1;
            }
        }

        int pos = -1;
        if ((argCount is 0 && textAtCursor is null)
            || (argCount is 1 && textAtCursor is not null))
        {
            pos = 0;
        }
        else if ((argCount is 1 && textAtCursor is null)
            || (argCount is 2 && textAtCursor is not null))
        {
            pos = 1;
        }

        string filter = textAtCursor ?? string.Empty;
        string activeBranch = repoInfo.ActiveBranch;
        List<PredictiveSuggestion>? list = null;

        if (pos is 0)
        {
            foreach (RemoteInfo remote in repoInfo.Remotes)
            {
                string remoteName = remote.Name;
                if (!remoteName.StartsWith(filter, StringComparison.Ordinal))
                {
                    continue;
                }

                string candidate;
                list ??= new List<PredictiveSuggestion>();

                if (textAtCursor is null)
                {
                    candidate = hasAutoFill
                        ? $"{origInput}{autoFill} {remoteName} {activeBranch}"
                        : $"{origInput}{remoteName} {activeBranch}";
                }
                else
                {
                    candidate = $"{origInput}{remoteName.AsSpan(textAtCursor.Length)} {activeBranch}";
                }

                if (remoteName is "origin")
                {
                    list.Insert(0, new PredictiveSuggestion(candidate));
                }
                else
                {
                    list.Add(new PredictiveSuggestion(candidate));
                }
            }
        }
        else if (pos is 2)
        {
            if (textAtCursor is null || (activeBranch.StartsWith(textAtCursor) && activeBranch.Length > textAtCursor.Length))
            {
                list ??= new List<PredictiveSuggestion>();
                list.Add(new PredictiveSuggestion($"{origInput}{activeBranch}"));
            }
        }

        return list is null ? default : new SuggestionPackage(list);
    }
}
