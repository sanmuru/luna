using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    .Syntax.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class
#if LANG_LUA
    LuaSyntaxNode
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxNode
#endif
    : GreenNode
{
    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind) : base((ushort)kind) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, int fullWidth) : base((ushort)kind, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics) : base((ushort)kind, diagnostics) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth) : base((ushort)kind, diagnostics, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations) : base((ushort)kind, diagnostics, annotations) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth) : base((ushort)kind, diagnostics, annotations, fullWidth) => GreenStats.NoteGreen(this);

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
        (ObjectReader reader) : base(reader) { }

    public sealed override string Language =>
#if LANG_LUA
        LanguageNames.Lua;
#elif LANG_MOONSCRIPT
        LanguageNames.MoonScript
#endif

    public SyntaxKind Kind => (SyntaxKind)this.RawKind;

    public override string KindText => this.Kind.ToString();

    public override int RawContextualKind => this.RawKind;

    public override bool IsStructuredTrivia => this is StructuredTriviaSyntax;

    public override bool IsDirective => this is DirectiveTriviaSyntax;

    public override bool IsDocumentationCommentTrivia => SyntaxFacts.IsDocumentationCommentTrivia(this.Kind);

    public SyntaxToken? GetFirstToken() => (SyntaxToken?)this.GetFirstTerminal();

    public SyntaxToken? GetLastToken() => (SyntaxToken?)this.GetLastTerminal();

    public SyntaxToken? GetLastNonmissingToken() => (SyntaxToken?)this.GetLastNonmissingTerminal();

    public virtual GreenNode? GetLeadingTrivia() => null;
    public sealed override GreenNode? GetLeadingTriviaCore() => this.GetLeadingTrivia();

    public virtual GreenNode? GetTrailingTrivia() => null;
    public sealed override GreenNode? GetTrailingTriviaCore() => this.GetTrailingTrivia();

    public abstract TResult Accept<TResult>(LuaSyntaxVisitor<TResult> visitor);

    public abstract void Accept(LuaSyntaxVisitor visitor);

#warning SetFactoryContext

    public override Microsoft.CodeAnalysis.SyntaxToken CreateSeparator<TNode>(SyntaxNode element) =>
#if LANG_LUA
        Lua.SyntaxFactory.Token(SyntaxKind.CommanToken);
#elif LANG_MOONSCRIPT
#error 未实现
#endif

    public override bool IsTriviaWithEndOfLine() =>
        this.Kind switch
        {
            SyntaxKind.EndOfLineTrivia or
            SyntaxKind.SingleLineCommentTrivia => true,
            _ => false
        };

    /// <summary>
    /// 使用条件弱表来保存与语法节点相对应的结构语法琐碎内容的唯一实例。
    /// </summary>
    private static readonly ConditionalWeakTable<SyntaxNode, Dictionary<Microsoft.CodeAnalysis.SyntaxTrivia, WeakReference<SyntaxNode>>> s_structuresTable = new();

    public override SyntaxNode? GetStructure(Microsoft.CodeAnalysis.SyntaxTrivia trivia)
    {
        if (trivia.HasStructure)
        {
            var parent = trivia.Token.Parent;
            if (parent is null)
                return this.GetNewStructure(trivia);
            else
            {
                SyntaxNode? structure;
                var structsInParent = s_structuresTable.GetOrCreateValue(parent);
                lock (structsInParent)
                {
                    if (!structsInParent.TryGetValue(trivia, out var weekStructure))
                    {
                        structure = this.GetNewStructure(trivia);
                        structsInParent.Add(trivia, new(structure));
                    }
                    else if (!weekStructure.TryGetTarget(out structure))
                    {
                        structure = this.GetNewStructure(trivia);
                        structsInParent.Add(trivia, new(structure));
                    }
                }

                return structure;
            }
        }
        else return null;
    }

#if LANG_LUA
    /// <summary>
    /// 使用指定语法琐碎内容创建新的<see cref="Lua.Syntax.StructuredTriviaSyntax"/>实例。
    /// </summary>
#elif LANG_MOONSCRIPT
    /// <summary>
    /// 使用指定语法琐碎内容创建新的<see cref="MoonScript.Syntax.StructuredTriviaSyntax"/>实例。
    /// </summary>
#endif
    /// <param name="trivia">现有的语法琐碎内容。</param>
    /// <returns>根据现有的语法琐碎内容创建的新的表示结构语法琐碎内容的语法节点。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SyntaxNode GetNewStructure(Microsoft.CodeAnalysis.SyntaxTrivia trivia) =>
#if LANG_LUA
        Lua
#elif LANG_MOONSCRIPT
        MoonScript
#endif
        .Syntax.StructuredTriviaSyntax.Create(trivia);

#warning 未完成
}
