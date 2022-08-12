using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

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
    public void ExceptionWithOperatorParseTests()
    {
        #region 基础操作符运算式
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
    }
    #endregion
}
