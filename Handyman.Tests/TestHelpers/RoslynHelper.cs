using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Handyman.Tests.TestHelpers
{
    public class RoslynHelper
    {
        private static readonly SyntaxTree[] BaseTypesSyntaxTree = ParseBaseTypes();
        private static readonly string RuntimePath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

        private static readonly IEnumerable<MetadataReference> DefaultReferences =
            new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Join(RuntimePath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Join(RuntimePath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Join(RuntimePath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Join(RuntimePath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Join(RuntimePath, typeof(Task).Assembly.GetName().Name + ".dll")),
            };

        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Debug)
                    .WithWarningLevel(0);

        public static Compilation Compile(string code)
        {
            return Compile(code, out var _);
        }

        public static Compilation Compile(string code, out SyntaxTree codeSyntaxTree)
        {
            codeSyntaxTree = ParseText(code);
            var trees = BaseTypesSyntaxTree.Concat(new[] { codeSyntaxTree }).ToArray();
            var compilation = CSharpCompilation.Create("test", trees, DefaultReferences, DefaultCompilationOptions);

            var errors = compilation.GetDiagnostics();

            if (errors.Any())
            {
                throw new ArgumentException($"Compilation errors: { string.Join("\n", errors.Select(e => e.GetMessage())) }");
            }

            return compilation;
        }

        private static SyntaxTree[] ParseBaseTypes()
        {
            string cd = Path.Combine(Directory.GetCurrentDirectory(), "BaseTypes");
            List<SyntaxTree> trees = new List<SyntaxTree>();

            foreach (string path in Directory.GetFiles(cd, "*.cs"))
            {
                trees.Add(ParseFile(path));
            }

            return trees.ToArray();
        }

        private static SyntaxTree ParseFile(string path)
        {
            using var stream = File.OpenRead(path);
            return SyntaxFactory.ParseSyntaxTree(SourceText.From(stream, Encoding.UTF8));
        }

        private static SyntaxTree ParseText(string text)
        {
            return SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
        }
    }
}
