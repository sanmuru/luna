﻿using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SamLu.CodeAnalysis.MoonScript.Syntax.InternalSyntax;

partial class Lexer
{
    private partial bool ScanMultiLineRawStringLiteral(ref TokenInfo info, int level)
    {
        if (this.ScanLongBrackets(out var isTerminal, level))
        {
            info.Kind = SyntaxKind.MultiLineRawStringLiteralToken;
            info.ValueKind = SpecialType.System_String;
            info.Text = this.TextWindow.GetText(intern: true);
            info.StringValue = this.TextWindow.Intern(this._builder);

            this.CheckFeatureAvaliability(MessageID.IDS_FeatureMultiLineRawStringLiteral);
            if (!isTerminal)
                this.AddError(ErrorCode.ERR_UnterminatedStringLiteral);

            return true;
        }

        return false;
    }
}
