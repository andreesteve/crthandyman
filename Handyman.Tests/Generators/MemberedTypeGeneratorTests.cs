using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Handyman.Generators;
using Handyman.Tests.TestHelpers;
using Handyman.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Handyman.Tests
{
    [TestClass]
    public class MemberedTypeGeneratorTests
    {
        [TestMethod]
        public void GenerateSyntax_ValidCode()
        {
            var compilation = RoslynHelper.Compile("class X { string[] id; }", out SyntaxTree tree);
            var member = tree.GetRoot().DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>().First();
            var symbol = (ITypeSymbol)compilation.GetSemanticModel(tree).GetTypeInfo(member.Type).Type;

            string code = new MemberedTypeGenerator().GenerateSyntax(new RequestType("Sample", new[] { new Member("id", symbol, "The identifier value.") }, "A sample request.", "System.Exception"));

            // If it doesn't throw, we are good
            RoslynHelper.Compile(code);
        }
    }
}
