using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

using Utilities;

[TestClass]
public partial class LanguageParserTests
{
    internal static LanguageParser CreateLanguageParser(string source, LuaParseOptions? options = null) => new(LexerTests.CreateLexer(source, options), null, null);

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
}
