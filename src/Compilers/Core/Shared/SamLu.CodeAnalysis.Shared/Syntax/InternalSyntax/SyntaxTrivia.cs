using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

internal class SyntaxTrivia :
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
{
    public readonly string Text;

    static SyntaxTrivia()
    {
        ObjectBinder.RegisterTypeReader(typeof(SyntaxTrivia), r => new SyntaxTrivia(r));
    }

    internal SyntaxTrivia(
        SyntaxKind kind,
        string text,
        DiagnosticInfo[]? diagnostics = null,
        SyntaxAnnotation[]? annotations = null) : base(kind, diagnostics, annotations, text.Length)
    {
        this.Text = text;
    }

    internal SyntaxTrivia(ObjectReader reader) : base(reader)
    {
        this.Text = reader.ReadString();
        this.FullWidth = this.Text.Length;
    }

    /// <summary>此语法节点是否为指令。</summary>
    /// <remarks>此属性的值永远为<see langword="false"/>。</remarks>
    public sealed override bool IsDirective => false;

    /// <summary此语法节点是否为标识。</summary>
    /// <remarks>此属性的值永远为<see langword="false"/>。</remarks>
    public sealed override bool IsToken => false;

    /// <summary>此语法节点是否为琐碎内容。</summary>
    /// <remarks>此属性的值永远为<see langword="true"/>。</remarks>
    public sealed override bool IsTrivia => true;

#warning 未完成

    internal static SyntaxTrivia Create(SyntaxKind kind, string text) => new(kind, text);

    public static implicit operator Microsoft.CodeAnalysis.SyntaxTrivia(SyntaxTrivia trivia) =>
        new(token: default, trivia, position: 0, index: 0);
}
