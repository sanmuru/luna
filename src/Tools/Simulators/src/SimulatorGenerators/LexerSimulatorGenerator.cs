using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Luna.Compilers.Generators;

[Generator]
public sealed class LexerSimulatorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        // https://stackoverflow.com/questions/64926889/generate-code-for-classes-with-an-attribute
        var treeWithAttribute = context.Compilation.SyntaxTrees
            .Where(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Any(classDeclaration => classDeclaration.DescendantNodes().OfType<AttributeSyntax>().Any()));
        var declaredClass = tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(cd => cd.DescendantNodes().OfType<AttributeSyntax>().Any());
        var semanticModel = context.Compilation.GetSemanticModel(tree);

    }
}
