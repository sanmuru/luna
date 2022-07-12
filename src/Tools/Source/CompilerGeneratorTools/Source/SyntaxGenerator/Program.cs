using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;
using SyntaxGenerator.Lua;

namespace SyntaxGenerator;

internal abstract class Program
{
    protected abstract string LanguageName { get; }

    public static int Main(string[] args)
    {
        if (args.Length is < 3 or > 4)
            return Program.WriteUsage();

        string languageName = args[0];
        string inputFile = args[1];
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("未能找到“{0}”", inputFile);
            return 1;
        }

        bool writeSource = true;
        bool writeTests = false;
        bool writeSignatures = false;
        bool writeGrammar = false;
        string? outputFile = null;

        if (args.Length == 4)
        {
            outputFile = args[2];

            if (args[3] == "/test")
            {
                writeTests = true;
                writeSource = false;
            }
            else if (args[3] == "/grammar")
            {
                writeGrammar = true;
            }
            else
            {
                return WriteUsage();
            }
        }
        else if (args.Length == 3)
        {
            if (args[2] == "/sig")
            {
                writeSignatures = true;
            }
            else
            {
                outputFile = args[1];
            }
        }

        Program program = languageName switch
        {
            "Lua" => new LuaProgram(),
            _ => throw new NotSupportedException()
        };

        Debug.Assert(program is not null);
        if (writeGrammar)
        {
            Debug.Assert(outputFile is not null);
            return program.WriteGrammarFile(inputFile, outputFile);
        }
        else
        {
            return program.WriteSourceFiles(inputFile, writeSource, writeTests, writeSignatures, outputFile);
        }
    }

    private static int WriteUsage()
    {
        Console.WriteLine("参数错误：");
        var programName = typeof(Program).GetTypeInfo().Assembly.ManifestModule.Name;
        Console.WriteLine(programName + " language-name input-file output-file [/test | /grammar]");
        Console.WriteLine(programName + " language-name input-file /sig");
        return 1;
    }

    protected virtual Tree ReadTree(string inputFile) => Program.ReadTree<Tree>(inputFile);

    protected static TTree ReadTree<TTree>(string inputFile)
        where TTree : Tree
    {
        var reader = XmlReader.Create(inputFile, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        var serializer = new XmlSerializer(typeof(TTree));
        var tree = serializer.Deserialize(reader) as TTree;
        if (tree is null)
            throw new InvalidDataException($"未能从文件“{inputFile}”中读取到语法模型树。");
        else
            return tree;
    }

    protected int WriteGrammarFile(string inputFile, string outputLocation)
    {
        try
        {
            var grammarText = this.CreateGrammarGenerator().Run(this.ReadTree(inputFile).Types);
            var outputMainFile = Path.Combine(outputLocation.Trim('"'), $"{this.LanguageName}.Generated.g4");

            using var outFile = new StreamWriter(File.Open(outputMainFile, FileMode.Create), Encoding.UTF8);
            outFile.Write(grammarText);
        }
        catch (Exception ex)
        {
            Console.WriteLine("语法生成失败。");
            Console.WriteLine(ex);

            // 打印异常信息但不重新排除异常。
            // 目的是看到异常信息后修复程序而非中断生成。
        }

        return 0;
    }

    protected int WriteSourceFiles(string inputFile, bool writeSource, bool writeTests, bool writeSignatures, string? outputFile)
    {
        var tree = this.ReadTree(inputFile);

        // The syntax.xml doc contains some nodes that are useful for other tools, but which are
        // not needed by this syntax generator.  Specifically, we have `<Choice>` and
        // `<Sequence>` nodes in the xml file to help others tools understand the relationship
        // between some fields (i.e. 'only one of these children can be non-null').  To make our
        // life easier, we just flatten all those nodes, grabbing all the nested `<Field>` nodes
        // and placing into a single linear list that we can then process.
        TreeFlattening.FlattenChildren(tree);

        if (writeSignatures)
        {
            var signatureWriter = this.CreateSignatureWriter(Console.Out, tree);
            signatureWriter.WriteFile();
        }
        else
        {
            if (writeSource)
            {
                Debug.Assert(outputFile is not null);
                var outputPath = outputFile.Trim('"');
                var prefix = Path.GetFileName(inputFile);
                var outputMainFile = Path.Combine(outputPath, $"{prefix}.Main.Generated.cs");
                var outputInternalFile = Path.Combine(outputPath, $"{prefix}.Internal.Generated.cs");
                var outputSyntaxFile = Path.Combine(outputPath, $"{prefix}.Syntax.Generated.cs");

                WriteToFile(writer => this.CreateSourceWriter(writer, tree, default).WriteMain(), outputMainFile);
                WriteToFile(writer => this.CreateSourceWriter(writer, tree, default).WriteInternal(), outputInternalFile);
                WriteToFile(writer => this.CreateSourceWriter(writer, tree, default).WriteSyntax(), outputSyntaxFile);
}
            if (writeTests)
            {
                WriteToFile(writer => this.CreateTestWriter(writer, tree, default).WriteFile(), outputFile);
            }
        }

        return 0;
    }

    protected abstract GrammarGenerator CreateGrammarGenerator();

    protected abstract SignatureWriter CreateSignatureWriter(TextWriter writer, Tree tree);

    protected abstract SourceWriter CreateSourceWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken);

    protected abstract TestWriter CreateTestWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken);

    protected void WriteToFile(Action<TextWriter> writeAction, string outputFile)
    {
        var stringBuilder = new StringBuilder();
        var writer = new StringWriter(stringBuilder);
        writeAction(writer);

        var text = stringBuilder.ToString();
        int length;
        do
        {
            length = text.Length;
            text = text.Replace($"{{{Environment.NewLine}{Environment.NewLine}", $"{{{Environment.NewLine}");
        } while (text.Length != length);

        try
        {
            using var outFile = new StreamWriter(File.Open(outputFile, FileMode.Create), Encoding.UTF8);
            outFile.Write(text);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("无法访问“{0}”。文件是否被占用？", outputFile);
        }
    }
}
