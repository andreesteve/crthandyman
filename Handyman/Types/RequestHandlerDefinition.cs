using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Handyman.Types
{
    /// <summary>
    /// Represents a request handler definition. This is usually a class that implements the required interfaces that define a request handler.
    /// </summary>
    /// <remarks>A request handler may handle one or more request types.</remarks>
    public sealed class RequestHandlerDefinition
    {
        public RequestHandlerDefinition(ITypeSymbol classType, ITypeSymbol handlerInterface, Document document, IEnumerable<ITypeSymbol> declaredSupportedRequestTypes)
        {
            this.ClassType = classType;
            this.HandlerInterface = handlerInterface;
            this.Document = document;
            this.DeclaredSupportedRequestTypes = declaredSupportedRequestTypes;
        }

        /// <summary>
        /// Gets the type of the class that implements the request handler.
        /// </summary>
        public ITypeSymbol ClassType { get; private set; }

        /// <summary>
        /// Gets the type of the interface that the request handler implements.
        /// </summary>
        public ITypeSymbol HandlerInterface { get; private set; }

        /// <summary>
        /// Gets the document that contains this handler definition.
        /// </summary>
        public Document Document { get; private set; }

        /// <summary>
        /// Gets a collection of request types that the implementation claims to support.
        /// </summary>
        public IEnumerable<ITypeSymbol> DeclaredSupportedRequestTypes { get; private set; }
    }
}