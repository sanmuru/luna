﻿using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript
{
    internal enum ErrorCode
    {
        Void = InternalErrorCode.Void,
        Unknown = InternalErrorCode.Unknown,

        ERR_InternalError = 1,

        ERR_InvalidInstrumentationKind,
        ERR_BadSourceCodeKind,
        ERR_BadDocumentationMode,
        ERR_BadLanguageVersion,
        /// <summary>意外的字符</summary>
        ERR_UnexpectedCharacter,
        /// <summary>语法错误。</summary>
        ERR_SyntaxError,
        /// <summary>无效的数字。</summary>
        ERR_InvalidNumber,
        /// <summary>数字溢出。</summary>
        ERR_NumberOverflow,
        /// <summary>没有结束配对的注释。</summary>
        ERR_OpenEndedComment,
        /// <summary>未终止的字符串常量。</summary>
        ERR_UnterminatedStringLiteral,
        /// <summary>不合法的转义序列。</summary>
        ERR_IllegalEscape,
        WRN_ErrorOverride,

        #region Lua实验性版本的消息
        ERR_FeatureIsExperimental = 8501,
        ERR_FeatureInPreview
        #endregion

        // 更新编译器的警告后应手动运行（eng\generate-compiler-code.cmd）。
    }
}
