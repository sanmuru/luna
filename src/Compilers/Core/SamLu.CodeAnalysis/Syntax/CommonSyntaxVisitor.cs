using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis;

/// <summary>
/// 表示通用的访问者基类。访问者每次访问和处理一个类型为<typeparamref name="TNode"/>的节点并产生类型为<typeparamref name="TResult"/>的结果。
/// </summary>
/// <typeparam name="TResult">访问者的处理方法的返回结果的类型。</typeparam>
/// <typeparam name="TNode">访问和处理的节点的类型</typeparam>
public abstract class CommonSyntaxVisitor<TResult, TNode>
    where TNode : SyntaxNode
{
    /// <summary>
    /// 处理这个节点并产生结果。
    /// </summary>
    /// <param name="node">要进行处理的节点。</param>
    /// <returns>产生的结果。</returns>
    public abstract TResult? Visit(TNode? node);

    /// <summary>
    /// 内部方法，调用其他方法处理这个节点并产生结果。
    /// </summary>
    /// <param name="node">要进行处理的节点。</param>
    /// <returns>产生的结果。</returns>
    protected virtual TResult? DefaultVisit(TNode node) => this.Visit(node);
}

/// <summary>
/// 表示通用的访问者基类。访问者每次访问和处理一个类型为<typeparamref name="TNode"/>的节点。
/// </summary>
/// <typeparam name="TNode">访问和处理的节点的类型</typeparam>
public abstract class CommonSyntaxVisitor<TNode>
{
    /// <summary>
    /// 处理这个节点。
    /// </summary>
    /// <param name="node">要进行处理的节点。</param>
    public abstract void Visit(TNode? node);

    /// <summary>
    /// 内部方法，调用其他方法处理这个节点。
    /// </summary>
    /// <param name="node">要进行处理的节点。</param>
    protected abstract void DefaultVisit(TNode node);
}
