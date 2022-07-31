using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Luna.Compilers.Tools;

namespace Luna.Compilers.Simulators;

internal static class LexerSimulator
{
    private static readonly Dictionary<Type, ILexerSimulator> s_lexerSimulators = new();
    private static readonly Dictionary<string, HashSet<Type>> s_fileExtensionMap = new();

    public static bool TryGetLexerSimulator(string fileExtension, [NotNullWhen(true)] out ILexerSimulator[]? lexerSimulators)
    {
        lexerSimulators = null;

        if (!s_fileExtensionMap.TryGetValue(fileExtension, out var types)) return false;

        lexerSimulators = types.Select(type =>
        {
            if (!s_lexerSimulators.TryGetValue(type, out var lexerSimulator))
            {
                try
                {
                    lexerSimulator = Activator.CreateInstance(type) as ILexerSimulator;
                    Debug.Assert(lexerSimulator is not null);
                    s_lexerSimulators.Add(type, lexerSimulator!);
                }
                catch
                {
                    // 移除实例化失败的类型。
                    foreach (var pair in s_fileExtensionMap)
                        pair.Value.Remove(type);
                    return null;
                }
            }
            return lexerSimulator;
        })
            .Where(instance => instance is not null)
            .OfType<ILexerSimulator>()
            .ToArray();
        return lexerSimulators.Length != 0;
    }

    public static void RegisterSimulator(string fileExtension, Type lexerSimulatorType)
    {
        var interfaceType = typeof(ILexerSimulator);
        if (!interfaceType.IsAssignableFrom(lexerSimulatorType)) throw new ArgumentException($"“{nameof(lexerSimulatorType)}” 必须是从 “{lexerSimulatorType.FullName}” 派生的类型。", nameof(lexerSimulatorType));

        var attributes = lexerSimulatorType.GetCustomAttributes<LexerSimulatorAttribute>();
        if (!attributes.Any()) return; // 未包含指定的特性，不注册。

        LexerSimulator.AddMapItem(s_fileExtensionMap, fileExtension, lexerSimulatorType);
    }

    public static void RegisterSimulatorFrom(Assembly assembly, Func<string, IEnumerable<string>?>? languageNameToFileExtensionsProvider = null)
    {
        if (assembly is null) throw new ArgumentNullException(nameof(assembly));
        languageNameToFileExtensionsProvider = LexerSimulator.GetFileExtensionsFromLanguageName;

        var interfaceType = typeof(ILexerSimulator);
        foreach (var type in assembly.DefinedTypes)
        {
            if (type.IsAbstract || type.IsInterface || type.IsEnum) continue;
            if (!interfaceType.IsAssignableFrom(type)) continue;

            var attributes = type.GetCustomAttributes<LexerSimulatorAttribute>();
            foreach (var attribute in attributes)
            {
                if (attribute.Languages.Length == 0) continue;

                foreach (var languageName in attribute.Languages)
                {
                    var extensions = languageNameToFileExtensionsProvider(languageName);
                    if (extensions is null) continue;

                    foreach (var extension in extensions)
                        LexerSimulator.RegisterSimulator(extension, type);
                }
            }
        }

    }

    internal static void RegisterSimulatorFromConfiguration(SimulatorConfiguration config)
    {
        Debug.Assert(config is not null);

        LexerSimulator.RegisterSimulatorFromConfigurationCore(config);
    }

    internal static void RegisterSimulatorFromConfiguration(string configFilePath)
    {
        Debug.Assert(configFilePath is not null);

        SimulatorConfiguration? config = null;
        if (File.Exists(configFilePath))
        {
            using var fs = File.OpenRead(configFilePath);
            config = SimulatorConfiguration.Deserialize(fs);
        }
        LexerSimulator.RegisterSimulatorFromConfigurationCore(config);
    }

    internal static void RegisterSimulatorFromConfigurationCore(SimulatorConfiguration? config)
    {
        string[]? searchPaths = null;
        Dictionary<string, HashSet<string>>? languageNameToFileExtensionMap = null;
        if (config is not null)
        {
            searchPaths = config.Paths;
            foreach (var pair in config.Extensions)
            {
                foreach (var languageName in pair.Value)
                {
                    languageNameToFileExtensionMap ??= new();
                    LexerSimulator.AddMapItem(languageNameToFileExtensionMap, languageName, pair.Key);
                }
            }
        }

        searchPaths ??= new[] { "Simulators/" };
        var fileExtensionsProvider = languageNameToFileExtensionMap is null ? null :
            new Func<string, IEnumerable<string>?>(languageName =>
                languageNameToFileExtensionMap.TryGetValue(languageName, out var fileExtensions) ? fileExtensions : null);

        foreach (var searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath)) continue;

            foreach (var file in Directory.GetFiles(searchPath, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(file);
                }
                catch
                {
                    continue;
                }

                LexerSimulator.RegisterSimulatorFrom(assembly, fileExtensionsProvider);
            }
        }
    }

    private static bool AddMapItem<TKey, TValue>(IDictionary<TKey, HashSet<TValue>> map, TKey key, TValue value) where TKey : notnull
    {
        if (map.TryGetValue(key, out var items))
            return items.Add(value);
        else
        {
            map.Add(key, new() { value });
            return true;
        }
    }

    private static IEnumerable<string>? GetFileExtensionsFromLanguageName(string languageName) =>
        languageName switch
        {
            "Lua" => new[] { ".lua" },
            "MoonScript" => new[] { ".moon" },

            _ => null
        };
}
