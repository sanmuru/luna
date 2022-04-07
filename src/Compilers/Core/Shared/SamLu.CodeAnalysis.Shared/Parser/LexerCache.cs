using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

internal partial class LexerCache
{
    private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> s_keywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(
        512,
        key =>
        {
            var kind = SyntaxFacts.GetKeywordKind(key);
            if (kind == SyntaxKind.None)
                kind = SyntaxFacts.GetContextualKeywordKind(key);

            return kind;
        }
    );
    private readonly TextKeyedCache<SyntaxTrivia> _triviaMap;
    private readonly TextKeyedCache<SyntaxToken> _tokenMap;
    private readonly CachingIdentityFactory<string, SyntaxKind> _keywordKindMap;
    /// <summary>
    /// 关键词的最大字符长度。
    /// </summary>
    internal const int MaxKeywordLength = 10;

    internal LexerCache()
    {
        this._triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
        this._tokenMap = TextKeyedCache<SyntaxToken>.GetInstance();
        this._keywordKindMap = LexerCache.s_keywordKindPool.Allocate();
    }

    internal void Free()
    {
        this._keywordKindMap.Free();
        this._triviaMap.Free();
        this._tokenMap.Free();
    }

    internal bool TryGetKeywordKind(string key, out SyntaxKind kind)
    {
        if (key.Length > MaxKeywordLength)
        {
            kind = SyntaxKind.None;
            return false;
        }

        kind = this._keywordKindMap.GetOrMakeValue(key);
        return kind != SyntaxKind.None;
    }

    internal SyntaxTrivia LookupTrivia(
        char[] textBuffer,
        int keyStart,
        int keyLength,
        int hashCode,
        Func<SyntaxTrivia> createTriviaFunction)
    {
        var value = this._triviaMap.FindItem(textBuffer, keyStart, keyLength, hashCode);

        if (value is null)
        {
            value = createTriviaFunction();
            this._triviaMap.AddItem(textBuffer, keyStart, keyLength, hashCode, value);
        }

        return value;
    }

    internal SyntaxToken LookupToken(
        char[] textBuffer,
        int keyStart,
        int keyLength,
        int hashCode,
        Func<SyntaxToken> createTokenFunction)
    {
        var value = this._tokenMap.FindItem(textBuffer, keyStart, keyLength, hashCode);

        if (value is null)
        {
            value = createTokenFunction();
            this._tokenMap.AddItem(textBuffer, keyStart, keyLength, hashCode, value);
        }

        return value;
    }

}
