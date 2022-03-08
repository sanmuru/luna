using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax
{
    /// <summary>
    /// 表示语法树中的非终结节点。此节点类仅针对于Lua语言构造。
    /// </summary>
    public abstract class LuaSyntaxNode : SyntaxNode, IFormattable
    {
        internal new SyntaxTree SyntaxTree
        {
            get
            {
                var result = this._syntaxTree ?? LuaSyntaxNode.ComputeSyntaxTree(this);
                Debug.Assert(result is not null);
                return result;
            }
        }

        internal new LuaSyntaxNode? Parent => (LuaSyntaxNode?)base.Parent;

        internal new LuaSyntaxNode? ParentOrStructuredTriviaParent => (LuaSyntaxNode?)base.ParentOrStructuredTriviaParent;

        internal InternalSyntax.LuaSyntaxNode LuaGreen => (InternalSyntax.LuaSyntaxNode)this.Green;

        public SyntaxKind Kine() => this.LuaGreen.Kind;

        public override string Language => this.Green.Language;

        internal LuaSyntaxNode(GreenNode green, SyntaxNode? parent, int position) : base(green, parent, position) { }

        /// <summary>
        /// 仅用于语法琐碎内容，因为它们不会作为节点的子级，即父节点为<see langword="null"/>，因此构造时需要明确指明所在的语法树。
        /// </summary>
        /// <param name="green"></param>
        /// <param name="position"></param>
        /// <param name="syntaxTree"></param>
        internal LuaSyntaxNode(GreenNode green, int position, SyntaxTree syntaxTree) : base(green, position, syntaxTree) { }

        private static SyntaxTree ComputeSyntaxTree(LuaSyntaxNode node)
        {
            ArrayBuilder<LuaSyntaxNode>? nodes = null;
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
                    Interlocked.Exchange(ref node._syntaxTree, LuaSyntaxTree.CreateWithoutClone(node));
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

                (nodes ??= ArrayBuilder<LuaSyntaxNode>.GetInstance()).Add(node);
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

            return tree;
        }

        public abstract TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor);

        public abstract void Accept(LuaSyntaxVisitor visitor);

        #region 序列化
        public static SyntaxNode DeserializeFrom(Stream stream!!, CancellationToken cancellationToken = default)
        {
            if (!stream.CanRead)
                throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeReadFrom);

            using var reader = ObjectReader.TryGetReader(stream, leaveOpen: true, cancellationToken);

            if (reader is null)
                throw new ArgumentException(CodeAnalysisResources.Stream_contains_invalid_data, nameof(stream));

            var root = (InternalSyntax.LuaSyntaxNode)reader.ReadValue();
            return root.CreateRed();
        }
        #endregion

        public new Location GetLocation() => new SourceLocation(this);

        string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => this.ToString();
    }
}
