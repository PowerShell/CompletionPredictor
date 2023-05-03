# Changelog

### [0.1.1] - 2023-05-03

- Set `tooltip` only if it's different from the completion text (#26)
- Move the git handler into its own folder (#25)
- Make ad-hoc fixes for issues found out while using the module (#23)
- Add argument prediction for some basic git command (#21)
- Allow argument completion on `cd` and `dir` (#20, #27)
- Set the predictor Runspace name (#16) (Thanks @ThomasNieto!)
- Update .psd1 file to include the project uri (#18)

[0.1.1]: https://github.com/PowerShell/PSReadLine/compare/v0.1.0...v0.1.1

## [0.1.0] - 2022-04-06

Initial release of the `CompletionPredictor` module:
1. Provides prediction results based on tab completion of the user's input using a separate Runspace.
1. Enable syncing some states between the PowerShell console default Runspace and the separate Runspace, including the current working directory, variables, and loaded modules.

Known limitations:
1. Prediction on command names is currently disabled because tab completion on command names usually exceeds the timeout limit set by PSReadLine for the predictive intellisense feature, which is 20ms. Different approaches will need to be explored for this, such as
   - building an index for available commands like the module analysis cache, or
   - reusing tab completion results in certain cases, so further user input will be used to filter the existing tab completion results instead of always triggering new tab completion requests.
1. Prediction on command arguments is currently disabled due to the same reason.
   - the default argument completion action is to enumerate file system items, which is slow in our current implementation. But this can be improved by special case the file system provider, so as to call .NET APIs directly when operating in the `FileSystemProvider`.
   - some custom argument completers are slow, especially those for native commands as they usually have to start an external process. This can potentially be improved by building index for common native commands.
