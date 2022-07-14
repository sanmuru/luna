namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    private partial SyntaxTrivia ScanComment()
    {
        if (this.ScanLongBrackets(out bool isTerminal))
        {
            this.CheckFeatureAvaliability(MessageID.IDS_FeatureMultiLineComment);
            if (!isTerminal)
                this.AddError(ErrorCode.ERR_OpenEndedComment);
        }
        else
            this.ScanToEndOfLine(isTrim: true);

        var text = this.TextWindow.GetText(intern: false);
        return SyntaxFactory.Comment(text);
    }
}
