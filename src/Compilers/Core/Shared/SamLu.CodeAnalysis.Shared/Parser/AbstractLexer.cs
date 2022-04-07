using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax
#endif

internal abstract partial class AbstractLexer : IDisposable
{
    internal readonly SlidingTextWindow TextWindow;
    private List<SyntaxDiagnosticInfo>? _errors;

    [MemberNotNullWhen(true, nameof(AbstractLexer._errors))]
    protected bool HasErros => this._errors is not null;

    protected AbstractLexer(SourceText text) => this.TextWindow = new(text);

    public virtual void Dispose() => this.TextWindow.Dispose();

    protected void Start()
    {
        this.TextWindow.Start();
        this._errors = null;
    }

    protected SyntaxDiagnosticInfo[]? GetErrors(int leadingTriviaWidth)
    {
        if (!this.HasErros) return null;

        if (leadingTriviaWidth > 0)
        {
            // 调整偏移量，加上起始语法琐碎内容的宽度。
            var array = new SyntaxDiagnosticInfo[this._errors.Count];
            for (int i = 0; i < this._errors.Count; i++)
                array[i] = this._errors[i].WithOffset(this._errors[i].Offset + leadingTriviaWidth);

            return array;
        }
        else return this._errors.ToArray();
    }

    protected void AddError(int position, int width, ErrorCode code) => this.AddError(this.MakeError(position, width, code));

    protected void AddError(int position, int width, ErrorCode code, params object[] args) => this.AddError(this.MakeError(position, width, code, args));

    protected void AddError(ErrorCode code) => this.AddError(AbstractLexer.MakeError(code));

    protected void AddError(ErrorCode code, params object[] args) => this.AddError(AbstractLexer.MakeError(code, args));

    protected void AddError(SyntaxDiagnosticInfo? error)
    {
        if (error is null) return;

        if (!this.HasErros) this._errors = new(8);

        this._errors.Add(error);
    }

    protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code) => new(this.GetLexemeOffsetFromPosition(position), width, code);

    protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args) => new(this.GetLexemeOffsetFromPosition(position), width, code, args);

    private int GetLexemeOffsetFromPosition(int position) => position >= this.TextWindow.LexemeStartPosition ? position - TextWindow.LexemeStartPosition : position;

    protected static SyntaxDiagnosticInfo MakeError(ErrorCode code) => new(code);

    protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args) => new(code, args);
}
