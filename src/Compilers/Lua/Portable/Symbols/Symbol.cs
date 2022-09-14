using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua;

partial class Symbol
{
    /// <summary>
    /// Returns true if this symbol can be referenced by its name in code. Examples of symbols
    /// that cannot be referenced by name are:
    ///    constructors, destructors, operators, explicit interface implementations,
    ///    accessor methods for properties and events, array types.
    /// </summary>
    public bool CanBeReferencedByName
    {
        get
        {
            switch (this.Kind)
            {
#warning 需要补充符号类型处理
                default:
                    throw ExceptionUtilities.UnexpectedValue(this.Kind);
            }

            return SyntaxFacts.IsValidIdentifier(this.Name) && !SyntaxFacts.ContainsDroppedIdentifierCharacters(this.Name);
        }
    }
}
