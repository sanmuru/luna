using System.Reflection;
using System.Text;
using HtmlAgilityPack;
using Luna.Compilers.Simulators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Luna.Compilers.Tools;

internal class Program
{
    public static int Main(string[] args)
    {
        if (args.Length is < 1 or > 3)
        {
            return WriteUsage();
        }

        string inputFile = args[0];
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"未找到“{inputFile}”");
            return 1;
        }

        string? outputFile = null;
        string? cssPath = null;
        string[]? cssFiles = null;
        if (args.Length == 2)
        {
            if (args[1].StartsWith("/css:"))
            {
                cssPath = args[1].Substring(5);
            }
            else
            {
                outputFile = args[1];
            }
        }
        else
        {
            outputFile = args[1];
            cssPath = args[2].Substring(5);
        }

        outputFile = inputFile + ".html";
        if (cssPath is not null)
        {
            if (Directory.Exists(cssPath))
                cssFiles = Directory.GetFiles(cssPath, "*.css");
            else if (File.Exists(cssPath))
                cssFiles = new[] { cssPath };
        }

        LexerSimulator.RegisterSimulatorFromConfiguration(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "config.json"));

        return Program.WriteHtml(inputFile, outputFile, cssFiles);
    }

    private static int WriteHtml(
        string inputFile,
        string outputFile,
        string[]? cssFiles)
    {
        var extension = Path.GetExtension(inputFile);
        if (LexerSimulator.TryGetLexerSimulator(extension, out var simulators))
        {
            for (var i = 0; i < simulators.Length; i++)
            {
                var simulator = simulators[i];

                var doc = new HtmlDocument();

                var head = doc.CreateElement("head");
                {
                    var meta = doc.CreateElement("meta");
                    meta.SetAttributeValue("charset", "utf-8");
                    head.AppendChild(meta);
                    foreach (var cssFile in cssFiles)
                    {
                        var link = doc.CreateElement("link");
                        link.SetAttributeValue("rel", "stylesheet");
                        link.SetAttributeValue("type", "text/css");
                        link.SetAttributeValue("href", cssFile);
                    }
                    var title = doc.CreateElement("title");
                    title.AppendChild(doc.CreateTextNode($"{Path.GetFileName(inputFile)} - {simulator.GetType().FullName}"));
                }

                var body = doc.CreateElement("body");
                {
                    var div = doc.CreateElement("div");
                    body.AppendChild(div);

                    using var fs = File.OpenRead(inputFile);
                    var text = SourceText.From(fs);
                    foreach (var token in simulator.LexToEnd(text))
                    {
                        if (token.HasLeadingTrivia) processTriviaList(token.LeadingTrivia);
                        processToken(simulator.GetTokenKind(token.RawKind), token.Text);
                        if (token.HasTrailingTrivia) processTriviaList(token.TrailingTrivia);
                    }

                    void processTriviaList(SyntaxTriviaList triviaList)
                    {
                        foreach (var trivia in triviaList)
                        {
                            processToken(simulator.GetTokenKind(trivia.RawKind), text.GetSubText(trivia.Span).ToString());
                        }
                    }
                    void processToken(TokenKind kind, string text)
                    {
                        var span = doc.CreateElement("span");
                        div.AppendChild(span);

                        span.AddClass(kind switch
                        {
                            TokenKind.None => "none",
                            TokenKind.Keyword => "kwd",
                            TokenKind.Operator => "op",
                            TokenKind.Punctuation => "punct",
                            TokenKind.NumericLiteral => "num",
                            TokenKind.StringLiteral => "str",
                            TokenKind.WhiteSpace => "space",
                            TokenKind.Comment => "comment",
                            TokenKind.Documentation => "doc",
                            _ => throw new InvalidOperationException(),
                        });
                        span.AppendChild(doc.CreateTextNode(text));
                    };
                }

                doc.Save(
                    simulators.Length == 1 ? outputFile :
                        $"{Path.GetDirectoryName(outputFile)!}{Path.GetFileNameWithoutExtension(outputFile)}.{i}{Path.GetExtension(outputFile)}",
                    Encoding.UTF8);
            }

            return 0;
        }
        else
        {
            Console.WriteLine($"不支持的文件后缀名“{extension}”");
            return 1;
        }
    }

    private static int WriteUsage()
    {
        Console.WriteLine("Invalid usage:");
        var programName = "  " + typeof(Program).GetTypeInfo().Assembly.ManifestModule.Name;
        Console.WriteLine(programName + " input output [/css:css-path]");
        Console.WriteLine(programName + " input [/css:css-path]");
        return 1;
    }
}
