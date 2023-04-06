namespace Microsoft.PowerShell.Predictor;

delegate List<string>? ArgPredictor(int pos, string filter, RepoInfo repoInfo);

internal sealed class GitNode2
{
    internal GitNode2(string name, ArgPredictor? predictArg)
    {
        Name = name;
        PredictArg = predictArg;
    }

    internal readonly string Name;
    internal readonly ArgPredictor? PredictArg;

    internal static Dictionary<string, GitNode2> InitGitNodes()
    {
        GitFlag common_force = ShortAndLong("f", "force");

        return new(StringComparer.Ordinal) {
            {
                "merge", new(
                    name: "merge",
                    flags: new()
                    {
                        Short(1, "n"),

                        Long(1, "stat"),
                        Long(1, "no-commit"),
                        Long(1, "squash"),
                        Long(1, "no-edit"),                        
                        Long(1, "no-verify"),
                        Long(1, "no-ff"),
                        Long(1, "ff-only"),
                        Long(1, "allow-unrelated-histories"),
                        Long(1, "no-allow-unrelated-histories"),
                        Long(1, "rerere-autoupdate"),
                        Long(1, "no-rerere-autoupdate"),
                        Long(1, "verify-signatures"),
                        Long(1, "into-name", expectArg: true),
                        Long(1, "progress"),
                        Long(1, "autostash"),
                        Long(1, "overwrite-ignore"),
                        Long(1, "signoff"),

                        ShortAndLong(1, "e", "edit"),
                        ShortAndLong(1, "s", "strategy=", new List<string>() { "ort", "recursive", "resolve", "octopus", "ours", "subtree" }),
                        ShortAndLong(1, "X", "strategy-option=", expectArg: true),
                        ShortAndLong(1, "S", "gpg-sign=", expectArg: true),
                        ShortAndLong(1, "m", "message", expectArg: true),
                        ShortAndLong(1, "F", "file=", expectArg: true),
                        ShortAndLong(1, "v", "verbose"),
                        ShortAndLong(1, "q", "quiet"),

                        Long(2, "abort"),
                        Long(2, "quit"),
                        Long(2, "continue"),
                    },

                    (int set, int pos, string filter, RepoInfo repoInfo) => {
                        if (set is not 1 || pos is not 0)
                        {
                            return null;
                        }

                        List<string>? ret = null;
                        string activeBranch = repoInfo.ActiveBranch;

                        if (filter.Length is 0)
                        {
                            foreach (RemoteInfo remote in repoInfo.Remotes)
                            {
                                if (remote.Branches is null)
                                {
                                    continue;
                                }

                                string remoteName = remote.Name;
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
                                ret ??= new List<string>();
                                ret.Add(localBranch);
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
                                    if (remote.Name != remoteName)
                                    {
                                        continue;
                                    }

                                    if (remote.Branches is not null)
                                    {
                                        foreach (string branch in remote.Branches)
                                        {
                                            if (branch.AsSpan().StartsWith(branchName) && branch.Length > branchName.Length)
                                            {
                                                ret ??= new List<string>();
                                                ret.Add($"{remoteName}/{branch}");
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
                                    if (localBranch.StartsWith(filter) && localBranch.Length > filter.Length)
                                    {
                                        ret ??= new List<string>();
                                        ret.Add(localBranch);
                                    }
                                }
                            }
                        }

                        return ret;
                    }
                )
            },
            {
                "branch", new(
                    name: "branch",
                    flags: new()
                    {
                        ShortAndLong(-1, "v", "verbose"),
                        ShortAndLong(-1, "q", "quiet"),

                        Long(1, "color=", new List<string>() { "auto", "never", "always " }),
                        Long(1, "no-color"),
                        Long(1, "show-current"),
                        Long(1, "abbrev=", expectArg: true),
                        Long(1, "no-abbrev"),
                        Long(1, "column=", expectArg: true),
                        Long(1, "no-column"),
                        Long(1, "sort=", expectArg: true),
                        Long(1, "merged", expectArg: true),
                        Long(1, "no-merged", expectArg: true),
                        Long(1, "contains", expectArg: true),
                        Long(1, "no-contains", expectArg: true),
                        Long(1, "points-at", expectArg: true),
                        Long(1, "format=", expectArg: true),
                        ShortAndLong(1, "r", "remotes"),
                        ShortAndLong(1, "a", "all"),

                        ShortAndLong("t", "track=", new List<string>() { "direct", "inherit" }),
                        ShortAndLong("u", "set-upstream-to=", expectArg: true),
                        Long("unset-upstream"),
                        
                        
                        
                        
                        common_all,
                        ShortAndLong("d", "delete"),
                        Short("D"),
                        ShortAndLong("m", "move"),
                        Short("M"),
                        ShortAndLong("c", "copy"),
                        Short("C"),
                        ShortAndLong("l", "list"),
                        
                        Long("create-reflog"),
                        Long("edit-description"),
                        common_force,
                        
                        
                        
                        
                        ShortAndLong("i", "ignore-case"),
                        
                    },
                    (int set, int pos, string filter, RepoInfo repoInfo) => {
                        if (pos is not 0)
                        {
                            return null;
                        }

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
                                ret ??= new List<string>();
                                foreach (string branch in repoInfo.Branches)
                                {
                                    if (localBranches.Contains(branch) && branch != repoInfo.ActiveBranch)
                                    {
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
                                if (branch != repoInfo.ActiveBranch && branch != repoInfo.DefaultBranch)
                                {
                                    ret ??= new List<string>();
                                    ret.Add(branch);
                                }
                            }
                        }

                        return null;
                    }
                )
            },
        };
    }
}
