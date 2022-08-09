namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class LanguageParser
{
    [Flags]
    internal enum TerminatorState
    {
        EndOfFile = 0,
#warning 未完成。
    }

    private const int LastTerminatorState = (int)TerminatorState.EndOfFile;

    private partial bool IsTerminalCore(TerminatorState state) => state switch
    {
#warning 未完成。
        _ => false
    };
}
