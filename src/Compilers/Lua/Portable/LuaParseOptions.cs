using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace SamLu.CodeAnalysis.Lua
{
    /// <summary>
    /// 此类型储存数个与解析有关的选项，并且提供修改这些选项的值的方法。
    /// </summary>
    public sealed class LuaParseOptions : ParseOptions, IEquatable<LuaParseOptions>
    {
        /// <summary>
        /// 默认解析选项。
        /// </summary>
        public static LuaParseOptions Default { get; } = new();

        private ImmutableDictionary<string, string> _features;

        public override IReadOnlyDictionary<string, string> Features => this._features;

        public override string Language => LanguageNames.Lua;

        /// <summary>
        /// 获取有效的语言版本，编译器将依据版本选择应用程序的语言规范。
        /// </summary>
        public LanguageVersion LanguageVersion { get; init; }

        public LanguageVersion SpecifiedLanguageVersion { get; init; }

        internal ImmutableArray<string> PreprocessorSymbols { get; init; }

        public override IEnumerable<string> PreprocessorSymbolNames => this.PreprocessorSymbols;

        public LuaParseOptions(
            LanguageVersion languageVersion = LanguageVersion.Default,
            DocumentationMode documentationMode = DocumentationMode.Parse,
            SourceCodeKind kind = SourceCodeKind.Regular,
            IEnumerable<string>? preprocessorSymbols = null) :
                this(languageVersion, documentationMode, kind, preprocessorSymbols.ToImmutableArrayOrEmpty(), ImmutableDictionary<string, string>.Empty)
        { }

        internal LuaParseOptions(
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

        private LuaParseOptions(LuaParseOptions other) : this(
            languageVersion: other.SpecifiedLanguageVersion,
            documentationMode: other.DocumentationMode,
            kind: other.Kind,
            preprocessorSymbols: other.PreprocessorSymbols,
            features: other.Features)
        { }

        public sealed override bool Equals(object? obj) => this.Equals(obj as LuaParseOptions);
        public bool Equals(LuaParseOptions? other)
        {
            if (object.ReferenceEquals(this, other)) return true;
            else if (!base.EqualsHelper(other)) return false;
            else return this.SpecifiedLanguageVersion == other.SpecifiedLanguageVersion;
        }

        public override int GetHashCode() =>
            Hash.Combine(base.GetHashCodeHelper(),
                Hash.Combine((int)this.SpecifiedLanguageVersion, 0));
    }
}
