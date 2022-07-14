namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class IndexMemberAccessExpressionSyntax
{
    public override ExpressionSyntax MemberExpression => this.Key.Expression;
}
