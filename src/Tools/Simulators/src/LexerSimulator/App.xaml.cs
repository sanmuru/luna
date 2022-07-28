using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using Luna.Compilers.Simulators;

namespace Luna.Compilers.Tools;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly Dictionary<Type, ILexerSimulator> s_lexerSimulators = new();
    private static readonly Dictionary<string, HashSet<Type>> s_fileExtensionMap = new();

    internal static bool TryGetLexerSimulator(string fileExtension, [NotNullWhen(true)] out ILexerSimulator[]? lexerSimulators)
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
                    s_lexerSimulators.Add(type, lexerSimulator);
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

    private void App_Startup(object sender, StartupEventArgs e)
    {
        string[]? searchPaths = null;
        Dictionary<string, HashSet<string>> languageNameToFileExtensionMap = new();
        if (File.Exists("config.json"))
        {
            using var fs = File.OpenRead("config.json");
            var doc = JsonDocument.Parse(fs);
            var config = doc.Deserialize<Configuration>(new JsonSerializerOptions()
            {
                IncludeFields = true,
                IgnoreReadOnlyProperties = true
            });
            if (config is not null)
            {
                searchPaths = config.Paths;
                foreach (var pair in config.Extensions)
                {
                    foreach (var languageName in pair.Value)
                        AddMapItem(languageNameToFileExtensionMap, languageName, pair.Key);
                }
            }
        }

        searchPaths ??= new[] { "Simulators/" };
        if (languageNameToFileExtensionMap.Count == 0)
        {
            AddMapItem(languageNameToFileExtensionMap, "Lua", ".lua");
            AddMapItem(languageNameToFileExtensionMap, "MoonScript", ".moon");
        }

        var interfaceType = typeof(ILexerSimulator);
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

                foreach (var type in assembly.ExportedTypes)
                {
                    if (!interfaceType.IsAssignableFrom(type)) continue;

                    var attributes = type.GetCustomAttributes<LexerSimulatorAttribute>();
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Languages.Length == 0) continue;

                        foreach (var languageName in attribute.Languages)
                        {
                            if (!languageNameToFileExtensionMap.ContainsKey(languageName)) continue;

                            foreach (var extension in languageNameToFileExtensionMap[languageName])
                                AddMapItem(s_fileExtensionMap, extension, type);
                        }
                    }
                }
            }
        }

        static bool AddMapItem<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> map, TKey key, TValue value) where TKey : notnull
        {
            if (map.TryGetValue(key, out var items))
                return items.Add(value);
            else
            {
                map.Add(key, new() { value });
                return true;
            }
        }
    }
}
