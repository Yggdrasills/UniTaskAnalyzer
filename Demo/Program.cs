using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Demo
{
    class Program
    {
        private static readonly string[] Awaitables = new[]
        {
            typeof(Task).FullName,
            typeof(Task<>).FullName,
            typeof(ConfiguredTaskAwaitable).FullName,
            typeof(ConfiguredTaskAwaitable<>).FullName,
            "System.Threading.Tasks.ValueTask", // Type not available in .net standard 1.3
            typeof(ValueTask<>).FullName,
            "System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable", // Type not available in .net standard
            typeof(ConfiguredValueTaskAwaitable<>).FullName
        };

        static void Main(string[] args)
        {
            var tree = CSharpSyntaxTree.ParseText(@"using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTaskAnalyzerConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        private static async void MainFoo(Func<UniTask> func)
        {
            IBar bar = new Bar();

            bar.Foo3().AsAsyncUnitUniTask().ContinueWith(null);
        }
    }

    public interface IBar
    {
        UniTaskVoid Foo();

        UniTask Foo2();

        UniTask<int> Foo3();
    }

    public class Bar : IBar
    {
        public UniTaskVoid Foo()
        {
            throw new NotImplementedException();
        }

        public UniTask Foo2()
        {
            throw new NotImplementedException();
        }

        public UniTask<int> Foo3()
        {
            throw new NotImplementedException();
        }
    }
}
");

            MetadataReference Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            var root = tree.GetRoot();

            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Last();

            var fullNode = GetInvocationNode(invocation) as InvocationExpressionSyntax;

            var brandNewInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"{fullNode}.Forget"));

            var trivia = fullNode.GetLeadingTrivia();

            root = root.ReplaceNode(fullNode, brandNewInvocation.WithLeadingTrivia(trivia));

            Console.WriteLine(root.SyntaxTree);

            Console.WriteLine();


            Console.ReadLine();
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
