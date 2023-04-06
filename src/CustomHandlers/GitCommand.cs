using System.Management.Automation.Subsystem.Prediction;

namespace Microsoft.PowerShell.Predictor;

internal sealed class GitNode
{
    internal GitNode(string name)
        : this(name, shortFlags: null, longFlags: null, paramValue: null)
    {
    }

    internal GitNode(string name, List<string>? shortFlags, List<string>? longFlags, Dictionary<string, List<string>>? paramValue)
    {
        Name = name;
        ShortFlags = shortFlags;
        LongFlags = longFlags;
        ParamValue = paramValue;
    }

    internal readonly string Name;
    internal readonly List<string>? ShortFlags;
    internal readonly List<string>? LongFlags;
    internal readonly Dictionary<string, List<string>>? ParamValue;

    internal Dictionary<string, GitNode?>? SubCommands;
    internal Func<RepoInfo, SuggestionPackage>? GetArgument;
}

internal partial class GitHandler
{
    private static Dictionary<string, GitNode?> GetGitCommands()
    {
        #region Commands

        var gitCommands = new Dictionary<string, GitNode?>()
        {
            {
                "add",
                new GitNode(
                    "add",
                    new() { "n", "v", "f", "i", "p", "e", "u", "A", "N" },
                    new() { "dry-run", "verbose", "force", "interactive", "patch", "edit", "update", "all", "no-ignore-removal", "no-all", "ignore-removal", "intent-to-add", "refresh", "ignore-errors", "ignore-missing", "renormalize" },
                    paramValue: null
                )
            },
            { "am", null },
            { "annotate", null },
            { "archive", null },
            { "bisect", new GitNode(name: "bisect") },
            {
                "blame",
                new GitNode(
                    "blame",
                    new() { "b", "L", "l", "t", "S", "p", "M", "C", "h", "c", "f", "n", "s", "e", "w" },
                    new() { "root", "show-stats", "reverse", "porcelain", "line-porcelain", "incremental", "encoding=", "contents", "date", "score-debug", "show-name", "show-number", "show-email", "abbrev" },
                    new()
                    {
                        { "encoding", new() { "utf-8", "none" } }
                    }
                )
            },
            {
                "branch",
                new GitNode(
                    "branch",
                    new() { "d", "D", "l", "f", "m", "M", "r", "a", "v", "vv", "q", "t", "u" },
                    new() { "color", "no-color", "list", "abbrev=", "no-abbrev", "column", "no-column", "merged", "no-merged", "contains", "set-upstream", "track", "no-track", "set-upstream-to=", "unset-upstream", "edit-description", "delete", "create-reflog", "force", "move", "all", "verbose", "quiet" },
                    new()
                    {
                        { "color", new() { "always", "never", "auto" } },
                        { "abbrev", new() { "7", "8", "9", "10" } }
                    }
                )
            },
            { "bundle", null },
            {
                "checkout",
                new GitNode(
                    "checkout",
                    new() { "q", "f", "b", "B", "t", "l", "m", "p" },
                    new() { "quiet", "force", "ours", "theirs", "track", "no-track", "detach", "orphan", "ignore-skip-worktree-bits", "merge", "conflict=", "patch" },
                    new()
                    {
                        { "conflict", new() { "merge", "diff3" } }
                    }
                )
            },
            {
                "cherry",
                new GitNode(
                    "cherry",
                    new() { "v" },
                    longFlags: null,
                    paramValue: null
                )
            },
            {
                "cherry-pick",
                new GitNode(
                    "cherry-pick",
                    new() { "e", "x", "r", "m", "n", "s", "S", "X" },
                    new() { "edit", "mainline", "no-commit", "signoff", "gpg-sign", "ff", "allow-empty", "allow-empty-message", "keep-redundant-commits", "strategy=", "strategy-option=", "continue", "quit", "abort" },
                    new()
                    {
                        { "strategy", new() { "resolve", "recursive", "octopus", "ours", "subtree" } }
                    }
                )
            },
            { "citool", null },
            {
                "clean",
                new GitNode(
                    "clean",
                    new() { "d", "f", "i", "n", "q", "e", "x", "X" },
                    new() { "force", "interactive", "dry-run", "quiet", "exclude=" },
                    paramValue: null
                )
            },
            {
                "clone",
                new GitNode(
                    "clone",
                    new() { "l", "s", "q", "v", "n", "o", "b", "u", "c" },
                    new() { "local", "no-hardlinks", "shared", "reference", "quiet", "verbose", "progress", "no-checkout", "bare", "mirror", "origin", "branch", "upload-pack", "template=", "config", "depth", "single-branch", "no-single-branch", "recursive", "recurse-submodules", "separate-git-dir=" },
                    paramValue: null
                )
            },
            {
                "commit",
                new GitNode(
                    "commit",
                    new() { "a", "p", "C", "c", "z", "F", "m", "t", "s", "n", "e", "i", "o", "u", "v", "q", "S" },
                    new() { "all", "patch", "reuse-message", "reedit-message", "fixup", "squash", "reset-author", "short", "branch", "porcelain", "long", "null", "file", "author", "date", "message", "template", "signoff", "no-verify", "allow-empty", "allow-empty-message", "cleanup=", "edit", "no-edit", "amend", "no-post-rewrite", "include", "only", "untracked-files", "verbose", "quiet", "dry-run", "status", "no-status", "gpg-sign", "no-gpg-sign" },
                    new()
                    {
                        { "cleanup", new() { "strip", "whitespace", "verbatim", "scissors", "default" } }
                    }
                )
            },
            {
                "config",
                new GitNode(
                    "config",
                    new() { "f", "l", "z", "e" },
                    new() { "replace-all", "add", "get", "get-all", "get-regexp", "get-urlmatch", "global", "system", "local", "file", "blob", "remove-section", "rename-section", "unset", "unset-all", "list", "bool", "int", "bool-or-int", "path", "null", "get-colorbool", "get-color", "edit", "includes", "no-includes" },
                    paramValue: null
                )
            },
            {
                "describe",
                new GitNode(
                    "describe",
                    shortFlags: null,
                    new() { "dirty", "all", "tags", "contains", "abbrev", "candidates=", "exact-match", "debug", "long", "match", "always", "first-parent" },
                    paramValue: null
                )
            },
            {
                "diff",
                new GitNode(
                    "diff",
                    new() { "p", "u", "s", "U", "z", "B", "M", "C", "D", "l", "S", "G", "O", "R", "a", "b", "w", "W" },
                    new() { "cached", "patch", "no-patch", "unified=", "raw", "patch-with-raw", "minimal", "patience", "histogram", "diff-algorithm=", "stat", "numstat", "shortstat", "dirstat", "summary", "patch-with-stat", "name-only", "name-status", "submodule", "color", "no-color", "word-diff", "word-diff-regex", "color-words", "no-renames", "check", "full-index", "binary", "apprev", "break-rewrites", "find-renames", "find-copies", "find-copies-harder", "irreversible-delete", "diff-filter=", "pickaxe-all", "pickaxe-regex", "relative", "text", "ignore-space-at-eol", "ignore-space-change", "ignore-all-space", "ignore-blank-lines", "inter-hunk-context=", "function-context", "exit-code", "quiet", "ext-diff", "no-ext-diff", "textconv", "no-textconv", "ignore-submodules", "src-prefix", "dst-prefix", "no-prefix", "staged" },
                    new()
                    {
                        { "unified", new() { "0", "1", "2", "3", "4", "5" } },
                        { "diff-algorithm", new() { "default", "patience", "minimal", "histogram", "myers" } },
                        { "color", new() { "always", "never", "auto" } },
                        { "word-diff", new() { "color", "plain", "porcelain", "none" } },
                        { "abbrev", new() { "7", "8", "9", "10" } },
                        { "diff-filter", new() { "A", "C", "D", "M", "R", "T", "U", "X", "B", "*" } },
                        { "inter-hunk-context", new() { "0", "1", "2", "3", "4", "5" } },
                        { "ignore-submodules", new() { "none", "untracked", "dirty", "all" } }
                    }
                )
            },
            {
                "difftool",
                new GitNode(
                    "difftool",
                    new() { "d", "y", "t", "x", "g" },
                    new() { "dir-diff", "no-prompt", "prompt", "tool=", "tool-help", "no-symlinks", "symlinks", "extcmd=", "gui" },
                    new()
                    {
                        { "tool", new() { "vimdiff", "vimdiff2", "araxis", "bc3", "codecompare", "deltawalker", "diffmerge", "diffuse", "ecmerge", "emerge", "gvimdiff", "gvimdiff2", "kdiff3", "kompare", "meld", "opendiff", "p4merge", "tkdiff", "xxdiff" } }
                    }
                )
            },
            {
                "fetch",
                new GitNode(
                    "fetch",
                    new() { "a", "f", "k", "p", "n", "t", "u", "q", "v" },
                    new() { "all", "append", "depth=", "unshallow", "update-shallow", "dry-run", "force", "keep", "multiple", "prune", "no-tags", "tags", "recurse-submodules=", "no-recurse-submodules", "submodule-prefix=", "recurse-submodules-default=", "update-head-ok", "upload-pack", "quiet", "verbose", "progress" },
                    new()
                    {
                        { "recurse-submodules", new() { "yes", "on-demand", "no" } },
                        { "recurse-submodules-default", new() { "yes", "on-demand" } }
                    }
                )
            },
            { "format-patch", null },
            {
                "gc",
                new GitNode(
                    "gc",
                    shortFlags: null,
                    new() { "aggressive", "auto", "prune=", "no-prune", "quiet", "force" },
                    paramValue: null
                )
            },
            {
                "grep",
                new GitNode(
                    "grep",
                    new() { "a", "i", "I", "w", "v", "h", "H", "E", "G", "P", "F", "n", "l", "L", "O", "z", "c", "p", "C", "A", "B", "W", "f", "e", "q" },
                    new() { "cached", "no-index", "untracked", "no-exclude-standard", "exclude-standard", "text", "textconv", "no-textconv", "ignore-case", "max-depth", "word-regexp", "invert-match", "full-name", "extended-regexp", "basic-regexp", "perl-regexp", "fixed-strings", "line-number", "files-with-matches", "open-file-in-pager", "null", "count", "color", "no-color", "break", "heading", "show-function", "context", "after-context", "before-context", "function-context", "and", "or", "not", "all-match", "quiet" },
                    paramValue: null
                )
            },
            { "gui", null },
            {
                "help",
                new GitNode(
                    "help",
                    new() { "a", "g", "i", "m", "w" },
                    new() { "all", "guides", "info", "man", "web" },
                    paramValue: null
                )
            },
            {
                "init",
                new GitNode(
                    "init",
                    new() { "q" },
                    new() { "quiet", "bare", "template=", "separate-git-dir=", "shared=" },
                    new()
                    {
                        { "shared", new() { "false", "true", "umask", "group", "all", "world", "everybody", "o" } }
                    }
                )
            },
            { "instaweb", null },
            {
                "log",
                new GitNode(
                    "log",
                    new() { "L", "n", "i", "E", "F", "g", "c", "c", "m", "r", "t" },
                    new() { "follow", "no-decorate", "decorate", "source", "use-mailmap", "full-diff", "log-size", "max-count", "skip", "since", "after", "until", "before", "author", "committer", "grep-reflog", "grep", "all-match", "regexp-ignore-case", "basic-regexp", "extended-regexp", "fixed-strings", "perl-regexp", "remove-empty", "merges", "no-merges", "min-parents", "max-parents", "no-min-parents", "no-max-parents", "first-parent", "not", "all", "branches", "tags", "remote", "glob=", "exclude=", "ignore-missing", "bisect", "stdin", "cherry-mark", "cherry-pick", "left-only", "right-only", "cherry", "walk-reflogs", "merge", "boundary", "simplify-by-decoration", "full-history", "dense", "sparse", "simplify-merges", "ancestry-path", "date-order", "author-date-order", "topo-order", "reverse", "objects", "objects-edge", "unpacked", "no-walk=", "do-walk", "pretty", "format=", "abbrev-commit", "no-abbrev-commit", "oneline", "encoding=", "notes", "no-notes", "standard-notes", "no-standard-notes", "show-signature", "relative-date", "date=", "parents", "children", "left-right", "graph", "show-linear-break", "patch", "stat" },
                    new()
                    {
                        { "decorate", new() { "short", "full", "no" } },
                        { "no-walk", new() { "sorted", "unsorted" } },
                        { "pretty", new() { "oneline", "short", "medium", "full", "fuller", "email", "raw" } },
                        { "format", new() { "oneline", "short", "medium", "full", "fuller", "email", "raw" } },
                        { "encoding", new() { "UTF-8" } },
                        { "date", new() { "relative", "local", "default", "iso", "rfc", "short", "raw" } }
                    }
                )
            },
            {
                "merge",
                new GitNode(
                    "merge",
                    new() { "e", "n", "s", "X", "q", "v", "S", "m" },
                    new() { "commit", "no-commit", "edit", "no-edit", "ff", "no-ff", "ff-only", "log", "no-log", "stat", "no-stat", "squash", "no-squash", "strategy", "strategy-option", "verify-signatures", "no-verify-signatures", "summary", "no-summary", "quiet", "verbose", "progress", "no-progress", "gpg-sign", "rerere-autoupdate", "no-rerere-autoupdate", "abort", "allow-unrelated-histories" },
                    new()
                    {
                        { "strategy", new() { "resolve", "recursive", "octopus", "ours", "subtree" } },
                        { "log", new() { "1", "2", "3", "4", "5", "6", "7", "8", "9" } }
                    }
                )
            },
            {
                "mergetool",
                new GitNode(
                    "mergetool",
                    new() { "t", "y" },
                    new() { "tool=", "tool-help", "no-prompt", "prompt" },
                    new()
                    {
                        { "tool", new() { "vimdiff", "vimdiff2", "araxis", "bc3", "codecompare", "deltawalker", "diffmerge", "diffuse", "ecmerge", "emerge", "gvimdiff", "gvimdiff2", "kdiff3", "kompare", "meld", "opendiff", "p4merge", "tkdiff", "xxdiff" } }
                    }
                )
            },
            {
                "mv",
                new GitNode(
                    "mv",
                    new() { "f", "k", "n", "v" },
                    new() { "force", "dry-run", "verbose" },
                    paramValue: null
                )
            },
            {
                "notes",
                new GitNode(
                    "notes",
                    shortFlags: null,
                    new() { "ref" },
                    paramValue: null
                )
            },
            {
                "prune",
                new GitNode(
                    "prune",
                    new() { "n", "v" },
                    new() { "dry-run", "verbose", "expire" },
                    paramValue: null
                )
            },
            {
                "pull",
                new GitNode(
                    "pull",
                    new() { "q", "v", "e", "n", "s", "X", "r", "a", "f", "k", "u" },
                    new() { "quiet", "verbose", "recurse-submodules=", "no-recurse-submodules=", "commit", "no-commit", "edit", "no-edit", "ff", "no-ff", "ff-only", "log", "no-log", "stat", "no-stat", "squash", "no-squash", "strategy=", "strategy-option=", "verify-signatures", "no-verify-signatures", "summary", "no-summary", "rebase=", "no-rebase", "all", "append", "depth=", "unshallow", "update-shallow", "force", "keep", "no-tags", "update-head-ok", "upload-pack", "progress" },
                    new()
                    {
                        { "strategy", new() { "resolve", "recursive", "octopus", "ours", "subtree" } },
                        { "recurse-submodules", new() { "yes", "on-demand", "no" } },
                        { "no-recurse-submodules", new() { "yes", "on-demand", "no" } },
                        { "rebase", new() { "false", "true", "preserve" } }
                    }
                )
            },
            {
                "push",
                new GitNode(
                    "push",
                    new() { "n", "f", "u", "q", "v" },
                    new() { "all", "prune", "mirror", "dry-run", "porcelain", "delete", "tags", "follow-tags", "receive-pack=", "exec=", "force-with-lease", "no-force-with-lease", "force", "repo=", "set-upstream", "thin", "no-thin", "quiet", "verbose", "progress", "recurse-submodules=", "verify", "no-verify" },
                    new()
                    {
                        { "recurse-submodules", new() { "check", "on-demand" } }
                    }
                )
            },
            {
                "rebase",
                new GitNode(
                    "rebase",
                    new() { "m", "s", "X", "S", "q", "v", "n", "C", "f", "i", "p", "x" },
                    new() { "onto", "continue", "abort", "keep-empty", "skip", "edit-todo", "merge", "strategy=", "strategy-option=", "gpg-sign", "quiet", "verbose", "stat", "no-stat", "no-verify", "verify", "force-rebase", "fork-point", "no-fork-point", "ignore-whitespace", "whitespace=", "committer-date-is-author-date", "ignore-date", "interactive", "preserve-merges", "exec", "root", "autosquash", "no-autosquash", "autostash", "no-autostash", "no-ff" },
                    new()
                    {
                        { "strategy", new() { "resolve", "recursive", "octopus", "ours", "subtree" } }
                    }
                )
            },
            { "reflog", new GitNode(name: "reflog") },
            {
                "remote",
                new GitNode(
                    "remote",
                    new() { "v" },
                    new() { "verbose" },
                    paramValue: null
                )
            },
            { "rerere", new GitNode(name: "rerere") },
            {
                "reset",
                new GitNode(
                    "reset",
                    new() { "q", "p" },
                    new() { "patch", "quiet", "soft", "mixed", "hard", "merge", "keep" },
                    paramValue: null
                )
            },
            {
                "restore",
                new GitNode(
                    "restore",
                    new() { "s", "p", "W", "S", "q", "m" },
                    new() { "source=", "patch", "worktree", "staged", "quiet", "progress", "no-progress", "ours", "theirs", "merge", "conflict=", "ignore-unmerged", "ignore-skip-worktree-bits", "overlay", "no-overlay" },
                    new()
                    {
                        { "conflict", new() { "merge", "diff3" } }
                    }
                )
            },
            {
                "revert",
                new GitNode(
                    "revert",
                    new() { "e", "m", "n", "S", "s", "X" },
                    new() { "edit", "mainline", "no-edit", "no-commit", "gpg-sign", "signoff", "strategy=", "strategy-option", "continue", "quit", "abort" },
                    new()
                    {
                        { "strategy", new() { "resolve", "recursive", "octopus", "ours", "subtree" } }
                    }
                )
            },
            {
                "rm",
                new GitNode(
                    "rm",
                    new() { "f", "n", "r", "q" },
                    new() { "force", "dry-run", "cached", "ignore-unmatch", "quiet" },
                    paramValue: null
                )
            },
            {
                "shortlog",
                new GitNode(
                    "shortlog",
                    new() { "n", "s", "e", "w" },
                    new() { "numbered", "summary", "email", "format=" },
                    paramValue: null
                )
            },
            {
                "show",
                new GitNode(
                    "show",
                    shortFlags: null,
                    new() { "pretty=", "format=", "abbrev-commit", "no-abbrev-commit", "oneline", "encoding=", "expand-tabs", "no-expand-tabs", "notes", "no-notes", "show-notes", "no-standard-notes", "standard-notes", "show-signature", "name-only", "name-status", "stat", "shortstat", "numstat" },
                    new()
                    {
                        { "pretty", new() { "oneline", "short", "medium", "full", "fuller", "email", "raw" } },
                        { "format", new() { "oneline", "short", "medium", "full", "fuller", "email", "raw" } },
                        { "encoding", new() { "utf-8" } }
                    }
                )
            },
            { "stash", new GitNode(name: "stash") },
            {
                "status",
                new GitNode(
                    "status",
                    new() { "s", "b", "u", "z" },
                    new() { "short", "branch", "porcelain", "long", "untracked-files", "ignore-submodules", "ignored", "column", "no-column" },
                    new()
                    {
                        { "untracked-files", new() { "no", "normal", "all" } },
                        { "ignore-submodules", new() { "none", "untracked", "dirty", "all" } }
                    }
                )
            },
            {
                "submodule",
                new GitNode(
                    "submodule",
                    shortFlags: null,
                    new() { "quiet", "cached" },
                    paramValue: null
                )
            },
            {
                "switch",
                new GitNode(
                    "switch",
                    new() { "c", "C", "d", "f", "m", "q", "t" },
                    new() { "create", "force-create", "detach", "guess", "no-guess", "force", "discard-changes", "merge", "conflict=", "quiet", "no-progress", "track", "no-track", "orphan", "ignore-other-worktrees", "recurse-submodules", "no-recurse-submodules" },
                    new()
                    {
                        { "conflict", new() { "merge", "diff3" } }
                    }
                )
            },
            {
                "tag",
                new GitNode(
                    "tag",
                    new() { "a", "s", "u", "f", "d", "v", "n", "l", "m", "F" },
                    new() { "annotate", "sign", "local-user", "force", "delete", "verify", "list", "sort", "column", "no-column", "contains", "points-at", "message", "file", "cleanup" },
                    paramValue: null
                )
            },
            {
                "whatchanged",
                new GitNode(
                    "whatchanged",
                    new() { "p" },
                    new() { "since" },
                    paramValue: null
                )
            },
            { "worktree", new GitNode(name: "worktree") }
        };

        #endregion Commands

        #region Sub-commands of some Git commands

        // Populate the sub commands for some of the git commands
        GitNode bisect = gitCommands["bisect"]!;
        bisect.SubCommands = new Dictionary<string, GitNode?>() {
            { "help", null },
            {
                "start",
                new GitNode(
                    "start",
                    shortFlags: null,
                    new() { "term-new", "term-bad", "term-old", "term-good", "no-checkout", "first-parent" },
                    paramValue: null
                )
            },
            { "bad", null },
            { "new", null },
            { "good", null },
            { "old", null },
            {
                "terms",
                new GitNode(
                    "terms",
                    shortFlags: null,
                    new() { "term-good", "term-bad" },
                    paramValue: null
                )
            },
            { "skip", null },
            { "next", null },
            { "reset", null },
            { "visualize", null },
            { "view", null },
            { "replay", null },
            { "log", null },
            { "run", null }
        };

        GitNode notes = gitCommands["notes"]!;
        notes.SubCommands = new Dictionary<string, GitNode?>() {
            { "list", null },
            {
                "add",
                new GitNode(
                    "add",
                    new() { "f", "m", "F", "c", "C" },
                    new() { "allow-empty" },
                    paramValue: null
                )
            },
            {
                "copy",
                new GitNode(
                    "copy",
                    new() { "f" },
                    longFlags: null,
                    paramValue: null
                )
            },
            {
                "append",
                new GitNode(
                    "append",
                    new() { "m", "F", "c", "C" },
                    new() { "allow-empty" },
                    paramValue: null
                )
            },
            {
                "edit",
                new GitNode(
                    "edit",
                    shortFlags: null,
                    new() { "allow-empty" },
                    paramValue: null
                )
            },
            { "show", null },
            {
                "merge",
                new GitNode(
                    "merge",
                    new() { "v", "q", "s" },
                    new() { "commit", "abort", "strategy=" },
                    new()
                    {
                        { "strategy", new() { "manual", "ours", "theirs", "union", "cat_sort_uniq" } }
                    }
                )
            },
            { "remove", null },
            {
                "prune",
                new GitNode(
                    "prune",
                    new() { "n", "V" },
                    longFlags: null,
                    paramValue: null
                )
            },
            { "get-ref", null }
        };

        GitNode reflog = gitCommands["reflog"]!;
        reflog.SubCommands = new Dictionary<string, GitNode?>() {
            {
                "show",
                new GitNode(
                    "show",
                    new() { "q", "L" },
                    new() { "quiet", "source", "use-mailmap", "mailmap", "decorate-refs", "decorate-refs-exclude", "decorate" },
                    paramValue: null
                )
            },
            {
                "expire",
                new GitNode(
                    "expire",
                    new() { "n" },
                    new() { "expire=", "expire-unreachable=", "rewrite", "updateref", "stale-fix", "dry-run", "verbose", "all" },
                    paramValue: null
                )
            },
            {
                "delete",
                new GitNode(
                    "delete",
                    new() { "n" },
                    new() { "rewrite", "updateref", "dry-run", "verbose" },
                    paramValue: null
                )
            },
            { "exists", null }
        };

        GitNode remote = gitCommands["remote"]!;
        remote.SubCommands = new Dictionary<string, GitNode?>() {
            {
                "add",
                new GitNode(
                    "add",
                    new() { "t", "m", "f" },
                    new() { "tags", "no-tags", "mirror=" },
                    new()
                    {
                        { "mirror", new() { "fetch", "push" } }
                    }
                )
            },
            { "rename", null }, // todo: can complete against remote names, for '<old>' -- "rename <old> <new>"
            { "remove", null }, // todo: can complete against remote names, for '<name>' -- "remove <name>
            {
                "set-head",
                new GitNode(
                    "set-head",
                    new() { "a", "d" },
                    new() { "auto", "delete" },
                    paramValue: null
                )
            },
            {
                "show",
                new GitNode(   // todo: can complete against remote names, for '<name>' -- "show [-n] <name>"
                    "show",
                    new() { "n" },
                    longFlags: null,
                    paramValue: null
                )
            },
            {
                "prune",
                new GitNode(   // todo: can complete against remote names, for '<name>' -- "prune <name>"
                    "prune",
                    new() { "n" },
                    new() { "dry-run" },
                    paramValue: null
                )
            },
            {
                "update",
                new GitNode(
                    "update",
                    new() { "p" },
                    new() { "prune" },
                    paramValue: null
                )
            },
            {
                "set-branches",
                new GitNode(
                    "set-branches",
                    shortFlags: null,
                    new() { "add" },
                    paramValue: null
                )
            },
            {
                "get-url",
                new GitNode(    // todo: can complete against remote names, for '<name>' -- "get-url <name>"
                    "get-url",
                    shortFlags: null,
                    new() { "push", "all" },
                    paramValue: null
                )
            },
            {
                "set-url",
                new GitNode(    // todo: can complete against remote names, for '<name>' -- "set-url <name> <url>"
                    "set-url",
                    shortFlags: null,
                    new() { "push", "add", "delete" },
                    paramValue: null
                )
            }
        };

        GitNode rerere = gitCommands["rerere"]!;
        rerere.SubCommands = new Dictionary<string, GitNode?>() {
            { "clear", null },
            { "forget", null },
            { "status", null },
            { "remaining", null },
            { "diff", null },
            { "gc", null }
        };

        GitNode stash = gitCommands["stash"]!;
        stash.SubCommands = new Dictionary<string, GitNode?>() {
            { "list", null },
            { "show", null },
            {
                "drop",
                new GitNode(
                    "drop",
                    new() { "q" },
                    new() { "quiet" },
                    paramValue: null
                )
            },
            {
                "pop",
                new GitNode(
                    "pop",
                    new() { "q" },
                    new() { "index", "quiet" },
                    paramValue: null
                )
            },
            {
                "apply",
                new GitNode(
                    "pop",
                    new() { "q" },
                    new() { "index", "quiet" },
                    paramValue: null
                )
            },
            { "branch", null },
            { "clear", null },
            {
                "push",
                new GitNode(
                    "push",
                    new() { "p", "S", "k", "q", "u", "a", "m" },
                    new() { "patch", "staged", "keep-index", "no-keep-index", "quiet", "include-untracked", "all", "message", "pathspec-from-file=", "pathspec-file-nul" },
                    paramValue: null
                )
            },
            {
                "save",
                new GitNode(
                    "save",
                    new() { "p", "S", "k", "q", "u", "a" },
                    new() { "patch", "staged", "keep-index", "no-keep-index", "quiet", "include-untracked", "all" },
                    paramValue: null
                )
            }
        };

        GitNode submodule = gitCommands["submodule"]!;
        submodule.SubCommands = new Dictionary<string, GitNode?>() {
            {
                "add",
                new GitNode(
                    "add",
                    new() { "b", "f" },
                    new() { "force", "name", "reference" },
                    paramValue: null
                )
            },
            {
                "status",
                new GitNode(
                    "status",
                    shortFlags: null,
                    new() { "cached", "recursive" },
                    paramValue: null
                )
            },
            { "init", null },
            {
                "deinit",
                new GitNode(
                    "deinit",
                    new() { "f" },
                    new() { "force", "all" },
                    paramValue: null
                )
            },
            {
                "update",
                new GitNode(
                    "update",
                    new() { "N", "f" },
                    new() { "init", "remote", "no-fetch", "force", "checkout", "merge", "rebase", "recommend-shallow", "no-recommend-shallow", "reference", "recursive", "single-branch", "no-single-branch" },
                    paramValue: null
                )
            },
            {
                "set-branch",
                new GitNode(
                    "set-branch",
                    shortFlags: null,
                    new() { "default", "branch" },
                    paramValue: null
                )
            },
            { "set-url", null },
            {
                "summary",
                new GitNode(
                    "summary",
                    shortFlags: null,
                    new() { "cached", "files", "summary-limit" },
                    paramValue: null
                )
            },
            {
                "foreach",
                new GitNode(
                    "foreach",
                    shortFlags: null,
                    new() { "recursive" },
                    paramValue: null
                )
            },
            {
                "sync",
                new GitNode(
                    "sync",
                    shortFlags: null,
                    new() { "recursive" },
                    paramValue: null
                )
            },
            { "absorbgitdirs", null }
        };

        GitNode worktree = gitCommands["worktree"]!;
        worktree.SubCommands = new Dictionary<string, GitNode?>() {
            { "add", null },
            { "list", null },
            { "lock", null },
            { "move", null },
            { "prune", null },
            { "remove", null },
            { "unlock", null }
        };

        #endregion Sub-commands of some Git commands

        #region Argument completion

        #endregion Argument completion

        return gitCommands;
    }
}
