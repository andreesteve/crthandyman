using System;
using System.Linq;
using System.Threading.Tasks;
using Handyman.DocumentAnalyzers;
using Handyman.Errors;
using Handyman.ProjectAnalyzers;
using Handyman.Types;
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

                try
                {
                    var requestHandlerAnalyzer = new RequestHandlerAnalyzer(context);
                    var handler = requestHandlerAnalyzer.TryGetRequestHandlerFromSyntaxTree();

                    if (handler == null)
                    {
                        Console.WriteLine("   NOT a handler");
                    }
                    else
                    {
                        Console.WriteLine("   **** IS a handler ****");
                    }

                    foreach (var execution in new RequestExecutionAnalyzer(context).FindAll())
                    {
                        Console.WriteLine("    " + execution.ToString());
                    }

                    // var root = await syntaxTree.GetRootAsync();
                    // root.DescendantNodesAndSelf()
                    //     .Where(n => n is MethodDeclarationSyntax)
                    //     .Select(n => requestHandlerAnalyzer.TryGetHandlerDefinition )


                }
                catch (HandymanErrorException exception)
                {
                    Console.WriteLine("err: " + exception.ToString());
                }
            }
        }

        private static async Task GetRequestDependencyTree(RequestHandlerDefinition handler)
        {
        }
    }
}
