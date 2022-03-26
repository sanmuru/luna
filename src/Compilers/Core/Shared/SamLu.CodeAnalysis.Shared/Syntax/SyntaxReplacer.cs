using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;


#if LANG_LUA
using ThisSyntaxNode = SamLu.CodeAnalysis.Lua.LuaSyntaxNode;

namespace SamLu.CodeAnalysis.Lua.Syntax;
#elif LANG_MOONSCRIPT
using ThisSyntaxNode = SamLu.CodeAnalysis.MoonScript.MoonScriptSyntaxNode;

namespace SamLu.CodeAnalysis.MoonScript.Syntax;
#endif

internal static class SyntaxReplacer
{
    internal static ThisSyntaxNode Replace<TNode>(
        ThisSyntaxNode root,
        IEnumerable<TNode>? nodes = null,
        Func<TNode, TNode, ThisSyntaxNode>? computeReplacementNode = null,
        IEnumerable<SyntaxToken>? tokens = null,
        Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
        IEnumerable<SyntaxTrivia>? trivia = null,
        Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
        where TNode : ThisSyntaxNode
    {
        var replacer = new Replacer<TNode>(
            nodes, computeReplacementNode,
            tokens, computeReplacementToken,
            trivia, computeReplacementTrivia);

        if (replacer.HasWork) return replacer.Visit(root);
        else return root;
    }

    internal static SyntaxToken Replace(
        SyntaxToken root,
        IEnumerable<ThisSyntaxNode>? nodes = null,
        Func<ThisSyntaxNode, ThisSyntaxNode, ThisSyntaxNode>? computeReplacementNode = null,
        IEnumerable<SyntaxToken>? tokens = null,
        Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
        IEnumerable<SyntaxTrivia>? trivia = null,
        Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
    {
        var replacer = new Replacer<ThisSyntaxNode>(
            nodes, computeReplacementNode,
            tokens, computeReplacementToken,
            trivia, computeReplacementTrivia);

        if (replacer.HasWork) return replacer.VisitToken(root);
        else return root;
    }

    private class Replacer<TNode> :
#if LANG_LUA
        LuaSyntaxRewriter
#elif LANG_MOONSCRIPT
        MoonScriptSyntaxRewriter
#endif
        where TNode : ThisSyntaxNode
    {
        private readonly Func<TNode, TNode, ThisSyntaxNode>? _computeReplacementNode;
        private readonly Func<SyntaxToken, SyntaxToken, SyntaxToken>? _computeReplacementToken;
        private readonly Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? _computeReplacementTrivia;

        private readonly HashSet<ThisSyntaxNode> _nodeSet;
        private readonly HashSet<SyntaxToken> _tokenSet;
        private readonly HashSet<SyntaxTrivia> _triviaSet;
        private readonly HashSet<TextSpan> _spanSet;

        private static readonly HashSet<ThisSyntaxNode> s_noNodes = new();
        private static readonly HashSet<SyntaxToken> s_noTokens = new();
        private static readonly HashSet<SyntaxTrivia> s_noTrivia = new();

        private readonly TextSpan _totalSpan;
        private readonly bool _shouldVisitTrivia;

        public bool HasWork => this._nodeSet.Count + this._tokenSet.Count + this._triviaSet.Count > 0;

        public Replacer(
            IEnumerable<TNode>? nodes,
            Func<TNode, TNode, ThisSyntaxNode>? computeReplacementNode,
            IEnumerable<SyntaxToken>? tokens,
            Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken,
            IEnumerable<SyntaxTrivia>? trivia,
            Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia)
        {
            this._computeReplacementNode = computeReplacementNode;
            this._computeReplacementToken = computeReplacementToken;
            this._computeReplacementTrivia = computeReplacementTrivia;

            this._nodeSet = nodes is null ? Replacer<TNode>.s_noNodes : new();
            this._tokenSet = tokens is null ? Replacer<TNode>.s_noTokens : new();
            this._triviaSet = trivia is null ? Replacer<TNode>.s_noTrivia : new();

            this._spanSet = new(
                new[]
                {
                    from n in this._nodeSet select n.FullSpan,
                    from t in this._tokenSet select t.FullSpan,
                    from t in this._triviaSet select t.FullSpan
                }.SelectMany(spans => spans)
            );

            this._totalSpan = Replacer<TNode>.ComputeTotalSpan(this._spanSet);

            this.VisitInfoStructuredTrivia =
                this._nodeSet.Any(n => n.IsPartOfStructuredTrivia()) ||
                this._tokenSet.Any(t => t.IsPartOfStructuredTrivia()) ||
                this._triviaSet.Any(t => t.IsPartOfStructuredTrivia());

            this._shouldVisitTrivia = this._triviaSet.Count > 0 || this.VisitInfoStructuredTrivia;
        }

        private static TextSpan ComputeTotalSpan(HashSet<TextSpan> spans)
        {
            bool first = true;
            int start = 0;
            int end = 0;

            foreach (var span in spans)
            {
                if (first)
                {
                    start = span.Start;
                    end = span.End;
                    first = false;
                }
                else
                {
                    start = Math.Min(start, span.Start);
                    end = Math.Max(end, span.End);
                }
            }

            return new TextSpan(start, end - start);
        }

        private bool ShouldVisit(TextSpan span)
        {
            if (!span.IntersectsWith(this._totalSpan)) return false;

            foreach (var s in this._spanSet)
            {
                if (span.IntersectsWith(s)) return true;
            }

            return false;
        }

        [return: NotNullIfNotNull("node")]
        public override ThisSyntaxNode? Visit(ThisSyntaxNode? node)
        {
            var rewritten = node;

            if (node is not null)
            {
                if (this.ShouldVisit(node.FullSpan))
                    rewritten = base.Visit(node);

                if (this._nodeSet.Contains(node) && this._computeReplacementNode is not null)
                    rewritten = this._computeReplacementNode((TNode)node, (TNode)rewritten!);
            }

            return rewritten;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var rewritten = token;

            if (this._shouldVisitTrivia && this.ShouldVisit(token.FullSpan))
                rewritten = base.VisitToken(token);

            if (this._tokenSet.Contains(token) && this._computeReplacementToken is not null)
                rewritten = this._computeReplacementToken(token, rewritten);

            return rewritten;
        }

        public override SyntaxTrivia VisitListElement(SyntaxTrivia trivia)
        {
            var rewritten = trivia;

            if (this.VisitInfoStructuredTrivia && trivia.HasStructure && this.ShouldVisit(trivia.FullSpan))
                rewritten = this.VisitTrivia(trivia);

            if (this._triviaSet.Contains(trivia) && this._computeReplacementTrivia is not null)
                rewritten = this._computeReplacementTrivia(trivia, rewritten);

            return rewritten;
        }
    }

#warning 未完成
}
