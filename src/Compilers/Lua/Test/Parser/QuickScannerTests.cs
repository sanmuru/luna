using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using SamLu.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace SamLu.CodeAnalysis.Lua.Parser.UnitTests;

[TestClass]
public partial class QuickScannerTests
{
    private static IEnumerable<char> GetCharByFlag(CharFlag flag)
    {
        var properties = QuickScannerTests.CharProperties.ToArray();
        for (int i = 0, n = properties.Length; i < n; i++)
        {
            var b = properties[i];
            if (b == (byte)flag)
                yield return (char)i;
        }
        if (flag == CharFlag.Complex)
            yield return (char)0x181;
    }

    private static CharFlag GetFlag(char c) => c < 0x180 ? (CharFlag)QuickScannerTests.CharProperties[c] : CharFlag.Complex;

    private static IEnumerable<string> Run()
    {
        return RunInternal(QuickScanState.Initial, string.Empty);

        static IEnumerable<string> RunInternal(QuickScanState state, string buffer)
        {
            for (int uc = 0; uc < 0x181; uc++)
            {
                var nextChar = (char)uc;
                var nextFlag = QuickScannerTests.GetFlag(nextChar);

                state = (QuickScanState)s_stateTransitions[(int)state, (int)nextFlag];
                switch (state)
                {
                    case QuickScanState.Done:
                        yield return buffer;
                        break;
                    case QuickScanState.Bad:
                        yield break;
                    default:
                        foreach (var s in RunInternal(state, buffer + nextChar))
                            yield return s;
                        break;
                }
            }
        }
    }

    public static bool Check(string? result)
    {
        if (result is null) return true;

        var lexer = new Lexer(SourceText.From(result), LuaParseOptions.Default);
        var token = lexer.Lex(LexerMode.Syntax);

        if (token.Kind == SyntaxKind.BadToken) return false;
        if (token.FullWidth != result.Length) return false;

        return true;
    }

    [TestMethod]
    public void StateTest()
    {
        Assert.IsTrue(
            QuickScannerTests.Run()
                .All(QuickScannerTests.Check)
        );
    }
}
