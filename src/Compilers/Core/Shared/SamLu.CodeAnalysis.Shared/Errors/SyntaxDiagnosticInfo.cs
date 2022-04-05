﻿using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

#if LANG_LUA
using This = SamLu.CodeAnalysis.Lua;

namespace SamLu.CodeAnalysis.Lua;
#elif LANG_MOONSCRIPT
using This = SamLu.CodeAnalysis.MoonScript;

namespace SamLu.CodeAnalysis.MoonScript;
#endif

internal class SyntaxDiagnosticInfo : DiagnosticInfo
{
    internal readonly int Offset;
    internal readonly int Width;

    internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args) : base(This.MessageProvider.Instance, (int)code, args)
    {
        Debug.Assert(width >= 0);
        this.Offset = offset;
        this.Width = width;
    }

    internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code) : this(offset, width, code, Array.Empty<object>()) { }

    internal SyntaxDiagnosticInfo(ErrorCode code, params object[] args) : this(0, 0, code, args) { }

    internal SyntaxDiagnosticInfo(ErrorCode code) : this(0, 0, code) { }

    public SyntaxDiagnosticInfo WithOffset(int offset) => new SyntaxDiagnosticInfo(offset, this.Width, (ErrorCode)this.Code, this.Arguments);

    #region 序列化
    static SyntaxDiagnosticInfo() => ObjectBinder.RegisterTypeReader(typeof(SyntaxDiagnosticInfo), r => new SyntaxDiagnosticInfo(r));

    protected SyntaxDiagnosticInfo(ObjectReader reader) : base(reader)
    {
        this.Offset = reader.ReadInt32();
        this.Width = reader.ReadInt32();
    }

    protected override void WriteTo(ObjectWriter writer)
    {
        base.WriteTo(writer);
        writer.WriteInt32(this.Offset);
        writer.WriteInt32(this.Width);
    }
    #endregion
}
