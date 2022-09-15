﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Compilers.Generators.Symbols;

using Luna.Compilers.Generators.Model;
using Luna.Compilers.Generators.Syntax.Model;
using Model;

internal abstract class SymbolsFileWriter : TreeFileWriter<SymbolTree, SymbolTreeType, ITreeTypeChild>
{
    private readonly IDictionary<string, Symbol> _symbolMap;

    protected SymbolsFileWriter(TextWriter writer, SymbolTree tree, CancellationToken cancellationToken) : base(writer, tree, cancellationToken)
    {
        _symbolMap = tree.Types.OfType<Symbol>().ToDictionary(n => n.Name);
    }

    #region 帮助方法
    protected bool IsSymbol(string typeName) => this.ParentMap.ContainsKey(typeName);

    protected Symbol? GetSymbol(string? typeName)
        => typeName is not null && _symbolMap.TryGetValue(typeName, out var node) ? node : null;
    #endregion
}
