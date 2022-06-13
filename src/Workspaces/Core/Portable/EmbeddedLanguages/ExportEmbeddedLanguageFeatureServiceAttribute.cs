﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Classification;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.EmbeddedLanguages
{
    /// <summary>
    /// Use this attribute to export an <see cref="IEmbeddedLanguageFeatureService"/>.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    internal abstract class ExportEmbeddedLanguageFeatureServiceAttribute : ExportAttribute
    {
        /// <summary>
        /// Name of the classifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Name of the containing language hosting the embedded language.  e.g. C# or VB.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Identifiers in code (or StringSyntaxAttribute) used to identify an embedded language string. For example
        /// <c>Regex</c> or <c>Json</c>.
        /// </summary>
        /// <remarks>This can be used to find usages of an embedded language using a comment marker like <c>//
        /// lang=regex</c> or passed to a symbol annotated with <c>[StringSyntaxAttribyte("Regex")]</c>.  The identifier
        /// is case sensitive for the StringSyntaxAttribute, and case insensitive for the comment.
        /// </remarks>
        public string[] Identifiers { get; }

        /// <inheritdoc cref="EmbeddedLanguageMetadata.SupportsUnannotatedAPIs"/>
        internal bool SupportsUnannotatedAPIs { get; }

        public ExportEmbeddedLanguageFeatureServiceAttribute(
            Type contractType, string name, string language, params string[] identifiers)
            : this(contractType, name, language, supportsUnannotatedAPIs: false, identifiers)
        {
        }

        internal ExportEmbeddedLanguageFeatureServiceAttribute(
            Type contractType, string name, string language, bool supportsUnannotatedAPIs, params string[] identifiers)
            : base(contractType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Identifiers = identifiers ?? throw new ArgumentNullException(nameof(identifiers));
            SupportsUnannotatedAPIs = supportsUnannotatedAPIs;

            Contract.ThrowIfFalse(contractType.IsInterface && typeof(IEmbeddedLanguageFeatureService).IsAssignableFrom(contractType),
                $"{nameof(contractType)} must be an interface and derived from {typeof(IEmbeddedLanguageFeatureService).FullName}");

            if (SupportsUnannotatedAPIs)
            {
                Contract.ThrowIfFalse(name is PredefinedEmbeddedLanguageNames.Regex or PredefinedEmbeddedLanguageNames.Json,
                    $"Only '{PredefinedEmbeddedLanguageNames.Regex}' or '{PredefinedEmbeddedLanguageNames.Json}' are allowed to '{nameof(SupportsUnannotatedAPIs)}'");
            }
        }
    }
}
