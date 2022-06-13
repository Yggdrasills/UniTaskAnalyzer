using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniTaskAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UniTaskForgetInvocationCodeFixProvider)), Shared]
    public class UniTaskForgetInvocationCodeFixProvider : CodeFixProvider
    {
        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(CodeFixResources.InsertForgetTitle), CodeFixResources.ResourceManager, typeof(CodeFixResources));

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UniTaskAnalyzer.DiagnosticId); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            var title = Title.ToString();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: c => InsertForgetInvocation(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> InsertForgetInvocation(Document document, InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var fullNode = GetInvocationNode(invocation) as InvocationExpressionSyntax;
            var leadingTrivia = fullNode.GetLeadingTrivia();
            var newInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"{fullNode}.Forget"));

            return document.WithSyntaxRoot((await document
                .GetSyntaxRootAsync(cancellationToken))
                .ReplaceNode(fullNode, newInvocation.WithLeadingTrivia(leadingTrivia)));
        }

        private static SyntaxNode GetInvocationNode(SyntaxNode parent)
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
