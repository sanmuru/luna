using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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

partial class
#if LANG_LUA
    LuaSyntaxTree
#elif LANG_MOONSCRIPT
    MoonScriptSyntaxTree
#endif
{
    private class ParsedSyntaxTree : ThisSyntaxTree
    {
        private readonly ThisParseOptions _options;
        private readonly string _path;
        private readonly ThisSyntaxNode _root;
        private readonly bool _hasCompilationUnitRoot;
        private readonly Encoding? _encoding;
        private readonly SourceHashAlgorithm _checksumAlgorithm;
        private SourceText? _lazyText;

        public override ThisParseOptions Options => this._options;
        public override string FilePath => this._path;
        public override bool HasCompilationUnitRoot => this._hasCompilationUnitRoot;
        public override Encoding? Encoding => this._encoding;
        public override int Length => this._root.FullSpan.Length;

        internal ParsedSyntaxTree(
            SourceText? text,
            Encoding? encoding,
            SourceHashAlgorithm checksumAlgorithm,
            string? path,
            ThisParseOptions options!!,
            ThisSyntaxNode root!!,
            bool cloneRoot)
        {
            Debug.Assert(
                text is null ||
                text.Encoding == encoding &&
                text.ChecksumAlgorithm == checksumAlgorithm);

            this._lazyText = text;
            this._encoding = encoding ?? text?.Encoding;
            this._checksumAlgorithm = checksumAlgorithm;
            this._options = options;
            this._path = path ?? String.Empty;
            this._root = cloneRoot ? this.CloneNodeAsRoot(root) : root;
            this._hasCompilationUnitRoot = root.Kind() == SyntaxKind.Block; // 基于Lua的语言的编译单元是Block。
        }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            if (this._lazyText is null)
                Interlocked.Exchange(
                    ref this._lazyText,
                    this.GetRoot(cancellationToken).GetText(this._encoding, this._checksumAlgorithm));

            return this._lazyText;
        }

        public override bool TryGetText([NotNullWhen(true)] out SourceText? text)
        {
            text = this._lazyText;
            return text is not null;
        }

        public override ThisSyntaxNode GetRoot(CancellationToken cancellationToken = default) => this._root;

        public override bool TryGetRoot([NotNullWhen(true)] out ThisSyntaxNode? root)
        {
            root = this._root;
            return true;
        }

        public override SyntaxReference GetReference(SyntaxNode node) => new SimpleSyntaxReference(node);

        public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
        {
            if (object.ReferenceEquals(this._root, root) && object.ReferenceEquals(this._options, options))
                return this;
            else
                return new ParsedSyntaxTree(
                    text: null,
                    encoding: this._encoding,
                    checksumAlgorithm: this._checksumAlgorithm,
                    path: this._path,
                    options: (ThisParseOptions)options,
                    root: (ThisSyntaxNode)root,
                    cloneRoot: true);
        }

        public override SyntaxTree WithFilePath(string path)
        {
            if (this._path == path)
                return this;
            else
                return new ParsedSyntaxTree(
                    text: this._lazyText,
                    encoding: this._encoding,
                    checksumAlgorithm: this._checksumAlgorithm,
                    path: path,
                    options: this._options,
                    root: this._root,
                    cloneRoot: true);
        }
    }
}
