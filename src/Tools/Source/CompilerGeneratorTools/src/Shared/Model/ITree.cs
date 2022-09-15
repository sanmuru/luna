using System.Collections.Immutable;
using Luna.Compilers.Generators.Syntax.Model;

namespace Luna.Compilers.Generators.Model;

public interface ITree<TTreeType, TTreeTypeChild>
    where TTreeType : ITreeType<TTreeTypeChild>
    where TTreeTypeChild : ITreeTypeChild
{
    string Root { get; }

    ImmutableList<TTreeType> Types { get; }
}
