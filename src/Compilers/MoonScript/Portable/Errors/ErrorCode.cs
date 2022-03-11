using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript
{
    internal enum ErrorCode
    {
        Void = InternalErrorCode.Void,
        Unknown = InternalErrorCode.Unknown,

        [Obsolete("MoonScript.NET不支持预处理指令", false)]
        ERR_InvalidPreprocessingSymbol = 1,
        ERR_InvalidInstrumentationKind,
        ERR_BadSourceCodeKind,
        ERR_BadDocumentationMode,
        ERR_BadLanguageVersion,

        // 更新编译器的警告后应手动运行（eng\generate-compiler-code.cmd）。
    }
}
