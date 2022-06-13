using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UniTaskAnalyzer
{
    public static class SyntaxNodeExtensions
    {

        public static bool HasForgetInvocation(this SyntaxNode node)
        {
            return GetInvocationNode(node).ToString().Contains(".Forget");
        }

        public static SyntaxNode GetInvocationNode(this SyntaxNode parent)
        {
            var syntaxNode = parent.ChildNodes().Last();

            if (syntaxNode.IsKind(SyntaxKind.InvocationExpression))
            {
                return parent.ChildNodes().Last();
            }
            else if (!syntaxNode.IsKind(SyntaxKind.ExpressionStatement))
            {
                return GetInvocationNode(parent.Parent);
            }

            return parent.ChildNodes().Last();
        }
    }
}
