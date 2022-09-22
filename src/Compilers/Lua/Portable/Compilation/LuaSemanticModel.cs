﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SamLu.CodeAnalysis.Lua.Symbols;
using SamLu.CodeAnalysis.Lua.Syntax;

namespace SamLu.CodeAnalysis.Lua;

partial class LuaSemanticModel
{
    /// <inheritdoc cref="LuaCompilation.Language"/>
    public sealed override string Language => LanguageNames.Lua;

    #region 获取定义符号
    public abstract ISymbol GetDeclaredSymbol(ChunkSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract IMethodSymbol GetDeclaredSymbol(BlockSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default);

    protected ParameterSymbol? GetParameterSymbol(
        ImmutableArray<ParameterSymbol> symbols,
        ParameterSyntax declarationSyntax,
        CancellationToken cancellationToken = default)
    {
        foreach (var symbol in symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var location in symbol.Locations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (object.ReferenceEquals(location.SourceTree, this.SyntaxTree) &&
                    declarationSyntax.Span.Contains(location.SourceSpan))
                    return symbol;
            }
        }

        return null;
    }

    public abstract ILabelSymbol GetDeclaredSymbol(LabelStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract ImmutableArray<ILocalSymbol> GetDeclaredSymbols(LocalDeclarationStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract ILocalSymbol GetDeclaredSymbol(LocalFunctionDefinitionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract ImmutableArray<ILocalSymbol> GetDeclaredSymbols(AssignmentStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract ILocalSymbol? GetDeclaredSymbol(ForStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);

    public abstract ImmutableArray<ILocalSymbol> GetDeclaredSymbol(ForInStatementSyntax declarationSyntax, CancellationToken cancellationToken = default);
    #endregion

    public abstract ForInStatementInfo GetForInStatementInfo(ForInStatementSyntax node);
}
