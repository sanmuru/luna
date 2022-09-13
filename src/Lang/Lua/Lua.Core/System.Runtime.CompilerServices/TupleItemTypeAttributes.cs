namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = true)]
public sealed class TupleItemTypeAttribute : Attribute
{
    public int ItemIndex { get; }
    public Type? ItemType { get; }

    public TupleItemTypeAttribute(int itemIndex, Type itemType)
    {
        this.ItemIndex = itemIndex;
        this.ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
    }
}

[AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = true)]
public sealed class TupleItemTypeSameAsAttribute : Attribute
{
    public int ItemIndex { get; }
    public string ParamName { get; }

    public TupleItemTypeSameAsAttribute(int itemIndex, string paramName)
    {
        this.ItemIndex= itemIndex;
        this.ParamName = paramName ?? throw new ArgumentNullException(nameof(paramName));
    }
}
