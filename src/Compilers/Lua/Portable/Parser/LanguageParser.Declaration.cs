﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

partial class LanguageParser
{
#if TESTING
    internal
#else
    private
#endif
        void ParseFunctionBody(out ParameterListSyntax parameters, out BlockSyntax block, out SyntaxToken endKeyword)
    {
        parameters = this.ParseParameterList();
        block = this.ParseBlock();
        endKeyword = this.EatToken(SyntaxKind.EndKeyword);
    }

    #region 形参
#if TESTING
    internal
#else
    private
#endif
        ParameterListSyntax ParseParameterList()
    {
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var parameters = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseParameter(),
            predicateNode: _ => true,
            predicateSeparator: _ => this.CurrentTokenKind == SyntaxKind.CommaToken);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this._syntaxFactory.ParameterList(openParen, parameters, closeParen);
    }

#if TESTING
    internal
#else
    private
#endif
        ParameterSyntax ParseParameter()
    {
        var identifier = this.CurrentTokenKind switch
        {
            SyntaxKind.IdentifierToken or
            SyntaxKind.DotDotDotToken => this.EatToken(),

            _ => LanguageParser.CreateMissingIdentifierToken(),
        };
        if (identifier.IsMissing)
            identifier = this.AddError(identifier, ErrorCode.ERR_IdentifierExpected);
        return this._syntaxFactory.Parameter(identifier);
    }
    #endregion

    #region 字段
#if TESTING
    internal
#else
    private
#endif
        FieldListSyntax ParseFieldList() =>
        this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseField(),
            predicateNode: _ => true,
            predicateSeparator: _ => this.IsPossibleFieldListSeparator(),
            list => this._syntaxFactory.FieldList(list))!;

#if TESTING
    internal
#else
    private
#endif
        bool IsPossibleFieldListSeparator() => this.CurrentTokenKind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken;

#if TESTING
    internal
#else
    private
#endif
        FieldSyntax ParseField()
    {
        // 解析键值对表字段。
        if (this.CurrentTokenKind == SyntaxKind.OpenBracketToken ||
            (SyntaxFacts.IsLiteralToken(this.CurrentTokenKind) && this.PeekToken(1).Kind == SyntaxKind.EqualsToken)) // 错误使用常量作为键。
            return this.ParseKeyValueField();
        // 解析名值对表字段。
        else if (this.CurrentTokenKind == SyntaxKind.EqualsToken || // 错误遗失标识符。
            (this.CurrentTokenKind == SyntaxKind.IdentifierToken && this.PeekToken(1).Kind == SyntaxKind.EqualsToken))
            return this.ParseNameValueField();
        // 解析列表项表字段。
        else
        {
            if (this.CurrentTokenKind == SyntaxKind.EndOfFileToken)
                return this._syntaxFactory.ItemField(this.CreateMissingIdentifierName());
            else
                return this._syntaxFactory.ItemField(this.ParseFieldValue());
        }
    }

#if TESTING
    internal
#else
    private
#endif
        NameValueFieldSyntax ParseNameValueField()
    {
        var name = this.ParseIdentifierName();
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.EqualsToken);
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this._syntaxFactory.NameValueField(name, equals, value);
    }

#if TESTING
    internal
#else
    private
