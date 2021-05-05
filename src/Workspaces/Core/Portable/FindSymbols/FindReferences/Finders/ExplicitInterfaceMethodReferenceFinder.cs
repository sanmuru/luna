﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FindSymbols.Finders
{
    internal class ExplicitInterfaceMethodReferenceFinder : AbstractReferenceFinder<IMethodSymbol>
    {
        protected override bool CanFind(IMethodSymbol symbol)
            => symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation;

        protected override Task<ImmutableArray<(ISymbol symbol, FindReferencesCascadeDirection cascadeDirection)>> DetermineCascadedSymbolsAsync(
            IMethodSymbol symbol,
            Project project,
            FindReferencesSearchOptions options,
            FindReferencesCascadeDirection cascadeDirection,
            CancellationToken cancellationToken)
        {
            if (!cascadeDirection.HasFlag(FindReferencesCascadeDirection.Up))
                return SpecializedTasks.EmptyImmutableArray<(ISymbol symbol, FindReferencesCascadeDirection cascadeDirection)>();

            // An explicit interface method will cascade to all the methods that it implements in the up direction.
            return Task.FromResult(
                symbol.ExplicitInterfaceImplementations.SelectAsArray(m => ((ISymbol)m, FindReferencesCascadeDirection.Up)));
        }

        protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(
            IMethodSymbol symbol,
            Project project,
            IImmutableSet<Document>? documents,
            FindReferencesSearchOptions options,
            CancellationToken cancellationToken)
        {
            // An explicit method can't be referenced anywhere.
            return SpecializedTasks.EmptyImmutableArray<Document>();
        }

        protected override ValueTask<ImmutableArray<FinderLocation>> FindReferencesInDocumentAsync(
            IMethodSymbol symbol,
            Func<ISymbol, ValueTask<bool>> isMatchAsync,
            Document document,
            SemanticModel semanticModel,
            FindReferencesSearchOptions options,
            CancellationToken cancellationToken)
        {
            // An explicit method can't be referenced anywhere.
            return new ValueTask<ImmutableArray<FinderLocation>>(ImmutableArray<FinderLocation>.Empty);
        }
    }
}
