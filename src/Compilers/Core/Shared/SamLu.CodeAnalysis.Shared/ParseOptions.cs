using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.
#if LANG_LUA
    Lua
#elif LANG_MOONSCRIPT
    MoonScript
#endif
    ;

/// <summary>
/// 此类型储存数个与解析有关的选项，并且提供修改这些选项的值的方法。
/// </summary>
public sealed partial class
#if LANG_LUA
    LuaParseOptions : ParseOptions, IEquatable<LuaParseOptions>
#elif LANG_MOONSCRIPT
    MoonScriptParseOptions : ParseOptions, IEquatable<MoonScriptParseOptions>
#endif
{
    /// <summary>
    /// 默认解析选项。
    /// </summary>
    public static
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
        Default { get; } = new();

    private ImmutableDictionary<string, string> _features;

    public override IReadOnlyDictionary<string, string> Features => this._features;

    /// <summary>
    /// 获取源代码的语言名称。
    /// </summary>
    public override string Language =>
#if LANG_LUA
        LanguageNames.Lua
#elif LANG_MOONSCRIPT
        LanguageNames.MoonScript
#endif
        ;

    /// <summary>
    /// 获取有效的语言版本，编译器将依据版本选择应用程序的语言规范。
    /// </summary>
    public LanguageVersion LanguageVersion { get; init; }

    /// <summary>
    /// 获取特定的语言版本，此属性的值在创建<see cref="LuaParseOptions"/>的新实例时传入构造函数，或使用<see cref="WithLanguageVersion"/>方法设置。
    /// </summary>
    public LanguageVersion SpecifiedLanguageVersion { get; init; }

    [Obsolete("Lua.NET不支持预处理指令", false)]
    internal ImmutableArray<string> PreprocessorSymbols { get; init; }

    /// <summary>
    /// 获取已定义的解析器符号的名称。
    /// </summary>
    [Obsolete("Lua.NET不支持预处理指令", false)]
    public override IEnumerable<string> PreprocessorSymbolNames => this.PreprocessorSymbols;

    [Obsolete("Lua.NET不支持预处理指令", false)]
    public
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
        (
        LanguageVersion languageVersion = LanguageVersion.Default,
        DocumentationMode documentationMode = DocumentationMode.Parse,
        SourceCodeKind kind = SourceCodeKind.Regular,
        IEnumerable<string>? preprocessorSymbols = null) :
            this(languageVersion, documentationMode, kind, preprocessorSymbols.ToImmutableArrayOrEmpty(), ImmutableDictionary<string, string>.Empty)
    { }

    [Obsolete("Lua.NET不支持预处理指令", false)]
    internal
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
        (
        LanguageVersion languageVersion,
        DocumentationMode documentationMode,
        SourceCodeKind kind,
        ImmutableArray<string> preprocessorSymbols,
        IReadOnlyDictionary<string, string>? features) : base(kind, documentationMode)
    {
        this.SpecifiedLanguageVersion = languageVersion;
        this.LanguageVersion = languageVersion.MapSpecifiedToEffectiveVersion();
        this.PreprocessorSymbols = preprocessorSymbols.ToImmutableArrayOrEmpty();
        this._features = features?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
    }

    private
#if LANG_LUA
        LuaParseOptions(LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions(MoonScriptParseOptions
#endif
        other) :
        this(
        languageVersion: other.SpecifiedLanguageVersion,
        documentationMode: other.DocumentationMode,
        kind: other.Kind,
        preprocessorSymbols: other.PreprocessorSymbols,
        features: other.Features)
    { }

    public new
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithKind(SourceCodeKind kind)
    {
        if (kind == this.SpecifiedKind) return this;

        var effectiveKind = kind.MapSpecifiedToEffectiveKind();
        return new(this)
        {
            SpecifiedKind = kind,
            Kind = effectiveKind
        };
    }

    public
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithLanguageVersion(LanguageVersion version)
    {
        if (version == this.SpecifiedLanguageVersion) return this;

        var effectiveLanguageVersion = version.MapSpecifiedToEffectiveVersion();
        return new(this)
        {
            SpecifiedLanguageVersion = version,
            LanguageVersion = effectiveLanguageVersion
        };
    }

    [Obsolete("Lua.NET不支持预处理指令", false)]
    public
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithPreprocessorSymbols(IEnumerable<string>? preprocessorSymbols) => this.WithPreprocessorSymbols(preprocessorSymbols.AsImmutableOrNull());

    [Obsolete("Lua.NET不支持预处理指令", false)]
    public
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithPreprocessorSymbols(params string[]? preprocessorSymbols) => this.WithPreprocessorSymbols(preprocessorSymbols.AsImmutableOrNull());

    [Obsolete("Lua.NET不支持预处理指令", false)]
    public
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithPreprocessorSymbols(ImmutableArray<string> symbols)
    {
        if (symbols.IsDefault)
            symbols = ImmutableArray<string>.Empty;

        if (symbols.Equals(this.PreprocessorSymbols)) return this;

        return new(this)
        {
            PreprocessorSymbols = symbols
        };
    }

    public new
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithDocumentationMode(DocumentationMode documentationMode)
    {
        if (documentationMode == this.DocumentationMode) return this;

        return new(this)
        {
            DocumentationMode = documentationMode
        };
    }

    public new
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
         WithFeatures(IEnumerable<KeyValuePair<string, string>>? features)
    {
        var dictionary = features?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string>.Empty;

        return new(this)
        {
            _features = dictionary
        };
    }

    internal override void ValidateOptions(ArrayBuilder<Diagnostic> builder)
    {
        this.ValidateOptions(builder, MessageProvider.Instance);

        // 验证当Latest/Default被转换后，LanguageVersion不是SpecifiedLanguageVersion。
        if (!this.LanguageVersion.IsValid())
            builder.Add(Diagnostic.Create(MessageProvider.Instance, (int)ErrorCode.ERR_BadLanguageVersion, LanguageVersion.ToString()));
        
        if (!this.PreprocessorSymbols.IsDefaultOrEmpty)
        {
            foreach (var symbol in this.PreprocessorSymbols)
            {
                if (symbol is null)
                    builder.Add(Diagnostic.Create(MessageProvider.Instance, (int)ErrorCode.ERR_InvalidPreprocessingSymbol, "null"));
                else if (!SyntaxFacts.IsValidIdentifier(symbol))
                    builder.Add(Diagnostic.Create(MessageProvider.Instance, (int)ErrorCode.ERR_InvalidPreprocessingSymbol, symbol));
            }
        }
    }

    internal bool IsFeatureEnabled(MessageID feature)
    {
        string? featureFlag = feature.RequiredFeature();
        if (featureFlag is not null) return this.Features.ContainsKey(featureFlag);

        var avaliableVersion = this.LanguageVersion;
        var requiredVersion = feature.RequiredVersion();
        return avaliableVersion >= requiredVersion;
    }

#region ParseOptions
    public sealed override ParseOptions CommonWithKind(SourceCodeKind kind) => this.WithKind(kind);

    protected sealed override ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode) => this.WithDocumentationMode(documentationMode);

    protected sealed override ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features) => this.WithFeatures(features);
#endregion

    public sealed override bool Equals(object? obj) => this.Equals(obj as
#if LANG_LUA
        LuaParseOptions
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions
#endif
        );
    public bool Equals(
#if LANG_LUA
        LuaParseOptions?
#elif LANG_MOONSCRIPT
        MoonScriptParseOptions?
#endif
         other)
    {
        if (object.ReferenceEquals(this, other)) return true;
        else if (!base.EqualsHelper(other)) return false;
        else return this.SpecifiedLanguageVersion == other.SpecifiedLanguageVersion;
    }

    public override int GetHashCode() =>
        Hash.Combine(base.GetHashCodeHelper(),
            Hash.Combine((int)this.SpecifiedLanguageVersion, 0));
}
