﻿namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

/// <summary>
/// 为便于本地化文本，此类将<see cref="MessageID"/>包装为实现<see cref="IFormattable"/>的对象。
/// </summary>
internal partial struct LocalizableErrorArgument : IFormattable
{
    private readonly MessageID _id;

    internal LocalizableErrorArgument(MessageID id) => this._id = id;

    public override string ToString() => this.ToString(format: null, formatProvider: null);

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        ErrorFacts.GetMessage(this._id, formatProvider as System.Globalization.CultureInfo);
}

/// <summary>
/// 为便于本地化<see cref="MessageID"/>的文本，在此类定义一系列的扩展方法。
/// </summary>
internal static partial class MessageIDExtensions
{
    /// <summary>
    /// 获取实现<see cref="IFormattable"/>的包装。
    /// </summary>
    /// <param name="id">要本地化的消息编号。</param>
    /// <returns>消息编号的一个实现<see cref="IFormattable"/>的包装</returns>
    public static LocalizableErrorArgument Localize(this MessageID id) => new(id);

    /// <summary>
    /// 返回通过/features开关开启相应<see cref="MessageID"/>特性的字符串表示。
    /// </summary>
    /// <remarks>
    /// <para>你应当先调用此方法，然后再调用<see cref="RequiredVersion(MessageID)"/>：</para>
    /// <para>    若此方法返回值为<see langword="null"/>时，调用<see cref="RequiredVersion(MessageID)"/>并使用其返回值。</para>
    /// <para>    若此方法返回值不为<see langword="null"/>时，使用返回值。</para>
    /// <para><see cref="RequiredFeature(MessageID)"/>和<see cref="RequiredVersion(MessageID)"/>之间应是互斥的。</para>
    /// </remarks>
    /// <param name="feature"></param>
    /// <returns><see cref="MessageID"/>特性的字符串表示。</returns>
    internal static partial string? RequiredFeature(this MessageID feature);

    internal static partial LanguageVersion RequiredVersion(this MessageID feature);
}
