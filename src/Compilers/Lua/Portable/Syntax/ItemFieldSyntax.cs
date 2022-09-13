namespace SamLu.CodeAnalysis.Lua.Syntax;

partial class ItemFieldSyntax
{
    public override ExpressionSyntax? FieldKey => null;

    internal override FieldSyntax WithFieldKeyCore(ExpressionSyntax? fieldKey)
    {
        if (fieldKey is null)
            return SyntaxFactory.ItemField(this.FieldValue);
        else
            return SyntaxFactory.KeyValueField(fieldKey, this.FieldValue);
    }
}
