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
            Console.WriteLine($"Creating workspace");
            using var workspace = MSBuildWorkspace.Create();

            Console.WriteLine($"Opening project {path}");
            var project = await workspace.OpenProjectAsync(path);

            Console.WriteLine($"Compiling project");
            var compilation = await project.GetCompilationAsync();

            Console.WriteLine($"Starting analysis");
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
                    foreach (var execution in new RequestExecutionAnalyzer(context).FindAll())
                    {
                        Console.WriteLine("    " + execution.ToString());
                    }

                    var requestHandlerAnalyzer = new RequestHandlerAnalyzer(context);
                    var handler = requestHandlerAnalyzer.TryGetRequestHandlerFromSyntaxTree(span);

                    // var root = await syntaxTree.GetRootAsync();
                    // root.DescendantNodesAndSelf()
                    //     .Where(n => n is MethodDeclarationSyntax)
                    //     .Select(n => requestHandlerAnalyzer.TryGetHandlerDefinition )

                    if (handler == null)
                    {
                        Console.WriteLine("   NOT a handler");
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
