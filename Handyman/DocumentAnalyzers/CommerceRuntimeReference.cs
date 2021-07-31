using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Handyman.Types;
using Microsoft.CodeAnalysis;

namespace Handyman
{
    /// <summary>
    /// Represents information associated to the commerce runtime.
    /// </summary>
    public sealed class CommerceRuntimeReference
    {
        public string RequestBaseClassFqn { get; internal set; }

        public string ResponseBaseClassFqn { get; internal set; }

        public INamedTypeSymbol RequestTypeSymbol { get; internal set; }

        public INamedTypeSymbol ResponseTypeSymbol { get; internal set; }

        public INamedTypeSymbol IRequestHandlerTypeSymbol { get; internal set; }

        public INamedTypeSymbol IRequestHandlerAsyncTypeSymbol { get; internal set; }

        public INamedTypeSymbol SingleAsyncRequestHandlerWithResponseTypeSymbol { get; internal set; }
        public INamedTypeSymbol SingleAsyncRequestHandlerTypeSymbol { get; internal set; }
        public INamedTypeSymbol SingleRequestHandlerTypeSymbol { get; internal set; }

        /// <summary>
        /// Represents a void response (i.e. no response).
        /// </summary>
        public ResponseType VoidResponse { get; internal set; }
    }
}
