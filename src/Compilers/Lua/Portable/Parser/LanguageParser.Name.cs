﻿using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
#if TESTING
    internal
#else
    private
#endif
        IdentifierNameSyntax ParseIdentifierName()
    {
        var identifier = this.EatToken(SyntaxKind.IdentifierToken);
        return this._syntaxFactory.IdentifierName(identifier);
    }

    private IdentifierNameSyntax ParseAsIdentifierName()
    {
        var identifier = this.EatTokenAsKind(SyntaxKind.IdentifierToken);
        return this._syntaxFactory.IdentifierName(identifier);
    }

    private void ParseSeparatedIdentifierNames(in SeparatedSyntaxListBuilder<IdentifierNameSyntax> namesBuilder) =>
        this.ParseSeparatedSyntaxList(
            namesBuilder,
            parseNodeFunc: _ => this.ParseIdentifierName(),
            predicateNode: _ => true,
            predicateSeparator: _ => this.CurrentTokenKind == SyntaxKind.CommaToken);

#if TESTING
    internal
#else
    private
#endif
        NameSyntax ParseName()
    {
        NameSyntax left = this.ParseIdentifierName();
        // QualifiedName
        while (this.CurrentTokenKind == SyntaxKind.DotToken)
        {
            var dot = this.EatToken(SyntaxKind.DotToken);
            IdentifierNameSyntax right;
            if (this.CurrentTokenKind != SyntaxKind.IdentifierToken)
            {
                dot = this.AddError(dot, ErrorCode.ERR_IdentifierExpected);
                right = this.CreateMissingIdentifierName();
                left = this._syntaxFactory.QualifiedName(left, dot, right);
            }
            else
            {
                right = this.ParseIdentifierName();
                left = this._syntaxFactory.QualifiedName(left, dot, right);
            }
        }

        // ImplicitSelfParameterName
        if (this.CurrentTokenKind == SyntaxKind.ColonToken)
        {
            var colon = this.EatToken(SyntaxKind.ColonToken);
            IdentifierNameSyntax right;
            if (this.CurrentTokenKind != SyntaxKind.IdentifierToken)
            {
                colon = this.AddError(colon, ErrorCode.ERR_IdentifierExpected);
                right = this.CreateMissingIdentifierName();
                left = this._syntaxFactory.ImplicitSelfParameterName(left, colon, right);
            }
            else
            {
                right = this.ParseIdentifierName();
                left = this._syntaxFactory.ImplicitSelfParameterName(left, colon, right);
            }

            // 将后续可能的QualifiedName及ImplicitSelfParameterName结构视为错误。
            if (this.CurrentTokenKind is SyntaxKind.DotToken or SyntaxKind.ColonToken)
            {
                var unexpectedChar = SyntaxFacts.GetText(this.CurrentTokenKind);
                var builder = this._pool.Allocate<SyntaxToken>();
                do
                {
                    builder.Add(this.EatToken());
                    if (this.CurrentTokenKind == SyntaxKind.IdentifierToken)
                        builder.Add(this.EatToken());
                }
                while (this.CurrentTokenKind is SyntaxKind.DotToken or SyntaxKind.ColonToken);
                var skippedTokensTrivia = this._syntaxFactory.SkippedTokensTrivia(this._pool.ToListAndFree(builder));
                skippedTokensTrivia = this.AddError(skippedTokensTrivia, ErrorCode.ERR_UnexpectedCharacter, unexpectedChar);

                left = this.AddTrailingSkippedSyntax(left, skippedTokensTrivia);
            }
        }

        return left;
    }

#if TESTING
    internal
#else
    private
#endif
        IdentifierNameSyntax CreateMissingIdentifierName() => this._syntaxFactory.IdentifierName(LanguageParser.CreateMissingIdentifierToken());

#if TESTING
    internal
#else
    private
#endif
        static SyntaxToken CreateMissingIdentifierToken() => SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken);
}