#endif
        KeyValueFieldSyntax ParseKeyValueField()
    {
        var openBracket = this.EatToken(SyntaxKind.OpenBracketToken);
        var key = this.ParseFieldKey();
        var closeBracket = this.EatToken(SyntaxKind.CloseBracketToken);
        var equals = this.EatToken(SyntaxKind.EqualsToken);
        var value = this.ParseFieldValue();
        return this._syntaxFactory.KeyValueField(openBracket, key, closeBracket, equals, value);
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseFieldKey()
    {
        var expr = this.ParseExpression();

        // 跳过后方的标志和表达式直到等于符号或右方括号。
        var skippedSyntax = this.SkipTokensAndExpressions(
            token => token.Kind is not (SyntaxKind.EqualsToken or SyntaxKind.CloseBracketToken or SyntaxKind.EndOfFileToken),
            new FieldKeySkippedNodesVisitor(this));
        if (skippedSyntax is not null)
            expr = this.AddTrailingSkippedSyntax(expr, skippedSyntax);

        return expr;
    }

    /// <summary>
    /// 处理字段键表达式后方需要跳过的语法标志和语法节点的访问器。
    /// </summary>
    private sealed class FieldKeySkippedNodesVisitor : LuaSyntaxVisitor<LuaSyntaxNode>
    {
        private readonly LanguageParser _parser;

        public FieldKeySkippedNodesVisitor(LanguageParser parser) => this._parser = parser;

        /// <summary>
        /// 处理语法标志，向语法标志添加<see cref="ErrorCode.ERR_InvalidExprTerm"/>错误。
        /// </summary>
        /// <param name="token">要处理的语法标志。</param>
        /// <returns>处理后的<paramref name="token"/>。</returns>
        public override LuaSyntaxNode VisitToken(SyntaxToken token) =>
            this._parser.AddError(token, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(token.Kind));

        /// <summary>
        /// 处理所有语法节点。
        /// </summary>
        /// <remarks>若<paramref name="node"/>不是表达式语法，则抛出异常。</remarks>
        /// <param name="node">要处理的语法节点。</param>
        /// <returns>处理后的<paramref name="node"/>。</returns>
        /// <exception cref="ExceptionUtilities.Unreachable">当<paramref name="node"/>不是表达式语法时，这种情况不应发生。</exception>
        protected override LuaSyntaxNode DefaultVisit(LuaSyntaxNode node) =>
            node is ExpressionSyntax ? node : throw ExceptionUtilities.Unreachable;
    }

#if TESTING
    internal
#else
    private
#endif
        ExpressionSyntax ParseFieldValue()
    {
        ExpressionSyntax? expr = null;
        if (this.IsPossibleExpression())
            expr = this.ParseExpressionCore();

        // 跳过后方的标志和表达式直到字段结束。
        var skippedSyntax = this.SkipTokensAndExpressions(token => token.Kind is not (SyntaxKind.CommaToken or SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken));
        if (skippedSyntax is null) // 后方没有需要跳过的标志和表达式。
            expr ??= this.ReportMissingExpression(this.CreateMissingIdentifierName());
        else
        {
            skippedSyntax = this.AddError(skippedSyntax, ErrorCode.ERR_InvalidFieldValueTerm);
            expr = this.AddTrailingSkippedSyntax(
                expr ?? this.ReportMissingExpression(this.CreateMissingIdentifierName()),
                skippedSyntax);
        }
        return expr;
    }
    #endregion

    #region 实参
#if TESTING
    internal
#else
    private
# endif
        bool IsPossibleInvocationArguments() => this.CurrentTokenKind is
        SyntaxKind.OpenParenToken or
        SyntaxKind.OpenBraceToken or
        SyntaxKind.StringLiteralToken or
        SyntaxKind.MultiLineRawStringLiteralToken;

#if TESTING
    internal
#else
    private
#endif
        InvocationArgumentsSyntax ParseInvocationArguments() =>
        this.CurrentTokenKind switch
        {
            SyntaxKind.OpenParenToken => this.ParseArgumentList(),
            SyntaxKind.OpenBraceToken => this.ParseArgumentTable(),
            SyntaxKind.StringLiteralToken or
            SyntaxKind.MultiLineRawStringLiteralToken => this.ParseArgumentString(),
            _ => throw ExceptionUtilities.UnexpectedValue(this.CurrentTokenKind)
        };

#if TESTING
    internal
#else
    private
#endif
        ArgumentListSyntax ParseArgumentList()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.OpenParenToken);
        var openParen = this.EatToken(SyntaxKind.OpenParenToken);
        var arguments = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseArgument(),
            predicateNode: _ => this.IsPossibleExpression(),
            predicateSeparator: _ => this.CurrentTokenKind == SyntaxKind.CommaToken);
        var closeParen = this.EatToken(SyntaxKind.CloseParenToken);
        return this._syntaxFactory.ArgumentList(openParen, arguments, closeParen);
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentTableSyntax ParseArgumentTable()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.OpenBraceToken);
        var table = this.ParseTableConstructorExpression();
        return this._syntaxFactory.ArgumentTable(table);
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentStringSyntax ParseArgumentString()
    {
        Debug.Assert(this.CurrentTokenKind is SyntaxKind.StringLiteralToken or SyntaxKind.MultiLineRawStringLiteralToken);
        var stringLiteral = this.EatToken();
        return this._syntaxFactory.ArgumentString(this._syntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, stringLiteral));
    }

