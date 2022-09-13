namespace SamLu.CodeAnalysis.Lua.Syntax;

partial class SimpleMemberAccessExpressionSyntax
{
    public override ExpressionSyntax Member => this.MemberName;

    internal override MemberAccessExpressionSyntax WithMemberCore(ExpressionSyntax member)
    {
        if (member is IdentifierNameSyntax memberName)
            return this.WithMemberName(memberName);
        else
            return SyntaxFactory.IndexMemberAccessExpression(this.Self, member);
    }
}
