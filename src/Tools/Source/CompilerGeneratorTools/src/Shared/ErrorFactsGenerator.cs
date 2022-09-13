using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
using SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using SamLu.CodeAnalysis.MoonScript;
#endif

namespace Luna.Compilers.Generators;

using SamLu.CodeAnalysis;

[Generator]
public sealed class ErrorFactsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    internal static bool TryGetCodeNames(
        out ImmutableArray<string> warningCodeNames,
        out ImmutableArray<string> fatalCodeNames,
        out ImmutableArray<string> infoCodeNames,
        out ImmutableArray<string> hiddenCodeNames)
    {
        var wrn = new List<string>();
        var ftl = new List<string>();
        var inf = new List<string>();
        var hdn = new List<string>();
        foreach (var codeName in Enum.GetNames(typeof(ErrorCode)))
        {
            if (codeName.StartsWith("WRN_", StringComparison.OrdinalIgnoreCase))
            {
                wrn.Add(codeName);
            }
            else if (codeName.StartsWith("FTL_", StringComparison.OrdinalIgnoreCase))
            {
                ftl.Add(codeName);
            }
            else if (codeName.StartsWith("INF_", StringComparison.OrdinalIgnoreCase))
            {
                inf.Add(codeName);
            }
            else if (codeName.StartsWith("HDN_", StringComparison.OrdinalIgnoreCase))
            {
                hdn.Add(codeName);
            }
        }

        warningCodeNames = wrn.ToImmutableArray();
        fatalCodeNames = ftl.ToImmutableArray();
        infoCodeNames = inf.ToImmutableArray();
        hiddenCodeNames = hdn.ToImmutableArray();
        return true;
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var outputText = new StringBuilder();
        #region 生成源代码
        if (!TryGetCodeNames(out var warningCodeNames, out var fatalCodeNames, out var infoCodeNames, out var hiddenCodeNames)) return;

        outputText.AppendLine($"namespace SamLu.CodeAnalysis.{LanguageNames.This}");
        outputText.AppendLine("{");
        outputText.AppendLine("    internal static partial class ErrorFacts");
        outputText.AppendLine("    {");

        outputText.AppendLine("        public static bool IsWarning(ErrorCode code)");
        outputText.AppendLine("        {");
        outputText.AppendLine("            switch (code)");
        outputText.AppendLine("            {");
        if (warningCodeNames.Length != 0)
        {
            foreach (var name in warningCodeNames)
            {
                outputText.Append("                case ErrorCode.");
                outputText.Append(name);
                outputText.AppendLine(":");
            }
            outputText.AppendLine("                    return true;");
        }
        outputText.AppendLine("                default:");
        outputText.AppendLine("                    return false;");
        outputText.AppendLine("            }");
        outputText.AppendLine("        }");

        outputText.AppendLine();

        outputText.AppendLine("        public static bool IsFatal(ErrorCode code)");
        outputText.AppendLine("        {");
        outputText.AppendLine("            switch (code)");
        outputText.AppendLine("            {");
        if (fatalCodeNames.Length != 0)
        {
            foreach (var name in fatalCodeNames)
            {
                outputText.Append("                case ErrorCode.");
                outputText.Append(name);
                outputText.AppendLine(":");
            }
            outputText.AppendLine("                    return true;");
        }
        outputText.AppendLine("                default:");
        outputText.AppendLine("                    return false;");
        outputText.AppendLine("            }");
        outputText.AppendLine("        }");

        outputText.AppendLine();

        outputText.AppendLine("        public static bool IsInfo(ErrorCode code)");
        outputText.AppendLine("        {");
        outputText.AppendLine("            switch (code)");
        outputText.AppendLine("            {");
        if (infoCodeNames.Length != 0)
        {
            foreach (var name in infoCodeNames)
            {
                outputText.Append("                case ErrorCode.");
                outputText.Append(name);
                outputText.AppendLine(":");
            }
            outputText.AppendLine("                    return true;");
        }
        outputText.AppendLine("                default:");
        outputText.AppendLine("                    return false;");
        outputText.AppendLine("            }");
        outputText.AppendLine("        }");

        outputText.AppendLine();

        outputText.AppendLine("        public static bool IsHidden(ErrorCode code)");
        outputText.AppendLine("        {");
        outputText.AppendLine("            switch (code)");
        outputText.AppendLine("            {");
        if (hiddenCodeNames.Length != 0)
        {
            foreach (var name in hiddenCodeNames)
            {
                outputText.Append("                case ErrorCode.");
                outputText.Append(name);
                outputText.AppendLine(":");
            }
            outputText.AppendLine("                    return true;");
        }
        outputText.AppendLine("                default:");
        outputText.AppendLine("                    return false;");
        outputText.AppendLine("            }");
        outputText.AppendLine("        }");

        outputText.AppendLine("    }");
        outputText.AppendLine("}");
        #endregion

        // 从StringBuilder创建一个SourceText，再次避免申请一个庞大字符串的空间。
        var sourceText = SourceText.From(new StringBuilderReader(outputText), outputText.Length, encoding: Encoding.UTF8);
        context.AddSource("ErrorFacts.Generated.cs", sourceText);
    }
}