#if TESTING
    internal
#else
    private
#endif
        ArgumentSyntax ParseArgument()
    {
        var expr = this.ParseExpression();
        return this._syntaxFactory.Argument(expr);
    }
    #endregion

    #region 特性
#if TESTING
    internal
#else
    private
#endif
        NameAttributeListSyntax ParseNameAttributeList()
    {
        var identifier = this.ParseIdentifierName();
        var attributeList = this.CurrentTokenKind == SyntaxKind.LessThanToken ? this.ParseAttributeList() : null;
        return this._syntaxFactory.NameAttributeList(identifier, attributeList);
    }

#if TESTING
    internal
#else
    private
#endif
        AttributeListSyntax ParseAttributeList()
    {
        Debug.Assert(this.CurrentTokenKind == SyntaxKind.LessThanToken);
        var lessThan = this.EatToken(SyntaxKind.LessThanToken);
        var attributes = this.ParseSeparatedSyntaxList(
            parseNodeFunc: _ => this.ParseAttribute(),
            predicateNode: _ => true,
            predicateSeparator: _ => this.CurrentTokenKind == SyntaxKind.CommaToken);
        var greaterThan = this.EatToken(SyntaxKind.GreaterThanToken);
        return this._syntaxFactory.AttributeList(lessThan, attributes, greaterThan);
    }

#if TESTING
    internal
#else
    private
#endif
        AttributeSyntax ParseAttribute()
    {
        SyntaxToken token;
        if (this.CurrentTokenKind is SyntaxKind.CloseKeyword or SyntaxKind.ConstKeyword)
            token = this.EatToken();
        else
            token = SyntaxFactory.MissingToken(SyntaxKind.None);

        var skippedSyntax = this.SkipTokens(token => token.Kind is not (
            SyntaxKind.CommaToken or        // 在分隔符中止。
            SyntaxKind.GreaterThanToken or  // 在特性列表结尾中止。
            SyntaxKind.EqualsToken or       // 在赋值符号提前中止。
            SyntaxKind.SemicolonToken or    // 在最近的语句结尾中止。
            SyntaxKind.EndOfFileToken       // 在文件结尾中止。
        ), new AttributeSkippedTokensVisitor(this));
        if (skippedSyntax is null)
        {
            if (token.IsMissing)
                token = this.AddError(token, ErrorCode.ERR_AttributeExpected);
        }
        else
        {
            token = this.AddTrailingSkippedSyntax(token, skippedSyntax);
        }

        return this._syntaxFactory.Attribute(token);
    }

    /// <summary>
    /// 处理特性后方需要跳过的语法标志的访问器。
    /// </summary>
    private sealed class AttributeSkippedTokensVisitor : LuaSyntaxVisitor<SyntaxToken>
    {
        private readonly LanguageParser _parser;

        public AttributeSkippedTokensVisitor(LanguageParser parser) => this._parser = parser;

        /// <summary>
        /// 处理语法标志，向语法标志添加<see cref="ErrorCode.ERR_InvalidAttrTerm"/>错误。
        /// </summary>
        /// <param name="token">要处理的语法标志。</param>
        /// <returns>处理后的<paramref name="token"/>。</returns>
        public override SyntaxToken VisitToken(SyntaxToken token) =>
            this._parser.AddError(token, ErrorCode.ERR_InvalidAttrTerm, SyntaxFacts.GetText(token.Kind));

        /// <summary>
        /// 处理所有语法节点。
        /// </summary>
        /// <remarks>此方法必定抛出异常。</remarks>
        /// <param name="node">要处理的语法节点。</param>
        /// <returns>处理后的<paramref name="node"/>。</returns>
        /// <exception cref="ExceptionUtilities.Unreachable">当<paramref name="node"/>不是表达式语法时，这种情况不应发生。</exception>
        [DoesNotReturn]
        protected override SyntaxToken DefaultVisit(LuaSyntaxNode node) => throw ExceptionUtilities.Unreachable;
    }
    #endregion
}
