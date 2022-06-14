using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UniTaskAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UniTaskVoidAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UniTaskVoidAnalyzer";

        private const string UniTaskVoidFullName = "Cysharp.Threading.Tasks.UniTaskVoid";

        private const string Category = "Usage";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.UniTaskAnalyzerTitle), Resources.ResourceManager, typeof(Resources));

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
            {
                return;
            }

            if (node.HasForgetInvocation())
                return;

            var semanticModel = context.SemanticModel;

            if (node.Parent is EqualsValueClauseSyntax)
                return;

            var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            if (methodSymbol == null)
                return;

            if (methodSymbol.ReturnType.ToString().Equals(UniTaskVoidFullName))
            {
                var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), methodSymbol.ToDisplayString());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
