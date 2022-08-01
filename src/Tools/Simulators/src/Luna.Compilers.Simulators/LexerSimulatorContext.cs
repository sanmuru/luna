using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Luna.Compilers.Simulators;

public readonly struct LexerSimulatorContext
{
    private readonly string _languageName;

    public string LanguageName => this._languageName;

    internal LexerSimulatorContext(string languageName)
    {
        this._languageName = languageName;
    }
}
