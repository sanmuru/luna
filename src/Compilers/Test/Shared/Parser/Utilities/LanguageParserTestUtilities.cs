#if LANG_LUA
using System.Data.Common;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests.Utilities;
#elif LANG_MOONSCRIPT
using SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.MoonScript.Parser.UnitTests.Utilities;
#endif

public static class LanguageParserTestUtilities
{
    internal static void ContainsDiagnostics(this Assert assert, Syntax.InternalSyntax.LuaSyntaxNode node) => Assert.IsTrue(node.ContainsDiagnostics, "未报告语法错误。");

    internal static void IsIdentifierName(this Assert assert, IdentifierNameSyntax identifierName, string name)
    {
        Assert.IsFalse(identifierName.Identifier.IsMissing, $"{nameof(identifierName)}包含的标识符名称标志缺失。");
        Assert.AreEqual(name, identifierName.Identifier.Text, $"{nameof(identifierName)}包含的标识符名称应为“{name}”，实为“{identifierName.Identifier.Text}”。");
    }

    internal static void IsMissingIdentifierName(this Assert assert, IdentifierNameSyntax identifierName)
    {
        Assert.IsTrue(identifierName.Identifier.IsMissing, $"{nameof(identifierName)}包含的标识符名称标志并未缺失。");
    }
}
