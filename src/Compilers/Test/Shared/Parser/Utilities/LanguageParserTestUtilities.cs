#if LANG_LUA
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests.Utilities;
#elif LANG_MOONSCRIPT
using SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;
using ThisInternalSyntaxNode = SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript.Parser.UnitTests.Utilities;
#endif

public static class LanguageParserTestUtilities
{
    #region AtEndOfFile
    internal static void AtEndOfFile(this Assert assert, LanguageParser parser) => Assert.IsTrue(parser.IsAtEndOfFile, "语言解析器并未抵达文件结尾。");
    internal static void NotAtEndOfFile(this Assert assert, LanguageParser parser) => Assert.IsFalse(parser.IsAtEndOfFile, "语言解析器已抵达文件结尾。");
    #endregion

    #region ContainsDiagnostics
    internal static void ContainsDiagnostics(this Assert assert, ThisInternalSyntaxNode node, params ErrorCode[] codes)
    {
        if (codes.Length == 0)
            Assert.IsTrue(node.ContainsDiagnostics, "未报告语法错误。");
        else
        {
            var diagnostics = node.GetDiagnostics();
            var unraisedCodes = codes.Where(code => !diagnostics.Any(diag => diag.Code == (int)code)).ToArray();
            if (unraisedCodes.Length != 0)
                Assert.Fail("未报告语法错误：{0}。", string.Join("、", unraisedCodes));
        }
    }

    internal static void NotContainsDiagnostics(this Assert assert, ThisInternalSyntaxNode node, params ErrorCode[] codes)
    {
        if (codes.Length == 0)
            Assert.IsFalse(node.ContainsDiagnostics, "报告语法错误。");
        else
        {
            var diagnostics = node.GetDiagnostics();
            var raisedCodes = codes.Where(code => diagnostics.Any(diag => diag.Code == (int)code)).ToArray();
            if (raisedCodes.Length != 0)
                Assert.Fail("报告语法错误：{0}。", string.Join("、", raisedCodes));
        }
    }
    #endregion

    #region IsIdentifierName
    internal static void IsIdentifierName(this Assert assert, NameSyntax name, string value)
    {
        Assert.IsInstanceOfType(name, typeof(IdentifierNameSyntax));
        assert.IsIdentifierName((IdentifierNameSyntax)name, value);
    }

    internal static void IsMissingIdentifierName(this Assert assert, NameSyntax name)
    {
        Assert.IsInstanceOfType(name, typeof(IdentifierNameSyntax));
        assert.IsMissingIdentifierName((IdentifierNameSyntax)name);
    }

    internal static void IsIdentifierName(this Assert assert, IdentifierNameSyntax identifierName, string value)
    {
        Assert.IsFalse(identifierName.Identifier.IsMissing, $"{nameof(identifierName)}包含的标识符名称标志缺失。");
        Assert.AreEqual(value, identifierName.Identifier.Text, $"{nameof(identifierName)}包含的标识符名称应为“{value}”，实为“{identifierName.Identifier.Text}”。");
    }

    internal static void IsMissingIdentifierName(this Assert assert, IdentifierNameSyntax identifierName)
    {
        Assert.IsTrue(identifierName.Identifier.IsMissing, $"{nameof(identifierName)}包含的标识符名称标志并未缺失。");
    }
    #endregion

    #region IsQualifiedName
    internal static void IsQualifiedName(this Assert assert, NameSyntax name, string value)
    {
        Assert.IsInstanceOfType(name, typeof(QualifiedNameSyntax));
        assert.IsQualifiedName((QualifiedNameSyntax)name, value);
    }

    internal static void IsMissingQualifiedName(this Assert assert, NameSyntax name)
    {
        Assert.IsInstanceOfType(name, typeof(QualifiedNameSyntax));
        assert.IsMissingIdentifierName((QualifiedNameSyntax)name);
    }

    internal static void IsQualifiedName(this Assert assert, NameSyntax name, Stack<string?> values)
    {
        Assert.IsInstanceOfType(name, typeof(QualifiedNameSyntax));
        assert.IsQualifiedName((QualifiedNameSyntax)name, values);
    }

    internal static void IsQualifiedName(this Assert assert, QualifiedNameSyntax qualifiedName, string value)
    {
        assert.IsIdentifierName(qualifiedName.right, value);
    }

    internal static void IsMissingQualifiedName(this Assert assert, QualifiedNameSyntax qualifiedName)
    {
        assert.IsMissingIdentifierName(qualifiedName.right);
    }

    internal static void IsQualifiedName(this Assert assert, QualifiedNameSyntax qualifiedName, Stack<string?> values)
    {
        if (values.Count < 2) Assert.Fail($"限定名称语法必须含有不少于2个标识符名称。{nameof(values)}中只含有{values.Count}个元素。");

        var value = values.Pop();
        if (value is not null)
            assert.IsQualifiedName(qualifiedName, value);
        else
            assert.IsMissingQualifiedName(qualifiedName);

        if (values.Count == 1)
        {
            value = values.Pop();
            if (value is not null)
                assert.IsIdentifierName(qualifiedName.Left, value);
            else
                assert.IsMissingIdentifierName(qualifiedName.Left);
        }
        else
            assert.IsQualifiedName(qualifiedName.Left, values);
    }
    #endregion

    #region IsImplicitSelfParameterName
    internal static void IsImplicitSelfParameterName(this Assert assert, NameSyntax name, string value)
    {
        Assert.IsInstanceOfType(name, typeof(ImplicitSelfParameterNameSyntax));
        assert.IsImplicitSelfParameterName((ImplicitSelfParameterNameSyntax)name, value);
    }

    internal static void IsMissingImplicitSelfParameterName(this Assert assert, NameSyntax name)
    {
        Assert.IsInstanceOfType(name, typeof(ImplicitSelfParameterNameSyntax));
        assert.IsMissingIdentifierName((ImplicitSelfParameterNameSyntax)name);
    }

    internal static void IsImplicitSelfParameterName(this Assert assert, NameSyntax name, Stack<string?> values)
    {
        Assert.IsInstanceOfType(name, typeof(ImplicitSelfParameterNameSyntax));
        assert.IsImplicitSelfParameterName((ImplicitSelfParameterNameSyntax)name, values);
    }

    internal static void IsImplicitSelfParameterName(this Assert assert, ImplicitSelfParameterNameSyntax implicitSelfParameterName, string value)
    {
        assert.IsIdentifierName(implicitSelfParameterName.right, value);
    }

    internal static void IsMissingImplicitSelfParameterName(this Assert assert, ImplicitSelfParameterNameSyntax implicitSelfParameterName)
    {
        assert.IsMissingIdentifierName(implicitSelfParameterName.right);
    }

    internal static void IsImplicitSelfParameterName(this Assert assert, ImplicitSelfParameterNameSyntax implicitSelfParameterName, Stack<string?> values)
    {
        if (values.Count < 2) Assert.Fail($"隐式self参数名称语法必须含有不少于2个标识符名称。{nameof(values)}中只含有{values.Count}个元素。");

        var value = values.Pop();
        if (value is not null)
            assert.IsImplicitSelfParameterName(implicitSelfParameterName, value);
        else
            assert.IsMissingImplicitSelfParameterName(implicitSelfParameterName);

        if (values.Count == 1)
        {
            value = values.Pop();
            if (value is not null)
                assert.IsIdentifierName(implicitSelfParameterName.Left, value);
            else
                assert.IsMissingIdentifierName(implicitSelfParameterName.Left);
        }
        else
            assert.IsQualifiedName(implicitSelfParameterName.Left, values);
    }
    #endregion

}
