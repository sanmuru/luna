namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class Lexer
{
    private partial SyntaxTrivia ScanComment()
    {
        if (this.ScanLongBrackets(out bool isTerminal))
        {
            if (!isTerminal)
                this.AddError(ErrorCode.ERR_OpenEndedComment);
        }
        else
            this.ScanToEndOfLine(isTrim: true);

        var text = this.TextWindow.GetText(intern: false);
        return SyntaxFactory.Comment(text);
    }
}
