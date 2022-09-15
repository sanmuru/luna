using System.Collections.Immutable;
using Luna.Compilers.Generators.Syntax.Model;

namespace Luna.Compilers.Generators.Model;

public interface ITreeType<TTreeTypeChild>
    where TTreeTypeChild : ITreeTypeChild
{
    string Name { get; }

    string? Base { get; }

    ImmutableList<TTreeTypeChild> Children { get; }
}
