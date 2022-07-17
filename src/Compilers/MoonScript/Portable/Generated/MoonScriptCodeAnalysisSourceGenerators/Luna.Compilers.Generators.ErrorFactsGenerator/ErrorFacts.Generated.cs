namespace SamLu.CodeAnalysis.MoonScript
{
    internal static partial class ErrorFacts
    {
        public static bool IsWarning(ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.WRN_ErrorOverride:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFatal(ErrorCode code)
        {
            switch (code)
            {
                default:
                    return false;
            }
        }

        public static bool IsInfo(ErrorCode code)
        {
            switch (code)
            {
                default:
                    return false;
            }
        }

        public static bool IsHidden(ErrorCode code)
        {
            switch (code)
            {
                default:
                    return false;
            }
        }
    }
}
