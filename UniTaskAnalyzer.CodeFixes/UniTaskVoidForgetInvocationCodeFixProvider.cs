using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace UniTaskAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UniTaskVoidForgetInvocationCodeFixProvider)), Shared]
    class UniTaskVoidForgetInvocationCodeFixProvider : UniTaskForgetInvocationCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UniTaskVoidAnalyzer.DiagnosticId); }
        }
    }
}
