namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class SyntaxParser
{
    protected virtual partial SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
    {
        var code = SyntaxParser.GetExpectedTokenErrorCode(expected, actual);
        return code switch
        {
            ErrorCode.ERR_SyntaxError =>
                new(offset, width, code, SyntaxFacts.GetText(expected)),
#warning 需完善错误码到语法诊断信息实例的映射。
            _ =>
            new(offset, width, code)
        };
    }

    /// <summary>
    /// 获取一个错误码，对应未得到期望的标志。
    /// </summary>
    /// <returns>对应未得到期望的标志的错误码。</returns>
    /// <inheritdoc cref="SyntaxParser.GetExpectedTokenError(SyntaxKind, SyntaxKind)"/>
    private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual) =>
        expected switch
        {
#warning 需完善未得到期望的标志对应的错误码。
            _ => ErrorCode.ERR_SyntaxError
        };
}
