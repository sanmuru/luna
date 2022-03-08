using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua.Syntax
{
    /// <summary>
    /// 表示每个时刻仅访问和处理一个<see cref="LuaSyntaxNode"/>并产生类型为<typeparamref name="TResult"/>的结果的访问者。
    /// </summary>
    /// <typeparam name="TResult">访问者的处理方法的返回结果的类型</typeparam>
    public abstract partial class LuaSyntaxVisitor<TResult>
    {
        public virtual TResult? Visit(SyntaxNode? node) =>
            (node is not null) ?
                ((LuaSyntaxNode)node).Accept(this) :
                // 兜底值。
                default;

        public virtual TResult? DefaultVisit(SyntaxNode node) => default;
    }

    /// <summary>
    /// 表示每个时刻仅访问和处理一个<see cref="LuaSyntaxNode"/>的访问者。
    /// </summary>
    public abstract partial class LuaSyntaxVisitor
    {
        public virtual void Visit(SyntaxNode? node)
        {
            if (node is not null)
                ((LuaSyntaxNode)node).Accept(this);
        }

        public virtual void DefaultVisit(SyntaxNode node) { }
    }
}
