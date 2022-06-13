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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UniTaskAwaitInsertionCodeFixProvider)), Shared]
    public class UniTaskAwaitInsertionCodeFixProvider : CodeFixProvider
    {
        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(CodeFixResources.InsertAwaitTitle), CodeFixResources.ResourceManager, typeof(CodeFixResources));

        public sealed override ImmutableArray<string> FixableDiagnosticIds
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
                    createChangedDocument: c => InsertAwaitOperator(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> InsertAwaitOperator(Document document, InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var identifierToken = invocation.GetFirstToken();

            var leadingTrivia = identifierToken.LeadingTrivia;

            var newDeclaration = invocation.ReplaceToken(identifierToken, identifierToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

            var awaitOperator = SyntaxFactory.AwaitExpression(newDeclaration);

            var rootNode = await document.GetSyntaxRootAsync(cancellationToken);

            var newRoot = rootNode.ReplaceNode(invocation, awaitOperator.WithLeadingTrivia(leadingTrivia));

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
