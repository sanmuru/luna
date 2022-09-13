using System.Diagnostics;

namespace SamLu.Lua;

public sealed class TypeInfo : Object, IEquatable<TypeInfo>, IEquatable<String>, IEquatable<string>
{
    internal readonly Type? _type;
    private readonly string _name;

    public string Name => this._name;

    public const string TypeInfo_Nil = "nil";
    public const string TypeInfo_Boolean = "boolean";
    public const string TypeInfo_Number = "number";
    public const string TypeInfo_String = "string";
    public const string TypeInfo_Function = "function";
    public const string TypeInfo_Userdata = "userdata";
    public const string TypeInfo_Thread = "thread";
    public const string TypeInfo_Table = "dictionary";

    public static readonly TypeInfo Nil = new(BasicType.Nil);
    public static readonly TypeInfo Boolean = new(BasicType.Boolean);
    public static readonly TypeInfo Number = new(BasicType.Number);
    public static readonly TypeInfo String = new(BasicType.String);
    public static readonly TypeInfo Function = new(BasicType.Function);
    public static readonly TypeInfo Userdata = new(BasicType.Userdata);
    public static readonly TypeInfo Thread = new(BasicType.Thread);
    public static readonly TypeInfo Table = new(BasicType.Table);

    internal TypeInfo(string name)
    {
        Debug.Assert(name is not null);
        this._name = name;
        this._type = name switch
        {
            TypeInfo_Nil => null,
            TypeInfo_Boolean => typeof(Boolean),
            TypeInfo_Number => typeof(Number),
            TypeInfo_String => typeof(String),
            TypeInfo_Function => typeof(Function),
            TypeInfo_Userdata => typeof(Userdata),
            TypeInfo_Thread => typeof(Thread),
            TypeInfo_Table => typeof(Table),
            _ => null
        };
    }

    internal TypeInfo(Type? type)
    {
        this._type = type;
        if (type is null)
            this._name = TypeInfo_Nil;
        else if (type == typeof(Boolean))
            this._name = TypeInfo_Boolean;
        else if (type == typeof(Number))
            this._name = TypeInfo_Number;
        else if (type == typeof(String))
            this._name = TypeInfo_String;
        else if (type == typeof(Function))
            this._name = TypeInfo_Function;
        else if (type == typeof(Userdata))
            this._name = TypeInfo_Userdata;
        else if (type == typeof(Thread))
            this._name = TypeInfo_Thread;
        else if (type == typeof(Table))
            this._name = TypeInfo_Table;
        else
            this._name = $"'{type.AssemblyQualifiedName}'";
    }

    internal TypeInfo(BasicType type) : this(type switch
    {
        BasicType.Nil => null,
        BasicType.Boolean => typeof(Boolean),
        BasicType.Number => typeof(Number),
        BasicType.String => typeof(String),
        BasicType.Function => typeof(Function),
        BasicType.Userdata => typeof(Userdata),
        BasicType.Thread => typeof(Thread),
        BasicType.Table => typeof(Table),
        _ => throw new NotSupportedException()
    })
    { }

    #region Object
    protected internal override Table? Metatable
    {
        get => Lua.String.s_mt;
        set => Lua.String.s_mt = value;
    }

    public override Object? GetMetatable() => Lua.String.s_mt;

    public override TypeInfo GetTypeInfo() => TypeInfo.String;

    public override MultiReturns Invoke(params Object?[] args) => ((String)this).Invoke(args);
    #endregion

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        string => this.Equals((string)obj),
        Lua.String => this.Equals((String)obj),
        TypeInfo => this.Equals((TypeInfo)obj),
        _ => false
    };
    public bool Equals(string? other) => other is not null && this._name == other;
    public bool Equals(String? other) => other is not null && this._name == (string)other;
    public bool Equals(TypeInfo? other) => other is not null && (this._name == other._name && this._type == other._type);

    public override int GetHashCode() => this._name.GetHashCode() ^ (this._type is null ? 0 : this._type.GetHashCode());

    public override string ToString() => this._name;

    /// <inheritdoc/>
    /// <exception cref="InvalidCastException"><paramref name="type"/> 不是能接受的转换目标类型。</exception>
    public override object ChangeType(Type type) => ((String)this).ChangeType(type);

    public static implicit operator TypeInfo(string type) => new(type ?? throw new ArgumentNullException(nameof(type)));
    public static implicit operator TypeInfo(String type) => new((string)(type ?? throw new ArgumentNullException(nameof(type))));
    public static implicit operator TypeInfo(Type? type) => new(type);

    public static explicit operator string(TypeInfo typeInfo) => typeInfo._name;
    public static explicit operator String(TypeInfo typeInfo) => typeInfo._name;
}

public enum BasicType
{
    Nil,
    Boolean,
    Number,
    String,
    Function,
    Userdata,
    Thread,
    Table
}
