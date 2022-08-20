using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

using System.Diagnostics;
using Utilities;

[TestClass]
public partial class LanguageParserTests
{
    internal static LanguageParser CreateLanguageParser(string source, LuaParseOptions? options = null) => new(LexerTests.CreateLexer(source, options), null, null);

    #region 名称
    [TestMethod]
    public void IdentifierNameParseTests()
    {
        { // 西文标识符
            var parser = LanguageParserTests.CreateLanguageParser(" identifier ");
            var identifierName = parser.ParseIdentifierName();
            Assert.That.IsIdentifierName(identifierName, "identifier");
            Assert.That.NotContainsDiagnostics(identifierName);
            Assert.That.AtEndOfFile(parser);
        }
        { // 中文标识符
            var parser = LanguageParserTests.CreateLanguageParser(" 标识符 ");
            var identifierName = parser.ParseIdentifierName();
            Assert.That.IsIdentifierName(identifierName, "标识符");
            Assert.That.NotContainsDiagnostics(identifierName);
            Assert.That.AtEndOfFile(parser);
        }

        { // 非标识符
            var parser = LanguageParserTests.CreateLanguageParser(" 'string' ");
            var identifierName = parser.ParseIdentifierName();
            Assert.That.IsMissingIdentifierName(identifierName);
            Assert.That.ContainsDiagnostics(identifierName);
            Assert.That.NotAtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void NameParseTests()
    {
        { // 合法的限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" name.identifier ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("name");
            values.Push("identifier");
            Assert.That.IsQualifiedName(name, values);
            Assert.That.NotContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 合法的隐式self参数名称
            var parser = LanguageParserTests.CreateLanguageParser(" name:identifier ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("name");
            values.Push("identifier");
            Assert.That.IsImplicitSelfParameterName(name, values);
            Assert.That.NotContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }

        { // 缺失右侧标识符名称的限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" name. ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("name");
            values.Push(null);
            Assert.That.IsQualifiedName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 缺失左侧标识符名称的限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" .identifier ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push(null);
            values.Push("identifier");
            Assert.That.IsQualifiedName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 缺失右侧标识符名称的限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" name: ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("name");
            values.Push(null);
            Assert.That.IsImplicitSelfParameterName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 缺失左侧标识符名称的限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" :identifier ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push(null);
            values.Push("identifier");
            Assert.That.IsImplicitSelfParameterName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }

        { // 合法的多重限定名称
            var parser = LanguageParserTests.CreateLanguageParser(" a.b.c.d.e.f.g ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("a");
            values.Push("b");
            values.Push("c");
            values.Push("d");
            values.Push("e");
            values.Push("f");
            values.Push("g");
            Assert.That.IsQualifiedName(name, values);
            Assert.That.NotContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 合法的多重限定隐式self参数名称
            var parser = LanguageParserTests.CreateLanguageParser(" a.b.c.d.e.f:g ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("a");
            values.Push("b");
            values.Push("c");
            values.Push("d");
            values.Push("e");
            values.Push("f");
            values.Push("g");
            Assert.That.IsImplicitSelfParameterName(name, values);
            Assert.That.NotContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 多重限定名称缺少点标志
            // 会被分拆成两个名称语法。
            var parser = LanguageParserTests.CreateLanguageParser(" a.b.c d.e.f ");
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("a");
                values.Push("b");
                values.Push("c");
                Assert.That.IsQualifiedName(name, values);
                Assert.That.NotContainsDiagnostics(name);
                Assert.That.NotAtEndOfFile(parser);
            }
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("d");
                values.Push("e");
                values.Push("f");
                Assert.That.IsQualifiedName(name, values);
                Assert.That.NotContainsDiagnostics(name);
                Assert.That.AtEndOfFile(parser);
            }
        }
        { // 多重限定隐式self参数名称缺少点标志
            // 会被分拆成两个名称语法。
            var parser = LanguageParserTests.CreateLanguageParser(" a.b.c d.e:f ");
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("a");
                values.Push("b");
                values.Push("c");
                Assert.That.IsQualifiedName(name, values);
                Assert.That.NotContainsDiagnostics(name);
                Assert.That.NotAtEndOfFile(parser);
            }
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("d");
                values.Push("e");
                values.Push("f");
                Assert.That.IsImplicitSelfParameterName(name, values);
                Assert.That.NotContainsDiagnostics(name);
                Assert.That.AtEndOfFile(parser);
            }
        }

        { // 多重限定名称缺少标识符
            var parser = LanguageParserTests.CreateLanguageParser(" a. .c. .e. .g ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("a");
            values.Push(null);
            values.Push("c");
            values.Push(null);
            values.Push("e");
            values.Push(null);
            values.Push("g");
            Assert.That.IsQualifiedName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }
        { // 多重限定隐式self参数名称缺少标识符
            var parser = LanguageParserTests.CreateLanguageParser(" a. .c. .e.:g ");
            var name = parser.ParseName();
            var values = new Stack<string?>();
            values.Push("a");
            values.Push(null);
            values.Push("c");
            values.Push(null);
            values.Push("e");
            values.Push(null);
            values.Push("g");
            Assert.That.IsImplicitSelfParameterName(name, values);
            Assert.That.ContainsDiagnostics(name);
            Assert.That.AtEndOfFile(parser);
        }

        { // 隐式self参数名称后错误追加限定、隐式self参数名称语法
            // 将跳过第一个合法的隐式self参数语法后的所有限定、隐式self参数语法。
            var parser = LanguageParserTests.CreateLanguageParser(" a:b.c d:e:f ");
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("a");
                values.Push("b");
                Assert.That.IsImplicitSelfParameterName(name, values);
                Assert.That.ContainsDiagnostics(name);
                Assert.That.NotAtEndOfFile(parser);
            }
            {
                var name = parser.ParseName();
                var values = new Stack<string?>();
                values.Push("d");
                values.Push("e");
                Assert.That.IsImplicitSelfParameterName(name, values);
                Assert.That.ContainsDiagnostics(name);
                Assert.That.AtEndOfFile(parser);
            }
        }
    }
    #endregion

    #region 表达式
    [TestMethod]
    public void LiteralExpressionParseTests()
    {
        var parser = LanguageParserTests.CreateLanguageParser("""
            nil
            false
            true
            1
            1.0
            'string'
            ""
            """);
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.NilLiteralExpression, SyntaxKind.NilKeyword);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.NilLiteralExpression);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.FalseLiteralExpression, SyntaxKind.FalseKeyword);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.FalseLiteralExpression);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.TrueLiteralExpression, SyntaxKind.TrueKeyword);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.TrueLiteralExpression);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.NumericLiteralExpression, 1L);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.NumericLiteralExpression, 1D);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxKind.StringLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.StringLiteralExpression, "string");
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.NotAtEndOfFile(parser);
        }
        {
            var literal = parser.ParseLiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxKind.StringLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.StringLiteralExpression, string.Empty);
            Assert.That.NotContainsDiagnostics(literal);
            Assert.That.AtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void ParenthesizedExpressionParseTests()
    {
        var tree = new Tree<SyntaxKind>();
        { // 合法的括号
            var parser = LanguageParserTests.CreateLanguageParser(" (a) ");
            var expr = parser.ParseParenthesizedExpression();
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.ParenthesizedExpression) { SyntaxKind.IdentifierName };
            Assert.That.IsParenthesizedExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 不合法的空的括号
            var parser = LanguageParserTests.CreateLanguageParser(" () ");
            var expr = parser.ParseParenthesizedExpression();
            Assert.That.IsParenthesizedExpression(expr);
            Assert.That.ContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 不合法的非空的括号，右括号缺失
            var parser = LanguageParserTests.CreateLanguageParser(" (a 1.0");
            {
                var expr = parser.ParseParenthesizedExpression();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.ParenthesizedExpression) { SyntaxKind.IdentifierName };
                Assert.That.IsParenthesizedExpression(expr, root);
                Assert.That.ContainsDiagnostics(expr);
                Assert.That.NotAtEndOfFile(parser);
            }
            {
                var expr = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
                Assert.That.IsLiteralExpression(expr, SyntaxKind.NumericLiteralExpression, 1D);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }
        }
    }

    [TestMethod]
    public void ExpressionWithOperatorParseTests()
    {
        #region 基础运算式
        #region 一元运算式
        { // 取负
            var parser = LanguageParserTests.CreateLanguageParser(" -1 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsUnaryExpression(expr, SyntaxKind.UnaryMinusExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 逻辑非
            var parser = LanguageParserTests.CreateLanguageParser(" not true ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsUnaryExpression(expr, SyntaxKind.LogicalNotExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 取长度
            var parser = LanguageParserTests.CreateLanguageParser(" #t ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsUnaryExpression(expr, SyntaxKind.LengthExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位非
            var parser = LanguageParserTests.CreateLanguageParser(" ~1 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsUnaryExpression(expr, SyntaxKind.BitwiseNotExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        #endregion
        #region 二元运算式
        { // 加法
            var parser = LanguageParserTests.CreateLanguageParser(" 1 + 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.AdditionExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 减法
            var parser = LanguageParserTests.CreateLanguageParser(" 1 - 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.SubtractionExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 乘法
            var parser = LanguageParserTests.CreateLanguageParser(" 1 * 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.MultiplicationExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 除法
            var parser = LanguageParserTests.CreateLanguageParser(" 1 / 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.DivisionExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 向下取整除法
            var parser = LanguageParserTests.CreateLanguageParser(" 1 // 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.FloorDivisionExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 取幂
            var parser = LanguageParserTests.CreateLanguageParser(" 1 ^ 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.ExponentiationExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 取模
            var parser = LanguageParserTests.CreateLanguageParser(" 1 % 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.ModuloExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位与
            var parser = LanguageParserTests.CreateLanguageParser(" 1 & 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.BitwiseAndExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位异或
            var parser = LanguageParserTests.CreateLanguageParser(" 1 ~ 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.BitwiseExclusiveOrExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位或
            var parser = LanguageParserTests.CreateLanguageParser(" 1 | 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.BitwiseOrExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位左移
            var parser = LanguageParserTests.CreateLanguageParser(" 1 << 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.BitwiseLeftShiftExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 按位右移
            var parser = LanguageParserTests.CreateLanguageParser(" 1 >> 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.BitwiseRightShiftExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 连接
            var parser = LanguageParserTests.CreateLanguageParser(" '1' .. '2' ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.ConcatenationExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 小于
            var parser = LanguageParserTests.CreateLanguageParser(" 1 < 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.LessThanExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 小于等于
            var parser = LanguageParserTests.CreateLanguageParser(" 1 <= 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.LessThanOrEqualExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 大于
            var parser = LanguageParserTests.CreateLanguageParser(" 1 > 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.GreaterThanExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 大于等于
            var parser = LanguageParserTests.CreateLanguageParser(" 1 >= 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.GreaterThanOrEqualExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 相等
            var parser = LanguageParserTests.CreateLanguageParser(" 1 == 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.EqualExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 不等
            var parser = LanguageParserTests.CreateLanguageParser(" 1 ~= 2 ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.NotEqualExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 逻辑与
            var parser = LanguageParserTests.CreateLanguageParser(" true and false ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.AndExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 逻辑或
            var parser = LanguageParserTests.CreateLanguageParser(" true or false ");
            var expr = parser.ParseExpressionWithOperator();
            Assert.That.IsBinaryExpression(expr, SyntaxKind.OrExpression);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        #endregion
        #endregion

        #region 组合运算式
        var unaryExpressionOperatorTokens = (from SyntaxKind kind in Enum.GetValues(typeof(SyntaxKind))
                                             where SyntaxFacts.IsUnaryExpressionOperatorToken(kind)
                                             select kind)
                                             .ToArray();
        var binaryExpressionOperatorTokens = (from SyntaxKind kind in Enum.GetValues(typeof(SyntaxKind))
                                              where SyntaxFacts.IsBinaryExpressionOperatorToken(kind)
                                              select kind)
                                              .ToArray();
        var leftAssociativeBinaryExpressionOperatorTokens = (from kind in binaryExpressionOperatorTokens
                                                             where SyntaxFacts.IsLeftAssociativeBinaryExpressionOperatorToken(kind)
                                                             select kind)
                                                             .ToArray();
        var rightAssociativeBinaryExpressionOperatorTokens = (from kind in binaryExpressionOperatorTokens
                                                              where SyntaxFacts.IsRightAssociativeBinaryExpressionOperatorToken(kind)
                                                              select kind)
                                                              .ToArray();
        { // 两个二元运算式
            var tree = new Tree<SyntaxKind>();

            static void FirstAssociativeBinaryExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" 1 {SyntaxFacts.GetText(first)} a {SyntaxFacts.GetText(second)} 'string' ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(second))
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(first))
                    {
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxKind.IdentifierName
                    },
                    SyntaxKind.StringLiteralExpression
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }
            static void SecondAssociativeBinaryExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" 1 {SyntaxFacts.GetText(first)} a {SyntaxFacts.GetText(second)} 'string' ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(first))
                {
                    SyntaxKind.NumericLiteralExpression,
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(second))
                    {
                        SyntaxKind.IdentifierName,
                        SyntaxKind.StringLiteralExpression
                    }
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }

            { // 左结合运算式，两个运算符相同
                var tokens = leftAssociativeBinaryExpressionOperatorTokens;
                foreach (var token in tokens)
                    FirstAssociativeBinaryExpressionParseTest(token, token, tree);
            }
            { // 右结合运算式，两个运算符相同
                var tokens = rightAssociativeBinaryExpressionOperatorTokens;
                foreach (var token in tokens)
                    SecondAssociativeBinaryExpressionParseTest(token, token, tree);
            }
            { // 左结合运算式，两个运算符优先级相同
                foreach (var first in leftAssociativeBinaryExpressionOperatorTokens)
                    foreach (var second in leftAssociativeBinaryExpressionOperatorTokens)
                    {
                        if (SyntaxFacts.GetOperatorPrecedence(first, false) != SyntaxFacts.GetOperatorPrecedence(second, false)) continue;
                        FirstAssociativeBinaryExpressionParseTest(first, second, tree);
                    }
            }
            { // 右结合运算式，两个运算符优先级相同
                foreach (var first in rightAssociativeBinaryExpressionOperatorTokens)
                    foreach (var second in rightAssociativeBinaryExpressionOperatorTokens)
                    {
                        if (SyntaxFacts.GetOperatorPrecedence(first, false) != SyntaxFacts.GetOperatorPrecedence(second, false)) continue;
                        SecondAssociativeBinaryExpressionParseTest(first, second, tree);
                    }
            }
            { // 两个运算符优先级不相同
                foreach (var first in binaryExpressionOperatorTokens)
                    foreach (var second in binaryExpressionOperatorTokens)
                    {
                        var firstPrecedence = SyntaxFacts.GetOperatorPrecedence(first, false);
                        var secondPrecedence = SyntaxFacts.GetOperatorPrecedence(second, false);
                        if (firstPrecedence < secondPrecedence)
                            SecondAssociativeBinaryExpressionParseTest(first, second, tree);
                        else if (firstPrecedence > secondPrecedence)
                            FirstAssociativeBinaryExpressionParseTest(first, second, tree);
                    }
            }
            { // 第一个为左结合运算符，第二个为右结合运算符，两个运算符优先级相同
                foreach (var first in leftAssociativeBinaryExpressionOperatorTokens)
                    foreach (var second in rightAssociativeBinaryExpressionOperatorTokens)
                    {
                        if (SyntaxFacts.GetOperatorPrecedence(first, false) != SyntaxFacts.GetOperatorPrecedence(second, false)) continue;
                        SecondAssociativeBinaryExpressionParseTest(first, second, tree);
                    }
            }
            { // 第一个为右结合运算符，第二个为左结合运算符，两个运算符优先级相同
                foreach (var first in rightAssociativeBinaryExpressionOperatorTokens)
                    foreach (var second in leftAssociativeBinaryExpressionOperatorTokens)
                    {
                        if (SyntaxFacts.GetOperatorPrecedence(first, false) != SyntaxFacts.GetOperatorPrecedence(second, false)) continue;
                        FirstAssociativeBinaryExpressionParseTest(first, second, tree);
                    }
            }
        }

        { // 两个一元运算式
            var tree = new Tree<SyntaxKind>();

            static void UnaryExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" {SyntaxFacts.GetText(first)} {SyntaxFacts.GetText(second)} a ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(first))
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(second))
                    {
                        SyntaxKind.IdentifierName
                    }
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }

            foreach (var first in unaryExpressionOperatorTokens)
                foreach (var second in unaryExpressionOperatorTokens)
                {
                    UnaryExpressionParseTest(first, second, tree);
                }
        }

        { // 第一个为二元运算符，第二个为一元运算符
            var tree = new Tree<SyntaxKind>();

            static void UnaryAssociativeExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" 1 {SyntaxFacts.GetText(first)} {SyntaxFacts.GetText(second)} a ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(first))
                {
                    SyntaxKind.NumericLiteralExpression,
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(second))
                    {
                        SyntaxKind.IdentifierName
                    }
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }

            foreach (var first in binaryExpressionOperatorTokens)
                foreach (var second in unaryExpressionOperatorTokens)
                {
                    UnaryAssociativeExpressionParseTest(first, second, tree);
                }
        }

        { // 第一个为一元运算符，第二个为二元运算符
            var tree = new Tree<SyntaxKind>();

            static void UnaryAssociativeExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" {SyntaxFacts.GetText(first)} 1 {SyntaxFacts.GetText(second)} a ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(second))
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(first))
                    {
                        SyntaxKind.NumericLiteralExpression
                    },
                    SyntaxKind.IdentifierName
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }
            static void BinaryAssociativeExpressionParseTest(SyntaxKind first, SyntaxKind second, Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" {SyntaxFacts.GetText(first)} 1 {SyntaxFacts.GetText(second)} a ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(first))
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(second))
                    {
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxKind.IdentifierName
                    }
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }

            foreach (var first in unaryExpressionOperatorTokens)
                foreach (var second in binaryExpressionOperatorTokens)
                {
                    var firstPrecedence = SyntaxFacts.GetOperatorPrecedence(first, true);
                    var secondPrecedence = SyntaxFacts.GetOperatorPrecedence(second, false);
                    if (firstPrecedence < secondPrecedence)
                        BinaryAssociativeExpressionParseTest(first, second, tree);
                    else if (firstPrecedence > secondPrecedence)
                        UnaryAssociativeExpressionParseTest(first, second, tree);
                }
        }
        #endregion

        #region 括号表达式
        {
            var tree = new Tree<SyntaxKind>();

            static void ParenthesizedExpressionParseTest(
                SyntaxKind first,
                SyntaxKind second,
                SyntaxKind third,
                SyntaxKind forth,
                Tree<SyntaxKind> tree)
            {
                var parser = LanguageParserTests.CreateLanguageParser($" {SyntaxFacts.GetText(first)}((1 {SyntaxFacts.GetText(second)} a){SyntaxFacts.GetText(third)}('string' {SyntaxFacts.GetText(forth)} false)) ");
                var expr = parser.ParseExpressionWithOperator();
                var root = new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetUnaryExpression(first))
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxKind.ParenthesizedExpression)
                    {
                        new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(third))
                        {
                            new TreeNode<SyntaxKind>(tree, SyntaxKind.ParenthesizedExpression)
                            {
                                new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(second))
                                {
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxKind.IdentifierName
                                }
                            },
                            new TreeNode<SyntaxKind>(tree, SyntaxKind.ParenthesizedExpression)
                            {
                                new TreeNode<SyntaxKind>(tree, SyntaxFacts.GetBinaryExpression(forth))
                                {
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxKind.FalseLiteralExpression
                                }
                            }
                        }
                    }
                };
                Assert.That.IsExpression(expr, root);
                Assert.That.NotContainsDiagnostics(expr);
                Assert.That.AtEndOfFile(parser);
            }

            foreach (var first in unaryExpressionOperatorTokens)
                foreach (var second in binaryExpressionOperatorTokens)
                    foreach (var third in binaryExpressionOperatorTokens)
                        foreach (var forth in binaryExpressionOperatorTokens)
                        {
                            ParenthesizedExpressionParseTest(first, second, third, forth, tree);
                        }
        }
        #endregion
    }

    [TestMethod]
    public void MemberAccessExpressionParseTests()
    {
        var tree = new Tree<SyntaxKind>();
        { // 通过普通成员操作语法获取标识符的成员
            var parser = LanguageParserTests.CreateLanguageParser("a.b");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");

            Assert.That.IsIdentifierName(expr.MemberName, "b");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过普通成员操作语法获取整型数字常量的成员
            var parser = LanguageParserTests.CreateLanguageParser("1.GetType");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken));
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr.Self, SyntaxKind.NumericLiteralExpression, 1L);

            Assert.That.IsIdentifierName(expr.MemberName, "GetType");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过普通成员操作语法获取浮点型数字常量的成员
            var parser = LanguageParserTests.CreateLanguageParser("1.0.ToString");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken));
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr.Self, SyntaxKind.NumericLiteralExpression, 1D);

            Assert.That.IsIdentifierName(expr.MemberName, "ToString");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过普通成员操作语法获取标识符的成员，但成员错误使用了整型数字常量
            // 解析器会将其解析成一个实际为“a”的部分缺失的普通成员操作表达式，和一个实际为“.1”的浮点型数字常量表达式。
            var parser = LanguageParserTests.CreateLanguageParser("a.1");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.ContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");
            Assert.That.NotContainsDiagnostics(expr.Self);

            Assert.That.IsMissing(expr.OperatorToken);

            Assert.That.IsMissingIdentifierName(expr.MemberName);
            Assert.That.ContainsDiagnostics(expr.MemberName);

            Assert.That.NotAtEndOfFile(parser);

            var literal = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.NumericLiteralExpression, 0.1D);
            Assert.That.NotContainsDiagnostics(expr.Self);

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过普通成员操作语法获取标识符的成员，但成员错误使用了浮点型数字常量
            // 解析器会将其解析成一个实际为“a”的部分缺失的普通成员操作表达式，后为一个实际为“.1”的浮点型数字常量表达式，最后为一个实际为“.0”的浮点型数字常量表达式。
            var parser = LanguageParserTests.CreateLanguageParser("a.1.0");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.ContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");
            Assert.That.ContainsDiagnostics(expr);

            Assert.That.IsMissing(expr.OperatorToken);

            Assert.That.IsMissingIdentifierName(expr.MemberName);
            Assert.That.ContainsDiagnostics(expr);

            Assert.That.NotAtEndOfFile(parser);

            {
                var literal = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
                Assert.That.IsLiteralExpression(literal, SyntaxKind.NumericLiteralExpression, 0.1D);
                Assert.That.NotContainsDiagnostics(expr.Self);
            }
            {
                var literal = parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken);
                Assert.That.IsLiteralExpression(literal, SyntaxKind.NumericLiteralExpression, 0D);
                Assert.That.NotContainsDiagnostics(expr.Self);
            }

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过普通成员操作语法获取标识符的成员，但成员错误使用了字符串常量
            // 解析器会将其解析成一个实际为“a.”部分缺失的普通成员操作表达式，后为一个字符串常量表达式。
            var parser = LanguageParserTests.CreateLanguageParser("a.'string'");
            var expr = parser.ParseSimpleMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsSimpleMemberAccessExpression(expr, root);
            Assert.That.ContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");
            Assert.That.ContainsDiagnostics(expr);

            Assert.That.IsNotMissing(expr.OperatorToken);

            Assert.That.IsMissingIdentifierName(expr.MemberName);
            Assert.That.ContainsDiagnostics(expr);

            Assert.That.NotAtEndOfFile(parser);

            var literal = parser.ParseLiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxKind.StringLiteralToken);
            Assert.That.IsLiteralExpression(literal, SyntaxKind.StringLiteralExpression, "string");
            Assert.That.NotContainsDiagnostics(expr.Self);

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过索引成员操作语法获取标识符的成员，索引是标识符
            var parser = LanguageParserTests.CreateLanguageParser("a[b]");
            var expr = parser.ParseIndexMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.IndexMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.IdentifierName
            };
            Assert.That.IsIndexMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");

            Assert.IsInstanceOfType(expr.Member, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Member, "b");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过索引成员操作语法获取标识符的成员，索引是常量
            var parser = LanguageParserTests.CreateLanguageParser("a[\"b\"]");
            var expr = parser.ParseIndexMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.IndexMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                SyntaxKind.StringLiteralExpression
            };
            Assert.That.IsIndexMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");

            Assert.IsInstanceOfType(expr.Member, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr.Member, SyntaxKind.StringLiteralExpression, "b");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过索引成员操作语法获取常量表达式的成员，索引是常量
            var parser = LanguageParserTests.CreateLanguageParser("1['ToString']");
            var expr = parser.ParseIndexMemberAccessExpressionSyntax(parser.ParseLiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxKind.NumericLiteralToken));
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.IndexMemberAccessExpression)
            {
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.StringLiteralExpression
            };
            Assert.That.IsIndexMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr.Self, SyntaxKind.NumericLiteralExpression, 1L);

            Assert.IsInstanceOfType(expr.Member, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr.Member, SyntaxKind.StringLiteralExpression, "ToString");

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过索引成员操作语法获取标识符的成员，索引是表达式
            var parser = LanguageParserTests.CreateLanguageParser("a[1.0..[[string]]]");
            var expr = parser.ParseIndexMemberAccessExpressionSyntax(parser.ParseIdentifierName());
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.IndexMemberAccessExpression)
            {
                SyntaxKind.IdentifierName,
                new TreeNode<SyntaxKind>(tree, SyntaxKind.ConcatenationExpression)
                {
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxKind.StringLiteralExpression
                }
            };
            Assert.That.IsIndexMemberAccessExpression(expr, root);
            Assert.That.NotContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr.Self, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr.Self, "a");

            Assert.IsInstanceOfType(expr.Member, typeof(BinaryExpressionSyntax));
            var binary = (BinaryExpressionSyntax)expr.Member;
            {
                Assert.IsInstanceOfType(binary.Left, typeof(LiteralExpressionSyntax));
                Assert.That.IsLiteralExpression((LiteralExpressionSyntax)binary.Left, SyntaxKind.NumericLiteralExpression, 1D);

                Assert.IsInstanceOfType(binary.Right, typeof(LiteralExpressionSyntax));
                Assert.That.IsLiteralExpression((LiteralExpressionSyntax)binary.Right, SyntaxKind.StringLiteralExpression, "string");
            }

            Assert.That.AtEndOfFile(parser);
        }
        { // 通过索引成员操作语法获取标识符的成员，但错误输入普通成员操作语法
            var parser = LanguageParserTests.CreateLanguageParser("a.[b].c");
            var expr = parser.ParseExpression();
            var root = new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
            {
                new TreeNode<SyntaxKind>(tree, SyntaxKind.IndexMemberAccessExpression)
                {
                    new TreeNode<SyntaxKind>(tree, SyntaxKind.SimpleMemberAccessExpression)
                    {
                        SyntaxKind.IdentifierName,
                        SyntaxKind.IdentifierName
                    },
                    SyntaxKind.IdentifierName
                },
                SyntaxKind.IdentifierName
            };
            Assert.That.IsExpression(expr, root);
            Assert.That.ContainsDiagnostics(expr);

            Assert.IsInstanceOfType(expr, typeof(SimpleMemberAccessExpressionSyntax));
            var outerSimple = (SimpleMemberAccessExpressionSyntax)expr;
            {
                Assert.That.ContainsDiagnostics(outerSimple);

                Assert.That.IsIdentifierName(outerSimple.MemberName, "c");
                Assert.That.NotContainsDiagnostics(outerSimple.MemberName);
            }

            Assert.IsInstanceOfType(outerSimple.Self, typeof(IndexMemberAccessExpressionSyntax));
            var innerIndex = (IndexMemberAccessExpressionSyntax)outerSimple.Self;
            {
                Assert.That.ContainsDiagnostics(innerIndex);

                Assert.IsInstanceOfType(innerIndex.Member, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)innerIndex.Member, "b");
                Assert.That.NotContainsDiagnostics(innerIndex.Member);
            }

            Assert.IsInstanceOfType(innerIndex.Self, typeof(SimpleMemberAccessExpressionSyntax));
            var innerSimple = (SimpleMemberAccessExpressionSyntax)innerIndex.Self;
            {
                Assert.That.ContainsDiagnostics(innerSimple);

                Assert.IsInstanceOfType(innerSimple.Self, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)innerSimple.Self, "a");
                Assert.That.NotContainsDiagnostics(innerSimple.Self);

                Assert.That.IsMissingIdentifierName(innerSimple.MemberName);
                Assert.That.ContainsDiagnostics(innerSimple.MemberName);
            }

            Assert.That.AtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void TableConstructorExpressionParseTests()
    {
        var parser = LanguageParserTests.CreateLanguageParser($"{{{FieldListSouce}}}");
        var table = parser.ParseTableConstructorExpression();
        Assert.That.NotContainsDiagnostics(table);
        FieldListTest(table.Fields);
    }

    [TestMethod]
    public void InvocationExpressionParseTests()
    {
        var parser = LanguageParserTests.CreateLanguageParser("""
            empty()
            list(a,2)
            table{a=1,2}
            print[[line]]
            """);
        { // 空的参数列表
            var invocation = parser.ParseInvocationExpressionSyntax(parser.ParseIdentifierName());
            Assert.That.NotContainsDiagnostics(invocation);

            Assert.That.IsIdentifierName((IdentifierNameSyntax)invocation.Expression, "empty");

            Assert.IsInstanceOfType(invocation.Arguments, typeof(ArgumentListSyntax));
            Assert.That.IsEmptyArgumentList((ArgumentListSyntax)invocation.Arguments);

            Assert.That.NotAtEndOfFile(parser);
        }
        { // 参数列表
            var invocation = parser.ParseInvocationExpressionSyntax(parser.ParseIdentifierName());
            Assert.That.NotContainsDiagnostics(invocation);

            Assert.That.IsIdentifierName((IdentifierNameSyntax)invocation.Expression, "list");

            Assert.IsInstanceOfType(invocation.Arguments, typeof(ArgumentListSyntax));
            var arguments = (ArgumentListSyntax)invocation.Arguments;
            Assert.That.IsNotEmptyArgumentList(arguments, 2);

            {
                Assert.IsInstanceOfType(arguments.List[0]!.Expression, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)arguments.List[0]!.Expression, "a");

                Assert.IsInstanceOfType(arguments.List[1]!.Expression, typeof(LiteralExpressionSyntax));
                Assert.That.IsLiteralExpression((LiteralExpressionSyntax)arguments.List[1]!.Expression, SyntaxKind.NumericLiteralExpression, 2L);
            }

            Assert.That.NotAtEndOfFile(parser);
        }
        { // 参数表
            var invocation = parser.ParseInvocationExpressionSyntax(parser.ParseIdentifierName());
            Assert.That.NotContainsDiagnostics(invocation);

            Assert.That.IsIdentifierName((IdentifierNameSyntax)invocation.Expression, "table");

            Assert.IsInstanceOfType(invocation.Arguments, typeof(ArgumentTableSyntax));
            var arguments = (ArgumentTableSyntax)invocation.Arguments;
            Assert.That.IsNotEmptyArgumentTable(arguments, 2);

            {
                Assert.IsInstanceOfType(arguments.Table.Fields.Fields[0], typeof(NameValueFieldSyntax));
                var field = (NameValueFieldSyntax)arguments.Table.Fields.Fields[0]!;
                Assert.That.IsIdentifierName(field.FieldName, "a");
                Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 1L);
            }
            {
                Assert.IsInstanceOfType(arguments.Table.Fields.Fields[1], typeof(ItemFieldSyntax));
                var field = (ItemFieldSyntax)arguments.Table.Fields.Fields[1]!;
                Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 2L);
            }

            Assert.That.NotAtEndOfFile(parser);
        }
        { // 参数字符串
            var invocation = parser.ParseInvocationExpressionSyntax(parser.ParseIdentifierName());
            Assert.That.NotContainsDiagnostics(invocation);

            Assert.That.IsIdentifierName((IdentifierNameSyntax)invocation.Expression, "print");

            Assert.IsInstanceOfType(invocation.Arguments, typeof(ArgumentStringSyntax));
            var arguments = (ArgumentStringSyntax)invocation.Arguments;

            Assert.IsInstanceOfType(arguments.String, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression(arguments.String, SyntaxKind.StringLiteralExpression, "line");
        }
    }
    #endregion

    #region 字段
    [TestMethod]
    public void FieldValueParseTests()
    {
        { // 字段值为合法表达式
            var parser = LanguageParserTests.CreateLanguageParser("1");
            var expr = parser.ParseFieldValue();
            Assert.IsInstanceOfType(expr, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)expr, SyntaxKind.NumericLiteralExpression, 1L);
            Assert.That.NotContainsDiagnostics(expr);
            Assert.That.AtEndOfFile(parser);
        }
        { // 字段值为合法表达式，但后方错误追加了其他标志和表达式
            // 只识别第一个合法表达式，后方的标志和表达式作为前者的被跳过的标志的语法琐碎内容，并报告错误。
            var parser = LanguageParserTests.CreateLanguageParser("a if b + c then return true end");
            var expr = parser.ParseFieldValue();
            Assert.IsInstanceOfType(expr, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)expr, "a");
            Assert.That.ContainsDiagnostics(expr);
            Assert.That.ContainsDiagnostics(expr.GetLastToken()!, ErrorCode.ERR_InvalidFieldValueTerm);
            Assert.That.AtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void NameValueFieldParseTests()
    {
        { // 合法的名值对字段
            var parser = LanguageParserTests.CreateLanguageParser("a = 1");
            var field = parser.ParseNameValueField();
            Assert.That.NotContainsDiagnostics(field);

            Assert.That.IsIdentifierName(field.FieldName, "a");

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 1L);

            Assert.That.AtEndOfFile(parser);
        }
        { // 不合法的名值对字段，缺少名称
            var parser = LanguageParserTests.CreateLanguageParser(" = 'string'");
            var field = parser.ParseNameValueField();
            Assert.That.ContainsDiagnostics(field);

            Assert.That.IsMissingIdentifierName(field.FieldName);
            Assert.That.ContainsDiagnostics(field);

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.StringLiteralExpression, "string");
            Assert.That.NotContainsDiagnostics(field.FieldValue);

            Assert.That.AtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void KeyValueFieldParseTests()
    {
        { // 合法的键值对字段
            var parser = LanguageParserTests.CreateLanguageParser("[a] = 1");
            var field = parser.ParseKeyValueField();
            Assert.That.NotContainsDiagnostics(field);

            Assert.IsInstanceOfType(field.FieldKey, typeof(IdentifierNameSyntax));
            Assert.That.IsIdentifierName((IdentifierNameSyntax)field.FieldKey, "a");

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 1L);

            Assert.That.AtEndOfFile(parser);
        }
        { // 不合法的键值对字段，缺失键表达式
            var parser = LanguageParserTests.CreateLanguageParser("[] = 1");
            var field = parser.ParseKeyValueField();
            Assert.That.ContainsDiagnostics(field);

            Assert.IsInstanceOfType(field.FieldKey, typeof(IdentifierNameSyntax));
            Assert.That.IsMissingIdentifierName((IdentifierNameSyntax)field.FieldKey);
            Assert.That.ContainsDiagnostics(field.FieldKey);
            Assert.That.ContainsDiagnostics(field.FieldKey, ErrorCode.ERR_InvalidExprTerm);

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 1L);
            Assert.That.NotContainsDiagnostics(field.FieldValue);

            Assert.That.AtEndOfFile(parser);
        }
        { // 不合法的键值对字段，缺失值表达式
            var parser = LanguageParserTests.CreateLanguageParser("['string'] = ");
            var field = parser.ParseKeyValueField();
            Assert.That.ContainsDiagnostics(field);

            Assert.IsInstanceOfType(field.FieldKey, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldKey, SyntaxKind.StringLiteralExpression, "string");
            Assert.That.NotContainsDiagnostics(field.FieldKey);

            Assert.IsInstanceOfType(field.FieldValue, typeof(IdentifierNameSyntax));
            Assert.That.IsMissingIdentifierName((IdentifierNameSyntax)field.FieldValue);
            Assert.That.ContainsDiagnostics(field);
        }
        { // 不合法的键值对字段，键表达式未使用方括号包裹
            var parser = LanguageParserTests.CreateLanguageParser("1.0 = true");
            var field = parser.ParseKeyValueField();
            Assert.That.ContainsDiagnostics(field);

            Assert.That.IsMissing(field.OpenBracketToken);
            Assert.That.ContainsDiagnostics(field.OpenBracketToken);

            Assert.IsInstanceOfType(field.FieldKey, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldKey, SyntaxKind.NumericLiteralExpression, 1D);
            Assert.That.NotContainsDiagnostics(field.FieldKey);

            Assert.That.IsMissing(field.CloseBracketToken);
            Assert.That.ContainsDiagnostics(field.CloseBracketToken);

            Assert.That.IsNotMissing(field.EqualsToken);
            Assert.That.NotContainsDiagnostics(field.EqualsToken);

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.TrueLiteralExpression);
            Assert.That.NotContainsDiagnostics(field.FieldValue);

            Assert.That.AtEndOfFile(parser);
        }
    }

    [TestMethod]
    public void FieldListParseTests()
    {
        var parser = LanguageParserTests.CreateLanguageParser(FieldListSouce);
        var fieldList = parser.ParseFieldList();
        FieldListTest(fieldList);
    }

    private const string FieldListSouce = """
        integer = 5,
        float = 5.0,
        [a+b]=c^d,
        true,
        'string'
        """;
    private static void FieldListTest(FieldListSyntax fieldList)
    {
        Assert.That.NotContainsDiagnostics(fieldList);
        {
            Assert.IsInstanceOfType(fieldList.Fields[0]!, typeof(NameValueFieldSyntax));
            var field = (NameValueFieldSyntax)fieldList.Fields[0]!;

            Assert.That.IsIdentifierName(field.FieldName, "integer");

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 5L);
        }
        {
            Assert.IsInstanceOfType(fieldList.Fields[1]!, typeof(NameValueFieldSyntax));
            var field = (NameValueFieldSyntax)fieldList.Fields[1]!;

            Assert.That.IsIdentifierName(field.FieldName, "float");

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.NumericLiteralExpression, 5D);
        }
        {
            Assert.IsInstanceOfType(fieldList.Fields[2]!, typeof(KeyValueFieldSyntax));
            var field = (KeyValueFieldSyntax)fieldList.Fields[2]!;

            Assert.That.IsBinaryExpression(field.FieldKey, SyntaxKind.AdditionExpression);
            {
                var binary = (BinaryExpressionSyntax)field.FieldKey;

                Assert.IsInstanceOfType(binary.Left, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)binary.Left, "a");

                Assert.IsInstanceOfType(binary.Right, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)binary.Right, "b");
            }

            Assert.That.IsBinaryExpression(field.FieldValue, SyntaxKind.ExponentiationExpression);
            {
                var binary = (BinaryExpressionSyntax)field.FieldValue;

                Assert.IsInstanceOfType(binary.Left, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)binary.Left, "c");

                Assert.IsInstanceOfType(binary.Right, typeof(IdentifierNameSyntax));
                Assert.That.IsIdentifierName((IdentifierNameSyntax)binary.Right, "d");
            }
        }
        {
            Assert.IsInstanceOfType(fieldList.Fields[3]!, typeof(ItemFieldSyntax));
            var field = (ItemFieldSyntax)fieldList.Fields[3]!;

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.TrueLiteralExpression);
        }
        {
            Assert.IsInstanceOfType(fieldList.Fields[4]!, typeof(ItemFieldSyntax));
            var field = (ItemFieldSyntax)fieldList.Fields[4]!;

            Assert.IsInstanceOfType(field.FieldValue, typeof(LiteralExpressionSyntax));
            Assert.That.IsLiteralExpression((LiteralExpressionSyntax)field.FieldValue, SyntaxKind.StringLiteralExpression, "string");
        }
    }
    #endregion
}
