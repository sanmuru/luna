namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class SyntaxFactoryContext
{
    internal bool IsInIfOrElseIf => this.CurrentStructure is SyntaxKind.IfStatement or SyntaxKind.ElseIfClause;
}
