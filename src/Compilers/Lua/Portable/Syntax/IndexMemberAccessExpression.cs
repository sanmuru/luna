namespace SamLu.CodeAnalysis.Lua.Syntax;

partial class IndexMemberAccessExpressionSyntax
{
    public override ExpressionSyntax MemberExpression => this.Key.Expression;

    internal override MemberAccessExpressionSyntax WithMemberExpressionCore(ExpressionSyntax memberExpression) =>
        this.WithKey(SyntaxFactory.BracketedExpression(memberExpression));
}
