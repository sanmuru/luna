﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.InheritanceChainMargin;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.CodeAnalysis.Editor.InheritanceChainMargin
{
    internal class InheritanceMarginTag : ITag
    {
        public readonly TaggedText DescriptionText;
        public readonly int LineNumber;
        public readonly Func<Task> NavigationFunc;

        private InheritanceMarginTag(
            TaggedText descriptionText,
            int lineNumber,
            Func<Task> navigationFunc)
        {
            DescriptionText = descriptionText;
            LineNumber = lineNumber;
            NavigationFunc = navigationFunc;
        }

        public static InheritanceMarginTag FromInheritanceInfo(InheritanceInfo inheritanceInfo)
        {

        }
    }
}
