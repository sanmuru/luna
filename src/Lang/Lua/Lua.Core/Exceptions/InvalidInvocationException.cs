using System.Runtime.Serialization;

namespace SamLu.Lua;

public class InvalidInvocationException : LuaException
{
    public InvalidInvocationException() { }

    public InvalidInvocationException(Object? value) : this("value", value) { }

    public InvalidInvocationException(TypeInfo typeInfo) : this("value", typeInfo ?? throw new ArgumentNullException(nameof(typeInfo))) { }

    public InvalidInvocationException(string paramInfo, Object? value) : this(paramInfo, value is null ? TypeInfo.Nil : value.GetType()) { }

    public InvalidInvocationException(string paramInfo, TypeInfo typeInfo) : this($"Attempt to call {paramInfo ?? throw new ArgumentNullException(nameof(paramInfo))} (a {typeInfo ?? throw new ArgumentNullException(nameof(typeInfo))} value)") { }

    public InvalidInvocationException(string? message) : base(message) { }

    public InvalidInvocationException(string? message, Exception? innerException) : base(message, innerException) { }

    protected InvalidInvocationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
