using System.Diagnostics;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class SyntaxFactoryContext
{
    private int _ifBlockDepth;

    internal bool IsInIfBlock => this._ifBlockDepth != 0;

    internal void EnterIfBlock() => this._ifBlockDepth++;

    internal void LeaveIfBlock()
    {
        Debug.Assert(this._ifBlockDepth > 0);
        this._ifBlockDepth--;
    }
}
