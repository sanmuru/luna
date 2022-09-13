using System.Diagnostics;
using Luna.Compilers.Generators.Model;

namespace Luna.Compilers.Generators;

internal abstract class AbstractFileWriter : IndentWriter
{
    private readonly Tree _tree;
    private readonly IDictionary<string, string?> _parentMap;
    private readonly ILookup<string, string> _childMap;

    private readonly IDictionary<string, Node> _nodeMap;
    private readonly IDictionary<string, TreeType> _typeMap;

    protected AbstractFileWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken) : base(writer, 4, cancellationToken)
    {
        _tree = tree;
        _nodeMap = tree.Types.OfType<Node>().ToDictionary(n => n.Name);
        _typeMap = tree.Types.ToDictionary(n => n.Name);
        _parentMap = tree.Types.ToDictionary(n => n.Name, n => n.Base);
        _parentMap.Add(tree.Root, null);
        _childMap = tree.Types.Where(n => n.Base is not null).ToLookup(n => n.Base!, n => n.Name);
    }

    protected IDictionary<string, string?> ParentMap { get { return _parentMap; } }
    protected ILookup<string, string> ChildMap { get { return _childMap; } }
    protected Tree Tree { get { return _tree; } }

    #region Node helpers
    protected static string OverrideOrNewModifier(Field field)
    {
        return IsOverride(field) ? "override " : IsNew(field) ? "new " : "";
    }

    protected static bool CanBeField(Field field)
    {
        return field.Type != "SyntaxToken" && !IsAnyList(field.Type) && !IsOverride(field) && !IsNew(field);
    }

    protected static string GetFieldType(Field field, bool green)
    {
        // Fields in red trees are lazily initialized, with null as the uninitialized value
        return getNullableAwareType(field.Type, optionalOrLazy: IsOptional(field) || !green, green);

        static string getNullableAwareType(string fieldType, bool optionalOrLazy, bool green)
        {
            if (IsAnyList(fieldType))
            {
                if (optionalOrLazy)
                    return green ? "GreenNode?" : "SyntaxNode?";
                else
                    return green ? "GreenNode?" : "SyntaxNode";
            }

            switch (fieldType)
            {
                case var _ when !optionalOrLazy:
                    return fieldType;

                case "bool":
                case "SyntaxToken" when !green:
                    return fieldType;

                default:
                    return fieldType + "?";
            }
        }
    }

    protected bool IsDerivedOrListOfDerived(string baseType, string derivedType)
    {
        return IsDerivedType(baseType, derivedType)
            || ((IsNodeList(derivedType) || IsSeparatedNodeList(derivedType))
                && IsDerivedType(baseType, GetElementType(derivedType)));
    }

    protected static bool IsSeparatedNodeList(string typeName)
    {
        return typeName.StartsWith("SeparatedSyntaxList<", StringComparison.Ordinal);
    }

    protected static bool IsNodeList(string typeName)
    {
        return typeName.StartsWith("SyntaxList<", StringComparison.Ordinal);
    }

    public static bool IsAnyNodeList(string typeName)
    {
        return IsNodeList(typeName) || IsSeparatedNodeList(typeName);
    }

    protected bool IsNodeOrNodeList(string typeName)
    {
        return IsNode(typeName) || IsNodeList(typeName) || IsSeparatedNodeList(typeName) || typeName == "SyntaxNodeOrTokenList";
    }

    protected static string GetElementType(string typeName)
    {
        if (!typeName.Contains("<"))
            return string.Empty;
        int iStart = typeName.IndexOf('<');
        int iEnd = typeName.IndexOf('>', iStart + 1);
        if (iEnd < iStart)
            return string.Empty;
        var sub = typeName.Substring(iStart + 1, iEnd - iStart - 1);
        return sub;
    }

    protected static bool IsAnyList(string typeName)
    {
        return IsNodeList(typeName) || IsSeparatedNodeList(typeName) || typeName == "SyntaxNodeOrTokenList";
    }

    protected bool IsDerivedType(string typeName, string? derivedTypeName)
    {
        if (typeName == derivedTypeName)
            return true;
        if (derivedTypeName is not null && _parentMap.TryGetValue(derivedTypeName, out var baseType))
        {
            return IsDerivedType(typeName, baseType);
        }
        return false;
    }

    protected static bool IsRoot(Node n)
    {
        return n.Root is not null && string.Compare(n.Root, "true", true) == 0;
    }

    protected bool IsNode(string typeName)
    {
        return _parentMap.ContainsKey(typeName);
    }

    protected Node? GetNode(string? typeName)
        => typeName is not null && _nodeMap.TryGetValue(typeName, out var node) ? node : null;

    protected TreeType? GetTreeType(string? typeName)
        => typeName is not null && _typeMap.TryGetValue(typeName, out var node) ? node : null;

    private static bool IsTrue(string? val)
        => val is not null && string.Compare(val, "true", true) == 0;

    protected static bool IsOptional(Field f)
        => IsTrue(f.Optional);

    protected static bool IsOverride(Field f)
        => f.Override is not null;

    protected static bool IsNew(Field f)
        => IsTrue(f.New);

    protected static bool HasErrors(Node n)
    {
        return n.Errors is null || string.Compare(n.Errors, "true", true) == 0;
    }

    protected static string CamelCase(string name)
    {
        if (char.IsUpper(name[0]))
        {
            name = char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
        return FixKeyword(name);
    }

    protected static string FixKeyword(string name)
    {
        if (IsKeyword(name))
        {
            return "@" + name;
        }
        return name;
    }

    protected static string StripPost(string name, string post)
    {
        return name.EndsWith(post, StringComparison.Ordinal)
            ? name.Substring(0, name.Length - post.Length)
            : name;
    }

    protected static bool IsKeyword(string name)
    {
        switch (name)
        {
            case "bool":
            case "byte":
            case "sbyte":
            case "short":
            case "ushort":
            case "int":
            case "uint":
            case "long":
            case "ulong":
            case "double":
            case "float":
            case "decimal":
            case "string":
            case "char":
            case "object":
            case "typeof":
            case "sizeof":
            case "null":
            case "true":
            case "false":
            case "if":
            case "else":
            case "while":
            case "for":
            case "foreach":
            case "do":
            case "switch":
            case "case":
            case "default":
            case "lock":
            case "try":
            case "throw":
            case "catch":
            case "finally":
            case "goto":
            case "break":
            case "continue":
            case "return":
            case "public":
            case "private":
            case "internal":
            case "protected":
            case "static":
            case "readonly":
            case "sealed":
            case "const":
            case "new":
            case "override":
            case "abstract":
            case "virtual":
            case "partial":
            case "ref":
            case "out":
            case "in":
            case "where":
            case "params":
            case "this":
            case "base":
            case "namespace":
            case "using":
            case "class":
            case "struct":
            case "interface":
            case "delegate":
            case "checked":
            case "get":
            case "set":
            case "add":
            case "remove":
            case "operator":
            case "implicit":
            case "explicit":
            case "fixed":
            case "extern":
            case "event":
            case "enum":
            case "unsafe":
                return true;
            default:
                return false;
        }
    }

    protected List<Kind> GetKindsOfFieldOrNearestParent(TreeType type, Field field)
    {
        while ((field.Kinds is null || field.Kinds.Count == 0) && IsOverride(field))
        {
            var t = GetTreeType(type.Base);
            field = (t switch
            {
                Node node => node.Fields,
                AbstractNode abstractNode => abstractNode.Fields,
                _ => throw new InvalidOperationException("Unexpected node type.")
            }).Single(f => f.Name == field.Name);
            type = t!;
        }

        return field.Kinds.Distinct().ToList();
    }
    #endregion
}
