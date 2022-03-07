using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax;

namespace SamLu.CodeAnalysis.Lua.Syntax
{
    public abstract class LuaSyntaxNode : SyntaxNode, IFormattable
    {
        internal LuaSyntaxNode(GreenNode green, SyntaxNode? parent, int position) : base(green, parent, position)
        {
        }

        internal LuaSyntaxNode(GreenNode green, int position, SyntaxTree syntaxTree) : base(green, position, syntaxTree)
        {
        }

        public abstract string ToString(string? format, IFormatProvider? formatProvider);
    }
}
