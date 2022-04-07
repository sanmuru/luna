using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal enum LexerMode
    {
        None = 0,

        Syntax = 0x0001,
        DebuggerSyntax = 0x0002,

        MaskLexMode = 0xFFFF
    }

    internal partial class Lexer : AbstractLexer
    {
        private const int IdentifierBufferInitialCapacity = 32;
        private const int TriviaListInitialCapacity = 8;

        private readonly LuaParseOptions _options;

        private LexerMode _mode;
        private readonly StringBuilder _builder;
        private char[] _identifierBuffer;
        private int _identifierLength;
        private readonly LexerCache _cache;
        private int _badTokenCount; // 产生的坏标识符的累计数量。

        public LuaParseOptions Options => this._options;

        internal struct TokenInfo
        {
            internal SyntaxKind Kind;
            internal SyntaxKind ContextualKind;
            internal string Text;
            internal SpecialType ValueKind;
            internal bool HasIdentifierEscapeSequence;
            internal string StringValue;
            internal long LongValue;
            internal double DoubleValue;
            internal bool IsVerbatim;
        }

        public Lexer(SourceText text, LuaParseOptions options) : base(text)
        {
            this._options = options;
            this._builder = new StringBuilder();
            this._identifierBuffer = new char[Lexer.IdentifierBufferInitialCapacity];
            this._cache = new();
            this._createQuickTokenFunction = this.CreateQuickToken;
        }

        public override void Dispose()
        {
            this._cache.Free();

            base.Dispose();
        }

        public void Reset(int position) => this.TextWindow.Reset(position);

        private static LexerMode ModeOf(LexerMode mode) => mode & LexerMode.MaskLexMode;

        private bool ModeIs(LexerMode mode) => Lexer.ModeOf(this._mode) == mode;

        public SyntaxToken Lex(ref LexerMode mode)
        {
            var result = this.Lex(mode);
            mode = this._mode;
            return result;
        }

        public SyntaxToken Lex(LexerMode mode)
        {
            this._mode = mode;
            
            switch (this._mode)
            {
                case LexerMode.Syntax:
                case LexerMode.DebuggerSyntax:
                    return this.QuickScanSyntaxToken() ?? this.LexSyntaxToken();
            }

            switch (Lexer.ModeOf(this._mode))
            {
                default:
                    throw ExceptionUtilities.UnexpectedValue(ModeOf(_mode));
            }
        }

#warning 未完成
    }
}
