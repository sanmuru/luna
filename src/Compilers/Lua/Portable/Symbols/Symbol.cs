using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua;

partial class Symbol
{
    /// <summary>
    /// 此符号是否能在代码中通过名称引用。
    /// </summary>
    /// <value>
    /// 若此符号是否能在代码中通过名称引用，则返回<see langword="true"/>；否则返回<see langword="false"/>。
    /// </value>
    public bool CanBeReferencedByName
    {
        get
        {
            switch (this.Kind)
            {
                case SymbolKind.Local:
                case SymbolKind.Label:
                    return true;

                case SymbolKind.Assembly:
                case SymbolKind.DynamicType:
                case SymbolKind.NetModule:
                    return false;

                case SymbolKind.Namespace:
                case SymbolKind.Field:
                case SymbolKind.Parameter:
                case SymbolKind.NamedType:
                    break;

                default:
                    throw ExceptionUtilities.UnexpectedValue(this.Kind);
            }

            return SyntaxFacts.IsValidIdentifier(this.Name);
        }
    }
}
