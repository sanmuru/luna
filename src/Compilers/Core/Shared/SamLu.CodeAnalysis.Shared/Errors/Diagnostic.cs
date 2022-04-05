using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

#if LANG_LUA
using ThisDiagnostic = SamLu.CodeAnalysis.Lua.LuaDiagnostic;
using ThisDiagnosticFormatter = SamLu.CodeAnalysis.Lua.LuaDiagnosticFormatter;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using ThisDiagnostic = SamLu.CodeAnalysis.MoonScript.MoonScriptDiagnostic;
using ThisDiagnosticFormatter = SamLu.CodeAnalysis.MoonScript.MoonScriptDiagnosticFormatter;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

internal class
#if LANG_LUA
    LuaDiagnostic
#elif LANG_MOONSCRIPT
    MoonScriptDiagnostic
#endif
    : DiagnosticWithInfo
{
    internal
#if LANG_LUA
        LuaDiagnostic
#elif LANG_MOONSCRIPT
        MoonScriptDiagnostic
#endif
        (DiagnosticInfo info, Location location, bool isSuppressed = false) : base(info, location, isSuppressed) { }

    public override string ToString() => ThisDiagnosticFormatter.Instance.Format(this);

    internal override Diagnostic WithLocation(Location location)
    {
        if (this.Location != location)
            return new ThisDiagnostic(this.Info, location, this.IsSuppressed);
        else return this;
    }

    internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
    {
        if (this.Severity != severity)
            return new ThisDiagnostic(this.Info.GetInstanceWithSeverity(severity), this.Location, this.IsSuppressed);
        else return this;
    }

    internal override Diagnostic WithIsSuppressed(bool isSuppressed)
    {
        if (this.IsSuppressed != isSuppressed)
            return new ThisDiagnostic(this.Info, this.Location, isSuppressed);
        else return this;
    }
}
