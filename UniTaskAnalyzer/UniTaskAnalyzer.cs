using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UniTaskAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UniTaskAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UniTaskAnalyzer";

        private const string UniTaskFullName = "Cysharp.Threading.Tasks.UniTask";
        private const string UniTaskGenericFullName = "Cysharp.Threading.Tasks.UniTask`1";

        private const string Category = "Usage";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.UniTaskAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.UniTaskAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.UniTaskAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax node))
                return;

            var fullNode = node.GetInvocationNode() as InvocationExpressionSyntax;

            if (fullNode.Expression != node.Expression)
                return;

            if (node.Parent.IsKind(SyntaxKind.Argument))
                return;

            if (fullNode.HasForgetInvocation())
                return;

            if (fullNode.Parent is EqualsValueClauseSyntax || HasAwaitOperator(fullNode.Parent))
                return;

            var semanticModel = context.SemanticModel;

            var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            if (methodSymbol is null)
                return;

            if (IsUniTask(semanticModel, node, methodSymbol))
            {
                var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), methodSymbol.ToDisplayString());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool IsUniTask(SemanticModel semanticModel, SyntaxNode node, IMethodSymbol methodSymbol)
        {
            var typeSymbol = semanticModel.GetTypeInfo(node).Type as INamedTypeSymbol;
            var symbol = semanticModel.Compilation.GetTypeByMetadataName(UniTaskFullName);
            var symbolGeneric = semanticModel.Compilation.GetTypeByMetadataName(UniTaskGenericFullName);

            return typeSymbol.IsGenericType ?
                SymbolEqualityComparer.Default.Equals(symbolGeneric, typeSymbol.ConstructedFrom) :
                SymbolEqualityComparer.Default.Equals(symbol, methodSymbol.ReturnType);
        }

        private static bool HasAwaitOperator(SyntaxNode node)
        {
            return node is AwaitExpressionSyntax;
        }
    }
}
