using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax;
using Roslyn.Utilities;

#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.Lua.LuaSyntaxTree;
using InternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxTree;
using InternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.MoonScriptSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

#if LANG_LUA
/// <summary>
/// 表示语法树中的非终结节点。此节点类仅针对于Lua语言构造。
/// </summary>
#elif LANG_MOONSCRIPT
/// <summary>
/// 表示语法树中的非终结节点。此节点类仅针对于MoonScript语言构造。
/// </summary>
#endif
public abstract partial class
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          : SyntaxNode, IFormattable
{
    internal new SyntaxTree SyntaxTree =>
        this._syntaxTree ?? ThisSyntaxNode.ComputeSyntaxTree(this);

    internal new ThisSyntaxNode? Parent => (ThisSyntaxNode?)base.Parent;

    internal new ThisSyntaxNode? ParentOrStructuredTriviaParent => (ThisSyntaxNode?)base.ParentOrStructuredTriviaParent;

    public override string Language => this.Green.Language;

    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          (GreenNode green, SyntaxNode? parent, int position) : base(green, parent, position) { }

    /// <summary>
    /// 仅用于语法琐碎内容，因为它们不会作为节点的子级，即父节点为<see langword="null"/>，因此构造时需要明确指明所在的语法树。
    /// </summary>
    /// <param name="green"></param>
    /// <param name="position"></param>
    /// <param name="syntaxTree"></param>
    internal
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
          (GreenNode green, int position, SyntaxTree syntaxTree) : base(green, position, syntaxTree) { }

    private static SyntaxTree ComputeSyntaxTree(ThisSyntaxNode node)
    {
        ArrayBuilder<ThisSyntaxNode>? nodes = null;
        SyntaxTree? tree = null;

        // 查找最近的具有非空语法树的父节点。
        while (true)
        {
            tree = node._syntaxTree;
            if (tree is not null) break; // 节点自身的语法树非空。

            var parent = node.Parent;
            if (parent is null) // 节点自身即为根节点。
            {
                // 原子操作设置语法树到根节点。
                Interlocked.Exchange(ref node._syntaxTree, ThisSyntaxTree.CreateWithoutClone(node));
                tree = node._syntaxTree;
                break;
            }

            tree = parent._syntaxTree;
            if (tree is not null)
            {
                // 将父节点的语法树设置到节点自身上。
                node._syntaxTree = tree;
                break;
            }

            (nodes ??= ArrayBuilder<ThisSyntaxNode>.GetInstance()).Add(node);
            node = parent;
        }

        // 自上而下传递语法树。
        if (nodes is not null)
        {
            Debug.Assert(tree is not null);

            foreach (var n in nodes)
            {
                var existingTree = n._syntaxTree;
                if (existingTree is not null)
                {
                    Debug.Assert(existingTree == tree, "至少有一个节点位于其他语法树。");
                    break;
                }

                n._syntaxTree = tree;
            }

            nodes.Free();
        }

        Debug.Assert(tree is not null);
        return tree;
    }

    public abstract TResult? Accept<TResult>(
#if LANG_LUA
        LuaSyntaxVisitor<TResult>
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor<TResult>
#endif
        visitor);

    public abstract void Accept(
#if LANG_LUA
        LuaSyntaxVisitor
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxVisitor
#endif
         visitor);

#region 序列化
    public static SyntaxNode DeserializeFrom(Stream stream!!, CancellationToken cancellationToken = default)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeReadFrom);

        using var reader = ObjectReader.TryGetReader(stream, leaveOpen: true, cancellationToken);

        if (reader is null)
            throw new ArgumentException(CodeAnalysisResources.Stream_contains_invalid_data, nameof(stream));

        var root = (InternalSyntaxNode)reader.ReadValue();
        return root.CreateRed();
    }
#endregion

    public new Location GetLocation() => new SourceLocation(this);

    string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => this.ToString();
}
