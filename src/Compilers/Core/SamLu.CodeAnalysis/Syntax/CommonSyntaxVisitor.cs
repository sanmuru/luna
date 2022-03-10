using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis;

/// <summary>
/// 表示每个时刻仅访问和处理一个<typeparamref name="TNode"/>并产生类型为<typeparamref name="TResult"/>的结果的访问者。
/// </summary>
/// <typeparam name="TResult">访问者的处理方法的返回结果的类型。</typeparam>
/// <typeparam name="TNode">访问者的<see cref="SyntaxNode"/>的类型</typeparam>
public abstract class CommonSyntaxVisitor<TResult, TNode>
    where TNode : SyntaxNode
{
    public abstract TResult? Visit(TNode? node);

    public abstract TResult? DefaultVisit(TNode node);
}

/// <summary>
/// 表示每个时刻仅访问和处理一个<typeparamref name="TNode"/>的访问者。
/// </summary>
public abstract class CommonSyntaxVisitor<TNode>
{
    public abstract void Visit(TNode? node);

    public abstract void DefaultVisit(TNode node);
}
