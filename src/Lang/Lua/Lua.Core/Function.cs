using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SamLu.Lua;

public sealed class Function : Object
{
    private readonly Delegate _func;

    public Function(Delegate func!!) => this._func = func;

    public override MultiReturns Invoke(params Object?[] args!!)
    {
        var mi = this._func.Method;
        bool hasReturnValue = mi.ReturnType == typeof(void);
        var parameterTypes = mi.GetParameters().Select(pi => pi.ParameterType).ToArray();

        int oldLength = args.Length;
        int newLength = parameterTypes.Length;
        object?[] newArgs = new object?[newLength];
        int count = Math.Min(oldLength, newLength); // 要处理的参数个数。
        for (int i = 0; i < count; i++)
        {
            Object? oldValue = args[i];
            if (oldValue is null) continue; // 不处理空值。

            object newValue = oldValue.ChangeType(parameterTypes[i]);
            newArgs[i] = newValue;
        }

        object? result = _func.DynamicInvoke(newArgs); // 调用委托以获取结果。
        if (!hasReturnValue)
            return MultiReturns.Empty;
        else if (result is MultiReturns)
            return (MultiReturns)result;
        else
            return new(Object.ConvertFrom(result));
    }

    #region Object
    internal static Table? s_mt;

    protected internal override Table? Metatable
    {
        get => Function.s_mt;
        set => Function.s_mt = value;
    }

    public override TypeInfo GetTypeInfo() => TypeInfo.Function;
    #endregion

    public static implicit operator Function(Delegate func!!) => new(func);
    public static explicit operator Delegate(Function func!!) => func._func;
}
