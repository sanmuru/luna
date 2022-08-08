#if DEBUG
using Microsoft.CodeAnalysis;

#if LANG_LUA
namespace SamLu.CodeAnalysis.Lua;

using SamLu.CodeAnalysis.Lua.Syntax;
using ThisSyntaxNode = LuaSyntaxNode;
using ThisSyntaxTree = LuaSyntaxTree;
using InternalSyntaxNode = Syntax.InternalSyntax.LuaSyntaxNode;
#elif LANG_MOONSCRIPT
namespace SamLu.CodeAnalysis.MoonScript;

using SamLu.CodeAnalysis.MoonScript.Syntax;
using ThisSyntaxNode = MoonScriptSyntaxNode;
using ThisSyntaxTree = MoonScriptSyntaxTree;
using InternalSyntaxNode = Syntax.InternalSyntax.MoonScriptSyntaxNode;
#endif

partial class
#if LANG_LUA
        LuaSyntaxNode
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxNode
#endif
{
    internal class MockNode : ThisSyntaxNode
    {
        public MockNode(InternalSyntaxNode green) : this(green, null, 0) { }
        public MockNode(InternalSyntaxNode green, ThisSyntaxNode? parent, int position) : base(green, parent, position) { }

        public override TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor) where TResult : default => default;

        public override void Accept(LuaSyntaxVisitor visitor) { }

        internal override SyntaxNode? GetCachedSlot(int index) => null;

        internal override SyntaxNode? GetNodeSlot(int slot) => null;
    }
}
#endif
