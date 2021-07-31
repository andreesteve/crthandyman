using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Handyman.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handyman.DocumentAnalyzers
{
    public class RequestExecutionAnalyzer
    {
        private readonly AnalysisContext context;

        public RequestExecutionAnalyzer(AnalysisContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Analyzes the document and return all instances of request executions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>All instances of request executions, or an empty collection if none found.</returns>
        public IEnumerable<RequestExecution> FindAll(CancellationToken cancellationToken = default)
        {
            var requestAnalyzer = new RequestResponseTypeAnalyzer(this.context);
            // search in the syntax tree for any method calls in which the methods ends with "Execute"
            foreach (var methodInvokationNode in this.context.SyntaxRoot.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (this.context.SemanticModel.GetSymbolInfo(methodInvokationNode, cancellationToken).Symbol is IMethodSymbol methodSymbol
                    && (methodSymbol.Name == "Execute" || methodSymbol.Name == "ExecuteAsync"))
                {
                    // TODO heuristic, if the method is defined in one of the common runtime assemblies, assume this is a valid execute
                    //methodSymbol.OriginalDefinition.ContainingType.ContainingAssembly.Name.StartsWith("Microsoft.Dynamics.Commerce.Runtime.Entities")

                    for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        var p = methodSymbol.Parameters[i];
                        if (p.Type.Equals(this.context.CommerceRuntimeReference.RequestTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            // this is a execute method that takes in a request type
                            // try to resolve type of the request argument
                            var requestArgumentToken = methodInvokationNode.ArgumentList.Arguments[i].Expression;
                            //var requestArgumentSymbol = this.context.SemanticModel.GetSymbolInfo(requestArgumentToken, cancellationToken).Symbol;
                            var requestConcreteType = this.context.SemanticModel.GetTypeInfo(requestArgumentToken).Type;
                            if (requestConcreteType != null)
                            {
                                var requestType = requestAnalyzer.ResolveRequestFromDeclaringType(requestConcreteType);
                                yield return new RequestExecution(requestType, methodInvokationNode);
                            }
                        }
                    }
                }
            }
        }
    }
}