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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HandymanCmd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = args[0];
            string projectName = args.Skip(1).FirstOrDefault();
            string documentName = args.Skip(2).FirstOrDefault();
            MSBuildLocator.RegisterDefaults();

            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConsole();
            });
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                await DoWork(serviceProvider.GetService<ILoggerFactory>(), path, projectName, documentName);
            }
        }

        private static async Task DoWork(ILoggerFactory loggerFactory, string path, string projectName, string documentName)
        {
            Stopwatch stopWatch = new Stopwatch();
            Console.Write($"Creating workspace");
            stopWatch.Restart();
            using var workspace = MSBuildWorkspace.Create();
            ConsoleWriteLineElapsed(stopWatch);

            Project project;
            if (Path.GetExtension(path) == ".sln")
            {
                if (string.IsNullOrWhiteSpace(projectName))
                {
                    Console.Error.WriteLine("A project name must be provided when loading a solution.");
                    return;
                }

                Console.Write($"Opening solution {path}");
                stopWatch.Restart();
                var solution = await workspace.OpenSolutionAsync(path);
                project = solution.Projects.FirstOrDefault(p => p.Name == projectName);
                ConsoleWriteLineElapsed(stopWatch);
            }
            else
            {
                Console.Write($"Opening project {path}");
                stopWatch.Restart();
                project = await workspace.OpenProjectAsync(path);
                ConsoleWriteLineElapsed(stopWatch);
            }
            if (project == null)
            {
                Console.Error.WriteLine("Project not found.");
                return;
            }

            Console.Write($"Compiling project");
            stopWatch.Restart();
            var compilation = await project.GetCompilationAsync();
            ConsoleWriteLineElapsed(stopWatch);

            Console.WriteLine($"Starting analysis");
            stopWatch.Restart();
            var factory = new AnalysisContextFactory(loggerFactory);

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
                        var requestResponseTypeAnalyzer = new RequestResponseTypeAnalyzer(context);
                        var handler = requestHandlerAnalyzer.TryGetRequestHandlerFromSyntaxTree();

                        Console.Write("    Is RequestHandler? ");
                        if (handler == null)
                        {
                            Console.WriteLine("NO");
                        }
                        else
                        {
                            Console.WriteLine("YES");
                            Console.WriteLine("    Implements:");
                            foreach (var request in handler.DeclaredSupportedRequestTypes)
                            {
                                Console.WriteLine($"        * {request}");
                            }

                            Console.WriteLine("    Depends on:");
                            foreach (var execution in handler.RequestExecutions)
                            {
                                Console.WriteLine($"        * {execution}");

                                // find handler that implements this request
                                var requestImplementationLocation = await requestResponseTypeAnalyzer.FindRequestImplementation(factory, execution.Request);
                                Console.WriteLine($"            |-- implemented by: {requestImplementationLocation?.ToString() ?? "unknown"}");
                            }
                        }
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

        private static void ConsoleWriteLineElapsed(Stopwatch watch)
        {
            Console.WriteLine($"...{Math.Round(watch.Elapsed.TotalMilliseconds)} ms");
        }
    }
}
