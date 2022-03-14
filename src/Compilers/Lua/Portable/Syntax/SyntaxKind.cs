namespace SamLu.CodeAnalysis.Lua;

public enum SyntaxKind : ushort
{
    None = 0,
    List = Microsoft.CodeAnalysis.GreenNode.ListKind,

    #region 标点
    /// <summary>表示<c>+</c>标记。</summary>
    PlusToken = 8193,
    /// <summary>表示<c>-</c>标记。</summary>
    MinusToken,
    /// <summary>表示<c>*</c>标记。</summary>
    AsteriskToken,
    /// <summary>表示<c>/</c>标记。</summary>
    SlashToken,
    /// <summary>表示<c>%</c>标记。</summary>
    PersentToken,
    /// <summary>表示<c>#</c>标记。</summary>
    HashToken,
    /// <summary>表示<c>&amp;</c>标记。</summary>
    AmpersandToken,
    /// <summary>表示<c>~</c>标记。</summary>
    TildeToken,
    /// <summary>表示<c>|</c>标记。</summary>
    BarToken,
    /// <summary>表示<c>&lt;</c>标记。</summary>
    LessThanToken,
    /// <summary>表示<c>&gt;</c>标记。</summary>
    GreaterThenToken,
    /// <summary>表示<c>=</c>标记。</summary>
    EqualsToken,
    /// <summary>表示<c>(</c>标记。</summary>
    OpenParenToken,
    /// <summary>表示<c>)</c>标记。</summary>
    CloseParenToken,
    /// <summary>表示<c>{</c>标记。</summary>
    OpenBraceToken,
    /// <summary>表示<c>}</c>标记。</summary>
    CloseBraceToken,
    /// <summary>表示<c>[</c>标记。</summary>
    OpenBracketToken,
    /// <summary>表示<c>]</c>标记。</summary>
    CloseBracketToken,
    /// <summary>表示<c>:</c>标记。</summary>
    ColonToken,
    /// <summary>表示<c>;</c>标记。</summary>
    SemicolonToken,
    /// <summary>表示<c>,</c>标记。</summary>
    CommanToken,
    /// <summary>表示<c>.</c>标记。</summary>
    DotToken,

    /// <summary>表示<c>&lt;&lt;</c>标记。</summary>
    LessThanLessThenToken = 8257,
    /// <summary>表示<c>&gt;&gt;</c>标记。</summary>
    GreaterThanLessThenToken,
    /// <summary>表示<c>//</c>标记。</summary>
    SlashSlashToken,
    /// <summary>表示<c>==</c>标记。</summary>
    EqualsEqualsToken,
    /// <summary>表示<c>~=</c>标记。</summary>
    TildeEqualsToken,
    /// <summary>表示<c>&lt;=</c>标记。</summary>
    LessThenEqualsToken,
    /// <summary>表示<c>&gt;=</c>标记。</summary>
    GreaterThenEqualsToken,
    /// <summary>表示<c>::</c>标记。</summary>
    ColonColonToken,
    /// <summary>表示<c>..</c>标记。</summary>
    DotDotToken,
    /// <summary>表示<c>...</c>标记。</summary>
    DotDotDotToken,
    #endregion

    #region 关键词
    /// <summary>表示<c>and</c>关键词。</summary>
    AndKeyword = 8321,
    /// <summary>表示<c>break</c>关键词。</summary>
    BreakKeyword,
    /// <summary>表示<c>do</c>关键词。</summary>
    DoKeyword,
    /// <summary>表示<c>else</c>关键词。</summary>
    ElseKeyword,
    /// <summary>表示<c>elseif</c>关键词。</summary>
    ElseIfKeyword,
    /// <summary>表示<c>end</c>关键词。</summary>
    EndKeyword,
    /// <summary>表示<c>false</c>关键词。</summary>
    FalseKeyword,
    /// <summary>表示<c>for</c>关键词。</summary>
    ForKeyword,
    /// <summary>表示<c>function</c>关键词。</summary>
    FunctionKeyword,
    /// <summary>表示<c>goto</c>关键词。</summary>
    GotoKeyword,
    /// <summary>表示<c>if</c>关键词。</summary>
    IfKeyword,
    /// <summary>表示<c>in</c>关键词。</summary>
    InKeyword,
    /// <summary>表示<c>local</c>关键词。</summary>
    LocalKeyword,
    /// <summary>表示<c>nil</c>关键词。</summary>
    NilKeyword,
    /// <summary>表示<c>not</c>关键词。</summary>
    NotKeyword,
    /// <summary>表示<c>or</c>关键词。</summary>
    OrKeyword,
    /// <summary>表示<c>repeat</c>关键词。</summary>
    RepeatKeyword,
    /// <summary>表示<c>return</c>关键词。</summary>
    ReturnKeyword,
    /// <summary>表示<c>then</c>关键词。</summary>
    ThenKeyword,
    /// <summary>表示<c>true</c>关键词。</summary>
    TrueKeyword,
    /// <summary>表示<c>until</c>关键词。</summary>
    UntilKeyword,
    /// <summary>表示<c>while</c>关键词。</summary>
    WhileKeyword,

