using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handyman.Types
{
    /// <summary>
    /// Represents an execution of a Request in the source code.
    /// </summary>
    public class RequestExecution
    {
        public RequestExecution(RequestType request, InvocationExpressionSyntax invocationSyntax)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.InvocationSyntax = invocationSyntax ?? throw new ArgumentNullException(nameof(invocationSyntax));
        }

        /// <summary>
        /// The request invoked.
        /// </summary>
        public RequestType Request { get; }

        // /// <summary>
        // /// The request invoked.
        // /// </summary>
        // public RequestType Response { get; }

        /// <summary>
        /// The syntax node where the method invocation occurred.
        /// </summary>
        public InvocationExpressionSyntax InvocationSyntax { get; }

        public override string ToString()
        {
            return $"{System.IO.Path.GetFileName(this.InvocationSyntax.SyntaxTree.FilePath)}:{this.InvocationSyntax.SyntaxTree.GetLineSpan(this.InvocationSyntax.Span).StartLinePosition.Line} -> Execute<?>({this.Request})";
        }
    }
}