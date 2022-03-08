using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua;

namespace SamLu.CodeAnalysis.Lua.Syntax
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
                this._hasCompilationUnitRoot = root.Kine() == SyntaxKind.CompilationUnit;
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


        }
    }
}
