namespace SamLu.CodeAnalysis.Lua.Syntax;

partial class NameValueFieldSyntax
{
    public override ExpressionSyntax? FieldKey => this.FieldName;

    internal override FieldSyntax WithFieldKeyCore(ExpressionSyntax? fieldKey)
    {
        if (fieldKey is null)
            return SyntaxFactory.ItemField(this.FieldValue);
        else if (fieldKey is IdentifierNameSyntax fieldName)
            return this.WithFieldName(fieldName);
        else
            return SyntaxFactory.KeyValueField(fieldKey, this.FieldValue);
    }
}
