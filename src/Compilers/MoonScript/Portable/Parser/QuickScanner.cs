namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    private enum QuickScanState : byte
    {
        /// <summary>初始状态。</summary>
        Initial,
        /// <summary>跟随空白字符之后状态。</summary>
        FollowingWhite,
        /// <summary>跟随回车符之后状态。</summary>
        FollowingCR,
        Ident,
        /// <summary>数字状态。</summary>
        Number,
        /// <summary>标点状态。</summary>
        Punctuation,
        /// <summary>点状态。</summary>
        Dot,
        /// <summary>复合标点起始状态。</summary>
        CompoundPunctStart,
        /// <summary>下一个后立即是<see cref="Done"/>状态。</summary>
        DoneAfterNext,
        /// <summary>完成状态。</summary>
        Done,
        /// <summary>错误状态。</summary>
        Bad = Done + 1
    }

    private enum CharFlags : byte
    {
        /// <summary>空白字符（空格符或制表符）。</summary>
        White,
        /// <summary>回车符。</summary>
        CR,
        /// <summary>换行符。</summary>
        LF,
        /// <summary>字母。</summary>
        Letter,
        /// <summary>数字。</summary>
        Digit,
        /// <summary>简单的标点。</summary>
        Punct,
        /// <summary>数字小数点。</summary>
        Dot,
        /// <summary>复杂的标点的起始。</summary>
        CompoundPunctStart,
        /// <summary>斜杠。</summary>
        Slash,
        /// <summary>复杂内容，将导致扫描终止。</summary>
        Complex,
        EndOfFile,
    }

    private static readonly byte[,] s_stateTransitions = new byte[,]
    {
        // Initial
        {
            (byte)QuickScanState.Initial,             // White
            (byte)QuickScanState.Initial,             // CR
            (byte)QuickScanState.Initial,             // LF
            (byte)QuickScanState.Ident,               // Letter
            (byte)QuickScanState.Number,              // Digit
            (byte)QuickScanState.Punctuation,         // Punct
            (byte)QuickScanState.Dot,                 // Dot
            (byte)QuickScanState.CompoundPunctStart,  // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Bad,                 // EndOfFile
        },

        // Following White
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Done,                // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Following CR
        {
            (byte)QuickScanState.Done,                // White
            (byte)QuickScanState.Done,                // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Done,                // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Done,                // Slash
            (byte)QuickScanState.Done,                // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Identifier
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Ident,               // Letter
            (byte)QuickScanState.Ident,               // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Number
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Bad,                 // Letter（指数后缀过于复杂）
            (byte)QuickScanState.Number,              // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Bad,                 // Dot（带小数点的数字常量过于复杂）
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Punctuation
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Done,                // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Dot
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Number,              // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Bad,                 // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Compound Punctuation
        {
            (byte)QuickScanState.FollowingWhite,      // White
            (byte)QuickScanState.FollowingCR,         // CR
            (byte)QuickScanState.DoneAfterNext,       // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Done,                // Digit
            (byte)QuickScanState.Bad,                 // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Bad,                 // Compound
            (byte)QuickScanState.Bad,                 // Slash
            (byte)QuickScanState.Bad,                 // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },

        // Done after next
        {
            (byte)QuickScanState.Done,                // White
            (byte)QuickScanState.Done,                // CR
            (byte)QuickScanState.Done,                // LF
            (byte)QuickScanState.Done,                // Letter
            (byte)QuickScanState.Done,                // Digit
            (byte)QuickScanState.Done,                // Punct
            (byte)QuickScanState.Done,                // Dot
            (byte)QuickScanState.Done,                // Compound
            (byte)QuickScanState.Done,                // Slash
            (byte)QuickScanState.Done,                // Complex
            (byte)QuickScanState.Done,                // EndOfFile
        },
    };

    /// <summary>
    /// 获取Unicode字符的前0x180个字符的属性。
    /// </summary>
    private static ReadOnlySpan<byte> CharProperties => new[]
    {
        // 0 .. 31
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex,
        (byte)CharFlags.White,   // TAB
        (byte)CharFlags.LF,      // LF
        (byte)CharFlags.White,   // VT
        (byte)CharFlags.White,   // FF
        (byte)CharFlags.CR,      // CR
        (byte)CharFlags.Complex,
        (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,

        // 32 .. 63
        (byte)CharFlags.White,    // SPC
        (byte)CharFlags.CompoundPunctStart,    // !
        (byte)CharFlags.Complex,  // "
        (byte)CharFlags.Punct,  // #
        (byte)CharFlags.Complex,  // $
        (byte)CharFlags.CompoundPunctStart, // %
        (byte)CharFlags.CompoundPunctStart, // &
        (byte)CharFlags.Complex,  // '
        (byte)CharFlags.Punct,    // (
        (byte)CharFlags.Punct,    // )
        (byte)CharFlags.CompoundPunctStart, // *
        (byte)CharFlags.CompoundPunctStart, // +
        (byte)CharFlags.Punct,    // ,
        (byte)CharFlags.CompoundPunctStart, // -
        (byte)CharFlags.Dot,      // .
        (byte)CharFlags.Slash,    // /
        (byte)CharFlags.Digit,    // 0
        (byte)CharFlags.Digit,    // 1
        (byte)CharFlags.Digit,    // 2
        (byte)CharFlags.Digit,    // 3
        (byte)CharFlags.Digit,    // 4
        (byte)CharFlags.Digit,    // 5
        (byte)CharFlags.Digit,    // 6
        (byte)CharFlags.Digit,    // 7
        (byte)CharFlags.Digit,    // 8
        (byte)CharFlags.Digit,    // 9
        (byte)CharFlags.CompoundPunctStart,  // :
        (byte)CharFlags.Punct,    // ;
        (byte)CharFlags.CompoundPunctStart,  // <
        (byte)CharFlags.CompoundPunctStart,  // =
        (byte)CharFlags.CompoundPunctStart,  // >
        (byte)CharFlags.CompoundPunctStart,  // ?

        // 64 .. 95
        (byte)CharFlags.Complex,  // @
        (byte)CharFlags.Letter,   // A
        (byte)CharFlags.Letter,   // B
        (byte)CharFlags.Letter,   // C
        (byte)CharFlags.Letter,   // D
        (byte)CharFlags.Letter,   // E
        (byte)CharFlags.Letter,   // F
        (byte)CharFlags.Letter,   // G
        (byte)CharFlags.Letter,   // H
        (byte)CharFlags.Letter,   // I
        (byte)CharFlags.Letter,   // J
        (byte)CharFlags.Letter,   // K
        (byte)CharFlags.Letter,   // L
        (byte)CharFlags.Letter,   // M
        (byte)CharFlags.Letter,   // N
        (byte)CharFlags.Letter,   // O
        (byte)CharFlags.Letter,   // P
        (byte)CharFlags.Letter,   // Q
        (byte)CharFlags.Letter,   // R
        (byte)CharFlags.Letter,   // S
        (byte)CharFlags.Letter,   // T
        (byte)CharFlags.Letter,   // U
        (byte)CharFlags.Letter,   // V
        (byte)CharFlags.Letter,   // W
        (byte)CharFlags.Letter,   // X
        (byte)CharFlags.Letter,   // Y
        (byte)CharFlags.Letter,   // Z
        (byte)CharFlags.Punct,    // [
        (byte)CharFlags.Complex,  // \
        (byte)CharFlags.Punct,    // ]
        (byte)CharFlags.CompoundPunctStart,    // ^
        (byte)CharFlags.Letter,   // _

        // 96 .. 127
        (byte)CharFlags.Complex,  // `
        (byte)CharFlags.Letter,   // a
        (byte)CharFlags.Letter,   // b
        (byte)CharFlags.Letter,   // c
        (byte)CharFlags.Letter,   // d
        (byte)CharFlags.Letter,   // e
        (byte)CharFlags.Letter,   // f
        (byte)CharFlags.Letter,   // g
        (byte)CharFlags.Letter,   // h
        (byte)CharFlags.Letter,   // i
        (byte)CharFlags.Letter,   // j
        (byte)CharFlags.Letter,   // k
        (byte)CharFlags.Letter,   // l
        (byte)CharFlags.Letter,   // m
        (byte)CharFlags.Letter,   // n
        (byte)CharFlags.Letter,   // o
        (byte)CharFlags.Letter,   // p
        (byte)CharFlags.Letter,   // q
        (byte)CharFlags.Letter,   // r
        (byte)CharFlags.Letter,   // s
        (byte)CharFlags.Letter,   // t
        (byte)CharFlags.Letter,   // u
        (byte)CharFlags.Letter,   // v
        (byte)CharFlags.Letter,   // w
        (byte)CharFlags.Letter,   // x
        (byte)CharFlags.Letter,   // y
        (byte)CharFlags.Letter,   // z
        (byte)CharFlags.Punct,    // {
        (byte)CharFlags.CompoundPunctStart,  // |
        (byte)CharFlags.Punct,    // }
        (byte)CharFlags.CompoundPunctStart,    // ~
        (byte)CharFlags.Complex,

        // 128 .. 159
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,

        // 160 .. 191
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Letter, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Letter, (byte)CharFlags.Complex, (byte)CharFlags.Complex,
        (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Letter, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex, (byte)CharFlags.Complex,

        // 192 .. 
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Complex,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,

        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Complex,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,

        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,

        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,

        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,

        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter,
        (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter, (byte)CharFlags.Letter
    };
}
