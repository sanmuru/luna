using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.Lua.LuaSyntaxTree;
using ThisParseOptions = SamLu.CodeAnalysis.Lua.LuaParseOptions;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;
using ThisSyntaxTree = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxTree;
using ThisParseOptions = SamLu.CodeAnalysis.MoonScript.MoonScriptParseOptions;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

public partial class
#if LANG_LUA
    LuaSyntaxTree
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTree
#endif
{
    internal sealed class DummySyntaxTree : ThisSyntaxTree
    {
        private readonly Syntax.BlockSyntax _node;

        public override Encoding? Encoding => Encoding.UTF8;

        public override int Length => 0;

        public override ThisParseOptions Options => ThisParseOptions.Default;

        public override string FilePath => string.Empty;

        public override bool HasCompilationUnitRoot => true;

        public DummySyntaxTree() =>
            this._node = this.CloneNodeAsRoot(SyntaxFactory.ParseBlock(string.Empty));

        public override string ToString() => string.Empty;

        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(string.Empty, Encoding.UTF8);

        public override bool TryGetText(out SourceText text)
        {
            text = SourceText.From(string.Empty, Encoding.UTF8);
            return true;
        }

        public override SyntaxReference GetReference(SyntaxNode node) => new SimpleSyntaxReference(node);

        public override ThisSyntaxNode GetRoot(CancellationToken cancellationToken = default) => this._node;

        public override bool TryGetRoot(out ThisSyntaxNode root)
        {
            root = this._node;
            return true;
        }

        public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default) => default;

        public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options) => SyntaxFactory.SyntaxTree(root, options: options, path: this.FilePath);

        public override SyntaxTree WithFilePath(string path) => SyntaxFactory.SyntaxTree(this._node, options: this.Options, path: path);
    }
}
