namespace System.Runtime.CompilerServices;

/// <summary>
/// 表明附加了此特性的方法的<see langword="dynamic"/>数组返回值为Lua的复数返回值
/// </summary>
[AttributeUsage(AttributeTargets.ReturnValue)]
public sealed class TupleAttribute : Attribute
{
    public int Length { get; }

    public TupleAttribute(int length) => this.Length = length;
}
