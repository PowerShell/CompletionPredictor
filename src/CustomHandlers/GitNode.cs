namespace Microsoft.PowerShell.Predictor;

delegate List<string>? ArgPredictor(int pos, string filter, RepoInfo repoInfo);

internal sealed class GitNode
{
    internal GitNode(string name, ArgPredictor? predictArg)
    {
        Name = name;
        PredictArg = predictArg;
    }

    internal readonly string Name;
    internal readonly ArgPredictor? PredictArg;

    internal static Dictionary<string, GitNode> InitGitNodes()
    {
        return new(StringComparer.Ordinal) {
            {
                "merge", new(
                    name: "merge",
                    // 
                    (int pos, string filter, RepoInfo repoInfo) => {
                        if (pos is not 0)
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
                    (int pos, string filter, RepoInfo repoInfo) => {
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
            {
                "checkout", new(
                    name: "checkout",

                )
            }
        };
    }
}
