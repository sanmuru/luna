using System.Runtime.Serialization;

namespace SamLu.Lua;

public class ComparisonNotSupportedException : Exception
{
    public ComparisonNotSupportedException(string left!!, string right!!) : this($"attempt to compare {left} with {right}") { }

    public ComparisonNotSupportedException(TypeInfo left!!, TypeInfo right!!) : this(left.ToString(), right.ToString()) { }

    public ComparisonNotSupportedException(string? message) : base(message) { }

    public ComparisonNotSupportedException(string? message, Exception? innerException) : base(message, innerException) { }

    protected ComparisonNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
