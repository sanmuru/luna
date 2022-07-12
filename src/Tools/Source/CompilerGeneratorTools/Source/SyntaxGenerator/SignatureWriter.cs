namespace SyntaxGenerator;

internal abstract class SignatureWriter
{
    protected readonly TextWriter _writer;
    protected readonly Tree _tree;
    protected readonly Dictionary<string, string?> _typeMap;

    protected abstract string RootNamespace { get; }

    protected SignatureWriter(TextWriter writer, Tree tree)
    {
        this._writer = writer;
        this._tree = tree;
        this._typeMap = tree.Types.ToDictionary(n => n.Name, n => (string?)n.Base);
        this._typeMap.Add(tree.Root, null);
    }

    public virtual void WriteFile()
    {
        this.WriteUsings();
        this._writer.WriteLine();
        this._writer.WriteLine("namespace {0}", this.RootNamespace);
        this._writer.WriteLine("{");

        this.WriteTypes();

        this._writer.WriteLine("}");
    }

    protected virtual void WriteUsings()
    {
        this._writer.WriteLine("using System;");
        this._writer.WriteLine("using System.Collections;");
        this._writer.WriteLine("using System.Collections.Generic;");
        this._writer.WriteLine("using System.Linq;");
        this._writer.WriteLine("using System.Threading;");
    }

    protected virtual void WriteTypes()
    {
        var nodes = this._tree.Types.Where(n => n is not PredefinedNode).ToList();
        for (int i = 0, n = nodes.Count; i < n; i++)
        {
            var node = nodes[i];
            this._writer.WriteLine();
            this.WriteType(node);
        }
    }

    protected virtual void WriteType(TreeType node)
    {
        if (node is AbstractNode)
        {
            AbstractNode nd = (AbstractNode)node;
            this._writer.WriteLine("  public abstract partial class {0} : {1}", node.Name, node.Base);
            this._writer.WriteLine("  {");
            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];
                if (IsNodeOrNodeList(field.Type))
                {
                    this._writer.WriteLine("    public abstract {0}{1} {2} {{ get; }}", "", field.Type, field.Name);
                }
            }
            this._writer.WriteLine("  }");
        }
        else if (node is Node)
        {
            Node nd = (Node)node;
            this._writer.WriteLine("  public partial class {0} : {1}", node.Name, node.Base);
            this._writer.WriteLine("  {");

            WriteKinds(nd.Kinds);

            var valueFields = nd.Fields.Where(n => !IsNodeOrNodeList(n.Type)).ToList();
            var nodeFields = nd.Fields.Where(n => IsNodeOrNodeList(n.Type)).ToList();

            for (int i = 0, n = nodeFields.Count; i < n; i++)
            {
                var field = nodeFields[i];
                this._writer.WriteLine("    public {0}{1}{2} {3} {{ get; }}", "", "", field.Type, field.Name);
            }

            for (int i = 0, n = valueFields.Count; i < n; i++)
            {
                var field = valueFields[i];
                this._writer.WriteLine("    public {0}{1}{2} {3} {{ get; }}", "", "", field.Type, field.Name);
            }

            this._writer.WriteLine("  }");
        }
    }

    protected virtual void WriteKinds(List<Kind> kinds)
    {
        if (kinds.Count > 1)
        {
            foreach (var kind in kinds)
                this._writer.WriteLine("    // {0}", kind.Name);
        }
    }

    protected bool IsSeparatedNodeList(string typeName) =>
        typeName.StartsWith("SeparatedSyntaxList<", StringComparison.Ordinal);

    protected bool IsNodeList(string typeName) =>
        typeName.StartsWith("SyntaxList<", StringComparison.Ordinal);

    public bool IsNodeOrNodeList(string typeName) =>
        IsNode(typeName) || IsNodeList(typeName) || IsSeparatedNodeList(typeName);

    protected bool IsNode(string typeName) =>
        this._typeMap.ContainsKey(typeName);
}
