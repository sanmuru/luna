using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.Lua
{
    internal enum ErrorCode
    {
        Void = InternalErrorCode.Void,
        Unknown = InternalErrorCode.Unknown,

        #region Lua 1.0的消息
        ERR_FeatureNotAvailableInVersion1 = 1,
        #endregion

        #region Lua 1.1的消息
        ERR_FeatureNotAvailableInVersion1_1 = 501,
        #endregion

        #region Lua 2.1的消息
        ERR_FeatureNotAvailableInVersion2_1 = 1001,
        #endregion

        #region Lua 2.2的消息
        ERR_FeatureNotAvailableInVersion2_2 = 1501,
        #endregion

        #region Lua 2.3的消息
        ERR_FeatureNotAvailableInVersion2_3 = 2001,
        #endregion

        #region Lua 2.4的消息
        ERR_FeatureNotAvailableInVersion2_4 = 2501,
        #endregion

        #region Lua 2.5的消息
        ERR_FeatureNotAvailableInVersion2_5 = 3001,
        #endregion

        #region Lua 3.0的消息
        ERR_FeatureNotAvailableInVersion3 = 3501,
        #endregion

        #region Lua 3.1的消息
        ERR_FeatureNotAvailableInVersion3_1 = 4001,
        #endregion

        #region Lua 3.2的消息
        ERR_FeatureNotAvailableInVersion3_2 = 4501,
        #endregion

        #region Lua 4.0的消息
        ERR_FeatureNotAvailableInVersion4 = 5001,
        #endregion

        #region Lua 5.0的消息
        ERR_FeatureNotAvailableInVersion5 = 5501,
        #endregion

        #region Lua 5.1的消息
        ERR_FeatureNotAvailableInVersion5_1 = 6001,
        #endregion

        #region Lua 5.2的消息
        ERR_FeatureNotAvailableInVersion5_2 = 6501,
        #endregion

        #region Lua 5.3的消息
        ERR_FeatureNotAvailableInVersion5_3 = 7001,
        #endregion

        #region Lua 5.4的消息
        ERR_FeatureNotAvailableInVersion5_4 = 7501,
        #endregion

        // 更新编译器的警告后应手动运行（eng\generate-compiler-code.cmd）。
    }
}