    // 上下文关键词
    /// <summary>表示<c>_G</c>关键词。</summary>
    GlobalEnvironmentKeyword = 8385,
    /// <summary>表示<c>_ENV</c>关键词。编译器在编译期间将这个关键词作为所有游离的变量的作用环境，它的值不是固定的。（自Lua 5.2版本添加。）</summary>
    EnvironmentKeyword,

    // 元字段和元方法
    /// <summary>表示元表（<c>()</c>）元字段<c>__metatable</c>。</summary>
    MetatableMetafield = 8449,
    /// <summary>表示加法（<c>+</c>）元方法<c>__add</c>。</summary>
    AdditionMetamethod,
    /// <summary>表示减法（二元<c>-</c>）元方法<c>__sub</c>。</summary>
    SubtractionMetamethod,
    /// <summary>表示乘法（<c>*</c>）元方法<c>__mul</c>。</summary>
    MultiplicationMetamethod,
    /// <summary>表示除法（<c>/</c>）元方法<c>__div</c>。</summary>
    DivisionMetamethod,
    /// <summary>表示取模（<c>%</c>）元方法<c>__mod</c>。</summary>
    ModuloMetamethod,
    /// <summary>表示取幂（<c>+</c>）元方法<c>__pow</c>。</summary>
    ExponentiationMetamethod,
    /// <summary>表示取负（一元<c>-</c>）元方法<c>__unm</c>。</summary>
    NegationMetamethod,
    /// <summary>表示向下取整除法（<c>+</c>）元方法<c>__idiv</c>。</summary>
    FloorDivision,
    /// <summary>表示按位与（<c>&amp;</c>）元方法<c>__band</c>。</summary>
    BitwiseAndMetamethod,
    /// <summary>表示按位或（<c>|</c>）元方法<c>__bor</c>。</summary>
    BitwiseOrMetamethod,
    /// <summary>表示按位异或（二元<c>~</c>）元方法<c>__bxor</c>。</summary>
    BitwiseXorMetamethod,
    /// <summary>表示按位取反（一元<c>~</c>）元方法<c>__bnot</c>。</summary>
    BitwiseNotMetamethod,
    /// <summary>表示按位向左位移（<c>&lt;&lt;</c>）元方法<c>__shl</c>。</summary>
    BitwiseLeftShiftMetamethod,
    /// <summary>表示按位向右位移（<c>&gt;&gt;</c>）元方法<c>__shr</c>。</summary>
    BitwiseRightShiftMetamethod,
    /// <summary>表示连接（<c>..</c>）元方法<c>__concat</c>。</summary>
    ConcatenationMetamethod,
    /// <summary>表示长度（<c>#</c>）元方法<c>__len</c>。</summary>
    LengthMetamethod,
    /// <summary>表示相等（<c>==</c>）元方法<c>__eq</c>。</summary>
    EqualMetamethod,
    /// <summary>表示小于（<c>&lt;</c>）元方法<c>__lt</c>。</summary>
    LessThanMetamethod,
    /// <summary>表示小于等于（<c>&lt;=</c>）元方法<c>__le</c>。</summary>
    LessEqualMetamethod,
    /// <summary>表示操作索引（<c>[]</c>）元方法<c>__index</c>。</summary>
    IndexingAccessMetamethod,
    /// <summary>表示调用（<c>()</c>）元方法<c>__call</c>。</summary>
    CallMetamethod,
    /// <summary>表示字典访问（<c>()</c>）元方法<c>__pairs</c>。</summary>
    PairsMetamethod,
    /// <summary>表示转换为字符串（<c>()</c>）元方法<c>__tostring</c>。</summary>
    ToStringMetamethod,
    /// <summary>表示垃圾收集（<c>+</c>）元方法<c>__gc</c>。</summary>
    GarbageCollectionMetamethod,
    /// <summary>表示标记要被关闭（<c>+</c>）元方法<c>__close</c>。（自Lua 5.4版本添加。）</summary>
    ToBeClosedMetamethod,
    /// <summary>表示弱表模式（<c>+</c>）元字段<c>__mode</c>。</summary>
    WeekModeMetafield,
    /// <summary>表示名称（<c>+</c>）元字段<c>__name</c>。</summary>
    NameMetafield,
    #endregion

    #region 文本记号
    BadToken = 9217,
    IdentifierToken,
    NumericLiteralToken,
    StringLiteralToken,

    SingleLineRawStringLiteralToken,
    MultiLineRawStringLiteralToken,
    #endregion

    #region 琐碎记号
    EndOfLingTrivia = 9249,
    WhitespaceTrivia,
    SingleLineCommentTrivia,
    MultiLineCommentTrivia,
    #endregion

    // declarations
    CompilationUnit = 10240,
}
