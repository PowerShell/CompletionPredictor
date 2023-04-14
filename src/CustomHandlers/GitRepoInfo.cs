using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Predictor;

internal class RepoInfo
{
    private readonly object _syncObj;
    private readonly string _root;
    private readonly string _git;
    private readonly string _head;
    private readonly string _ref_remotes;
    private readonly string _ref_heads;

    private bool _checkForUpdate;
    private string? _defaultBranch;
    private string? _activeBranch;
    private List<string>? _branches;
    private List<RemoteInfo>? _remotes;
    private DateTime? _ref_remote_LastWrittenTimeUtc;
    private DateTime? _ref_heads_LastWrittenTimeUtc;
    private DateTime? _head_LastWrittenTimeUtc;

    internal RepoInfo(string root)
    {
        _syncObj = new();
        _checkForUpdate = true;
        _root = root;
        _git = Path.Join(root, ".git");
        _head = Path.Join(_git, "HEAD");
        _ref_heads = Path.Join(_git, "refs", "heads");
        _ref_remotes = Path.Join(_git, "refs", "remotes");
    }

    internal string RepoRoot => _root;

    internal string DefaultBranch
    {
        get
        {
            if (_defaultBranch is null)
            {
                Refresh();
            }
            return _defaultBranch!;
        }
    }

    internal string ActiveBranch
    {
        get
        {
            Refresh();
            return _activeBranch!;
        }
    }

    internal List<string> Branches
    {
        get
        {
            Refresh();
            return _branches!;
        }
    }

    internal List<RemoteInfo> Remotes
    {
        get
        {
            Refresh();
            return _remotes!;
        }
    }

    internal void NeedCheckForUpdate()
    {
        _checkForUpdate = true;
        if (_remotes is null)
        {
            return;
        }

        foreach (var remote in _remotes)
        {
            remote.NeedCheckForUpdate();
        }
    }

    private void Refresh()
    {
        if (_checkForUpdate)
        {
            lock(_syncObj)
            {
                if (_checkForUpdate)
                {
                    if (_head_LastWrittenTimeUtc == null || File.GetLastWriteTimeUtc(_head) > _head_LastWrittenTimeUtc)
                    {
                        (_activeBranch, _head_LastWrittenTimeUtc) = GetActiveBranch();
                    }

                    if (_ref_heads_LastWrittenTimeUtc == null || Directory.GetLastWriteTimeUtc(_ref_heads) > _ref_heads_LastWrittenTimeUtc)
                    {
                        (_branches, _ref_heads_LastWrittenTimeUtc) = GetBranches();
                    }

                    if (_ref_remote_LastWrittenTimeUtc == null || Directory.GetLastWriteTimeUtc(_ref_remotes) > _ref_remote_LastWrittenTimeUtc)
                    {
                        (_remotes, _ref_remote_LastWrittenTimeUtc) = GetRemotes();
                    }

                    if (_defaultBranch is null)
                    {
                        bool hasMaster = false, hasMain = false;
                        foreach (var branch in _branches!)
                        {
                            if (branch == "master")
                            {
                                hasMaster = true;
                                break;
                            }

                            if (branch == "main")
                            {
                                hasMain = true;
                            }
                        }

                        _defaultBranch = hasMaster ? "master" : hasMain ? "main" : string.Empty;
                    }

                    _checkForUpdate = false;
                }
            }
        }
    }

    private (string, DateTime) GetActiveBranch()
    {
        var head = new FileInfo(_head);
        using var reader = head.OpenText();
        string content = reader.ReadLine()!;
        return (content.Substring("ref: refs/heads/".Length), head.LastWriteTimeUtc);
    }

    private (List<string>, DateTime) GetBranches()
    {
        var ret = new List<string>();
        var dirInfo = new DirectoryInfo(_ref_heads);

        if (dirInfo.Exists)
        {
            RemoteInfo.ReadBranches(dirInfo, ret);
            return (ret, dirInfo.LastWriteTimeUtc);
        }

        return (ret, DateTime.UtcNow);
    }

    private (List<RemoteInfo>, DateTime) GetRemotes()
    {
        var ret = new List<RemoteInfo>();
        var dirInfo = new DirectoryInfo(_ref_remotes);

        if (dirInfo.Exists)
        {
            foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories())
            {
                ret.Add(new RemoteInfo(dir.Name, dir.FullName));
            }

            return (ret, dirInfo.LastWriteTimeUtc);
        }

        return (ret, DateTime.UtcNow);
    }
}

internal class RemoteInfo
{
    private static readonly bool s_isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static readonly EnumerationOptions s_enumOption = new() { RecurseSubdirectories = true, IgnoreInaccessible = true, };

    private readonly object _syncObj;
    private readonly string _path;

    private bool _checkForUpdate;
    private List<string>? _branches;
    private DateTime? _lastWrittenTimeUtc;

    internal readonly string Name;

    internal RemoteInfo(string name, string path)
    {
        _syncObj = new();
        _path = path;

        Name = name;
    }

    internal List<string>? Branches
    {
        get
        {
            Refresh();
            return _branches;
        }
    }

    internal void NeedCheckForUpdate()
    {
        _checkForUpdate = true;
    }

    internal static void ReadBranches(DirectoryInfo dirInfo, List<string> branches)
    {
        foreach (FileInfo file in dirInfo.EnumerateFiles("*", s_enumOption))
        {
            string name = Path.GetRelativePath(dirInfo.FullName, file.FullName);
            if (name == "HEAD")
            {
                using var reader = file.OpenText();
                string? content = reader.ReadLine();
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                name = content.Substring("ref: refs/remotes/".Length + dirInfo.Name.Length + 1);
            }

            branches.Add(s_isWindows ? name.Replace('\\', '/') : name);
        }
    }

    private void Refresh()
    {
        if (ShouldUpdate())
        {
            lock(_syncObj)
            {
                if (ShouldUpdate())
                {
                    var dirInfo = new DirectoryInfo(_path);
                    var option = new EnumerationOptions()
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true,
                    };

                    var branches = new List<string>();
                    ReadBranches(dirInfo, branches);

                    // Reference assignment is an atomic operation.
                    _branches = branches;
                    _checkForUpdate = false;
                    _lastWrittenTimeUtc = dirInfo.LastWriteTimeUtc;
                }
            }
        }
    }

    private bool ShouldUpdate()
    {
        if (_lastWrittenTimeUtc is null)
        {
            return true;
        }

        return _checkForUpdate && Directory.GetLastWriteTimeUtc(_path) > _lastWrittenTimeUtc;
    }
}
