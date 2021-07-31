using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handyman.Types
{
    /// <summary>
    /// Represents a request handler definition. This is usually a class that implements the required interfaces that define a request handler.
    /// </summary>
    /// <remarks>A request handler may handle one or more request types.</remarks>
    public sealed class RequestHandlerDefinition
    {
        /// <summary>
        /// Gets the type of the class that implements the request handler.
        /// </summary>
        public ITypeSymbol ClassType { get; internal set; }

        /// <summary>
        /// Gets the type of the interface that the request handler implements.
        /// </summary>
        public ITypeSymbol HandlerInterface { get; internal set; }

        /// <summary>
        /// Gets the document that contains this handler definition.
        /// </summary>
        public Document Document { get; internal set; }

        /// <summary>
        /// Gets a collection of request types that the implementation claims to support.
        /// </summary>
        public IEnumerable<RequestType> DeclaredSupportedRequestTypes { get; internal set; }

        /// <summary>
        /// Gets the syntax declarion for the Execute method of this handler.
        /// </summary>
        /// <value></value>
        public MethodDeclarationSyntax ExecuteMethodSyntax { get; internal set; }
    }
}