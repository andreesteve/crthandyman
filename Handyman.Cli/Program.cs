using System;
using System.Linq;
using System.Threading.Tasks;
using Handyman.DocumentAnalyzers;
using Handyman.Errors;
using Handyman.ProjectAnalyzers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace HandymanCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];
            MSBuildLocator.RegisterDefaults();

            DoWork(path).GetAwaiter().GetResult();
        }

        private static async Task DoWork(string path)
        {
            using var workspace = MSBuildWorkspace.Create();

            var project = await workspace.OpenProjectAsync(path);
            var compilation = await project.GetCompilationAsync();
            var factory = new AnalysisContextFactory();

            foreach (var document in project.Documents)
            {
                Console.WriteLine($"Handling {document.Name}");
                var context = await factory.CreateContextFor(document);

                var syntaxTree = await document.GetSyntaxTreeAsync();
                var textLength = (await syntaxTree.GetTextAsync()).Length;
                var span = new TextSpan(textLength / 2, 1);

                try
                {
                    var requestHandlerAnalyzer = new RequestHandlerAnalyzer(context);
                    var handler = requestHandlerAnalyzer.TryGetRequestHandlerFromSyntaxTree(span);

                    // var root = await syntaxTree.GetRootAsync();
                    // root.DescendantNodesAndSelf()
                    //     .Where(n => n is MethodDeclarationSyntax)
                    //     .Select(n => requestHandlerAnalyzer.TryGetHandlerDefinition )

                    if (handler == null)
                    {
                        Console.WriteLine("   not a handler");
                    }
                    else
                    {
                        Console.WriteLine("   IS a handler");
                    }
                }
                catch (HandymanErrorException exception)
                {
                    Console.WriteLine("err: " + exception.ToString());
                }
            }
        }
    }
}
