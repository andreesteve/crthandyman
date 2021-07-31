using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
            string documentName = args.Skip(1).FirstOrDefault();
            MSBuildLocator.RegisterDefaults();

            DoWork(path, documentName).GetAwaiter().GetResult();
        }

        private static async Task DoWork(string path, string documentName)
        {
            Stopwatch stopWatch = new Stopwatch();
            Console.Write($"Creating workspace");
            stopWatch.Restart();
            using var workspace = MSBuildWorkspace.Create();
            ConsoleWriteLineElapsed(stopWatch);

            Console.Write($"Opening project {path}");
            stopWatch.Restart();
            var project = await workspace.OpenProjectAsync(path);
            ConsoleWriteLineElapsed(stopWatch);

            Console.Write($"Compiling project");
            stopWatch.Restart();
            var compilation = await project.GetCompilationAsync();
            ConsoleWriteLineElapsed(stopWatch);

            Console.WriteLine($"Starting analysis");
            stopWatch.Restart();
            var factory = new AnalysisContextFactory();

            foreach (var document in project.Documents)
            {
                if (string.IsNullOrWhiteSpace(documentName) || Path.GetFileNameWithoutExtension(document.Name).Equals(documentName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Analyzing {document.Name}");
                    stopWatch.Restart();
                    var context = await factory.CreateContextFor(document);

                    try
                    {
                        var requestHandlerAnalyzer = new RequestHandlerAnalyzer(context);
                        var handler = requestHandlerAnalyzer.TryGetRequestHandlerFromSyntaxTree();

                        if (handler == null)
                        {
                            Console.WriteLine("    NOT a handler");
                        }
                        else
                        {
                            Console.WriteLine("    **** IS a handler ****");
                        }

                        foreach (var execution in handler.RequestExecutions)
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
                    finally
                    {
                        Console.Write($"Analysis complete for {document.Name}");
                        ConsoleWriteLineElapsed(stopWatch);
                    }
                }
            }
        }

        private static async Task GetRequestDependencyTree(RequestHandlerDefinition handler)
        {
        }

        private static void ConsoleWriteLineElapsed(Stopwatch watch)
        {
            Console.WriteLine($"...{Math.Round(watch.Elapsed.TotalMilliseconds)} ms");
        }
    }
}
