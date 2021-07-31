using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handyman.ProjectAnalyzers;
using Handyman.Types;
using Microsoft.CodeAnalysis;

namespace Handyman.DocumentAnalyzers
{
    /// <summary>
    /// A factory for optmal creation of types related to the Commerce Runtime.
    /// </summary>
    public sealed class TypeCache
    {
        // class declaration -> request
        public readonly ConcurrentDictionary<ITypeSymbol, RequestType> Requests = new ConcurrentDictionary<ITypeSymbol, RequestType>(SymbolEqualityComparer.Default);

        // class declaration -> response
        public readonly ConcurrentDictionary<ITypeSymbol, ResponseType> Responses = new ConcurrentDictionary<ITypeSymbol, ResponseType>(SymbolEqualityComparer.Default);

        // class declaration -> handler
        public readonly ConcurrentDictionary<ITypeSymbol, RequestHandlerDefinition> Handlers = new ConcurrentDictionary<ITypeSymbol, RequestHandlerDefinition>(SymbolEqualityComparer.Default);
    }
}