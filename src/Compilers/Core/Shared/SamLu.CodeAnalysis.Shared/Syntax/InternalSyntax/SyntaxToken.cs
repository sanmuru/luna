
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

#if LANG_LUA
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
#elif LANG_MOONSCRIPT
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
#endif

internal partial class SyntaxToken : ThisInternalSyntaxNode
{
    public virtual SyntaxKind ContextualKind => this.Kind;
    public sealed override int RawContextualKind => (int)this.ContextualKind;

    public virtual string Text => SyntaxFacts.GetText(this.Kind);
    public virtual string ValueText => this.Text;

    public override int Width => this.Text.Length;

    internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ThisInternalSyntaxNode> LeadingTrivia => new(this.GetLeadingTrivia());

    internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ThisInternalSyntaxNode> TrailingTrivia => new(this.GetTrailingTrivia());

    public sealed override bool IsToken => true;

    #region 构造函数
    internal SyntaxToken(SyntaxKind kind) : base(kind)
    {
        this.SetFullWidth();
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics) : base(kind, diagnostics)
    {
        this.SetFullWidth();
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations) : base(kind, diagnostics, annotations)
    {
        this.SetFullWidth();
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(SyntaxKind kind, int fullWidth) : base(kind, fullWidth)
    {
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[]? diagnostics) : base(kind, diagnostics, fullWidth)
    {
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations) : base(kind, diagnostics, annotations, fullWidth)
    {
        this.SetIsNotMissingFlag();
    }

    internal SyntaxToken(ObjectReader reader) : base(reader)
    {
        var text = this.Text;
        if (text is not null) this.FullWidth = text.Length;

        this.SetIsNotMissingFlag();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetFullWidth() => this.FullWidth = this.Text.Length;

    /// <summary>
    /// 在此基类上添加<see cref="Microsoft.CodeAnalysis.GreenNode.NodeFlags.IsNotMissing"/>标志。若子类要表示缺失的语法标识，则在子类中移除这个标志。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetIsNotMissingFlag() => this.SetFlags(NodeFlags.IsNotMissing);
    #endregion

    /// <summary>
    /// 表示语法标识在序列化过程中是否应被重用。
    /// </summary>
    internal override bool ShouldReuseInSerialization => base.ShouldReuseInSerialization &&
        // 同时不应超过词法器的最大缓存标识空间。
        this.FullWidth < Lexer.MaxCachedTokenSize;

    /// <exception cref="InvalidOperationException">此方法永远不会被调用。</exception>
    internal sealed override GreenNode? GetSlot(int index) => throw ExceptionUtilities.Unreachable;

    #region 这些成员在各语言的独立项目中定义
    // FirstTokenWithWellKnownText常量
    // LastTokenWithWellKnownText常量
    // s_tokensWithNoTrivia字段
    // s_tokensWithElasticTrivia字段
    // s_tokensWithSingleTrailingSpace字段
    // s_tokensWithSingleTrailingCRLF字段
    #endregion

    internal static SyntaxToken Create(SyntaxKind kind)
    {
        if (kind > SyntaxToken.LastTokenWithWellKnownText)
        {
            if (!SyntaxFacts.IsAnyToken(kind))
                throw new ArgumentException(string.Format(LunaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
            else
                return SyntaxToken.CreateMissing(kind, null, null);
        }

        return SyntaxToken.s_tokensWithNoTrivia[(int)kind].Value;
    }

    internal static SyntaxToken Create(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
    {
        if (kind > SyntaxToken.LastTokenWithWellKnownText)
        {
            if (!SyntaxFacts.IsAnyToken(kind))
                throw new ArgumentException(string.Format(LunaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
            else
                return SyntaxToken.CreateMissing(kind, null, null);
        }

        if (leading is null)
        {
            if (trailing is null)
                return SyntaxToken.s_tokensWithNoTrivia[(int)kind].Value;
            else if (trailing == SyntaxFactory.Space)
                return SyntaxToken.s_tokensWithSingleTrailingSpace[(int)kind].Value;
            else if (trailing == SyntaxFactory.CarriageReturnLineFeed)
                return SyntaxToken.s_tokensWithSingleTrailingCRLF[(int)kind].Value;
        }
        else if (leading == SyntaxFactory.ElasticZeroSpace && trailing == SyntaxFactory.ElasticZeroSpace)
            return SyntaxToken.s_tokensWithElasticTrivia[(int)kind].Value;

        return new SyntaxTokenWithTrivia(kind, leading, trailing);
    }

    internal static SyntaxToken CreateMissing(SyntaxKind kind, GreenNode? leading, GreenNode? trailing) => new MissingTokenWithTrivia(kind, leading, trailing);

    public override object? GetValue() => this.Value;

    public override string GetValueText() => this.ValueText;

    public override int GetLeadingTriviaWidth()
    {
        var leading = this.GetLeadingTrivia();
        return leading is null ? 0 : leading.FullWidth;
    }

    public override int GetTrailingTriviaWidth()
    {
        var trailing = this.GetTrailingTrivia();
        return trailing is null ? 0 : trailing.FullWidth;
    }

    public override GreenNode WithLeadingTrivia(GreenNode? trivia) => this.TokenWithLeadingTrivia(trivia);

    public virtual SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia) =>
        new SyntaxTokenWithTrivia(this.Kind, trivia, null, this.GetDiagnostics(), this.GetAnnotations());

    public override GreenNode WithTrailingTrivia(GreenNode? trivia) => this.TokenWithTrailingTrivia(trivia);

    public virtual SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia) =>
        new SyntaxTokenWithTrivia(this.Kind, null, trivia, this.GetDiagnostics(), this.GetAnnotations());

    internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
    {
        Debug.Assert(this.GetType() == typeof(SyntaxToken));
        return new SyntaxToken(this.Kind, this.FullWidth, diagnostics, this.GetAnnotations());
    }

    internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
    {
        Debug.Assert(this.GetType() == typeof(SyntaxToken));
        return new SyntaxToken(this.Kind, this.FullWidth, this.GetDiagnostics(), annotations);
    }

    #region 访问方法
    public override TResult Accept<TResult>(
#if LANG_LUA
        LuaSyntaxVisitor<TResult>
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor<TResult>
#endif
        visitor) => visitor.VisitToken(this);

    public override void Accept(
#if LANG_LUA
        LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor
#endif
        visitor) => visitor.VisitToken(this);
    #endregion

    protected override void WriteTokenTo(TextWriter writer, bool leading, bool trailing)
    {
        if (leading) this.GetLeadingTrivia()?.WriteTo(writer, true, true);

        writer.Write(this.Text);

        if (trailing) this.GetTrailingTrivia()?.WriteTo(writer, true, true);
    }

    public override bool IsEquivalentTo([NotNullWhen(true)] GreenNode? other)
    {
        if (!base.IsEquivalentTo(other)) return false;

        var otherToken = (SyntaxToken)other;
        if (this.Text != otherToken.Text) return false;

        var thisLeading = this.GetLeadingTrivia();
        var otherLeading = otherToken.GetLeadingTrivia();
        if (thisLeading != otherLeading)
        {
            if (thisLeading is null || otherLeading is null) return false;
            else if (!thisLeading.IsEquivalentTo(otherLeading)) return false;
        }

        var thisTrailing = this.GetTrailingTrivia();
        var otherTrailing = otherToken.GetTrailingTrivia();
        if (thisTrailing != otherTrailing)
        {
            if (thisTrailing is null || otherTrailing is null) return false;
            else if (!thisTrailing.IsEquivalentTo(otherTrailing)) return false;
        }

        return true;
    }

    /// <exception cref="InvalidOperationException">此方法永远不会被调用。</exception>
    internal sealed override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw ExceptionUtilities.Unreachable;

    public override string ToString() => this.Text;
}
