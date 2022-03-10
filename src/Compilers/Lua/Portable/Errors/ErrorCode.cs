using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua
{
    internal enum ErrorCode
    {
        Void = InternalErrorCode.Void,
        Unknown = InternalErrorCode.Unknown,

        [Obsolete("Lua.NET不支持预处理指令", false)]
        ERR_InvalidPreprocessingSymbol = 1,
        ERR_InvalidInstrumentationKind,
        ERR_BadSourceCodeKind,
        ERR_BadDocumentationMode,
        ERR_BadLanguageVersion,

        #region Lua 1.0的消息
        ERR_FeatureNotAvailableInVersion1 = 501,
        #endregion

        #region Lua 1.1的消息
        ERR_FeatureNotAvailableInVersion1_1 = 1001,
        #endregion

        #region Lua 2.1的消息
        ERR_FeatureNotAvailableInVersion2_1 = 1501,
        #endregion

        #region Lua 2.2的消息
        ERR_FeatureNotAvailableInVersion2_2 = 2001,
        #endregion

        #region Lua 2.3的消息
        ERR_FeatureNotAvailableInVersion2_3 = 2501,
        #endregion

        #region Lua 2.4的消息
        ERR_FeatureNotAvailableInVersion2_4 = 3001,
        #endregion

        #region Lua 2.5的消息
        ERR_FeatureNotAvailableInVersion2_5 = 3501,
        #endregion

        #region Lua 3.0的消息
        ERR_FeatureNotAvailableInVersion3 = 4001,
        #endregion

        #region Lua 3.1的消息
        ERR_FeatureNotAvailableInVersion3_1 = 4501,
        #endregion

        #region Lua 3.2的消息
        ERR_FeatureNotAvailableInVersion3_2 = 5001,
        #endregion

        #region Lua 4.0的消息
        ERR_FeatureNotAvailableInVersion4 = 5501,
        #endregion

        #region Lua 5.0的消息
        ERR_FeatureNotAvailableInVersion5 = 6001,
        #endregion

        #region Lua 5.1的消息
        ERR_FeatureNotAvailableInVersion5_1 = 6501,
        #endregion

        #region Lua 5.2的消息
        ERR_FeatureNotAvailableInVersion5_2 = 7001,
        #endregion

        #region Lua 5.3的消息
        ERR_FeatureNotAvailableInVersion5_3 = 7501,
        #endregion

        #region Lua 5.4的消息
        ERR_FeatureNotAvailableInVersion5_4 = 8001,
        #endregion

        // 更新编译器的警告后应手动运行（eng\generate-compiler-code.cmd）。
    }
}
