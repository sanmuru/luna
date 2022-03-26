using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;

namespace SamLu.CodeAnalysis.Lua
{
    partial class LuaSyntaxTree
    {
        private class ParsedSyntaxTree : LuaSyntaxTree
        {
            private readonly LuaParseOptions _options;
            private readonly string _path;
            private readonly LuaSyntaxNode _root;
            private readonly bool _hasCompilationUnitRoot;
            private readonly Encoding? _encoding;
            private readonly SourceHashAlgorithm _checksumAlgorithm;
            private SourceText? _lazyText;

            public override LuaParseOptions Options => this._options;
            public override string FilePath => this._path;
            public override bool HasCompilationUnitRoot => this._hasCompilationUnitRoot;
            public override Encoding? Encoding => this._encoding;
            public override int Length => this._root.FullSpan.Length;

            internal ParsedSyntaxTree(
                SourceText? text,
                Encoding? encoding,
                SourceHashAlgorithm checksumAlgorithm,
                string? path,
                LuaParseOptions options!!,
                LuaSyntaxNode root!!,
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
                this._hasCompilationUnitRoot = root.Kind() == SyntaxKind.CompilationUnit;
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

            public override LuaSyntaxNode GetRoot(CancellationToken cancellationToken = default) => this._root;

            public override bool TryGetRoot([NotNullWhen(true)] out LuaSyntaxNode? root)
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
                        options: (LuaParseOptions)options,
                        root: (LuaSyntaxNode)root,
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
}
