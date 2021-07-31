using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Handyman.Types;
using Handyman.Errors;
using Handyman.Comparers;
using System;

namespace Handyman.DocumentAnalyzers
{
    /// <summary>
    /// Analyzes syntax tree and tries to find a reference to a request or response.
    /// </summary>
    public sealed class RequestResponseTypeAnalyzer
    {
        private readonly AnalysisContext context;

        public RequestResponseTypeAnalyzer(AnalysisContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Given a type symbol for the declaration of a request, resolve a RequestType.
        /// </summary>
        /// <param name="declaringType">The type that declares the request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The request or null if declaring type is not a request.</returns>
        public RequestType ResolveRequestFromDeclaringType(ITypeSymbol declaringType)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            if (!this.context.TypeCache.Requests.TryGetValue(declaringType, out RequestType request))
            {
                if (declaringType.IsDerivedFrom(this.context.CommerceRuntimeReference.RequestTypeSymbol))
                {
                    // TODO: parse members and documentation
                    request = new RequestType(declaringType, Enumerable.Empty<Member>(), string.Empty, this.context.CommerceRuntimeReference.RequestBaseClassFqn);
                }

                // cache even negative cases to avoid reanalyzing
                this.context.TypeCache.Requests.TryAdd(declaringType, request);
            }

            return request;
        }

        public Task<TypeLocation<RequestHandlerDefinition>> FindRequestImplementation(AnalysisContextFactory contextFactory, int tokenPosition, CancellationToken cancellationToken = default)
        {
            // TODO: handle class definition symbol
            SyntaxNode node = this.context.SyntaxRoot.FindToken(tokenPosition).Parent;
            var requestType = this.GetRequestReferenceOrThrow(node, search: 3, cancellationToken);
            return this.FindRequestImplementation(contextFactory, requestType, cancellationToken);
        }

        /// <summary>
        /// Given a request type, finds all locations that implement the request type.
        /// </summary>
        /// <param name="requestType">The request type to search implementations for.</param>
        /// <param name="contextFactory">An existing context factory to speed up the search.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The locations where the request is implemented in a request handler.</returns>
        public async Task<TypeLocation<RequestHandlerDefinition>> FindRequestImplementation(AnalysisContextFactory contextFactory, RequestType requestType, CancellationToken cancellationToken = default)
        {
            var locations = (await SymbolFinder.FindReferencesAsync(requestType.DeclaringType, context.Document.Project.Solution, cancellationToken))
                // TODO figure out when we need r.Definition.Name == info.Type.Name (HACK!!!)
                .FirstOrDefault(r => r.Definition.Equals(requestType.DeclaringType, SymbolEqualityComparer.Default) || r.Definition.Name == requestType.DeclaringType.Name)?.Locations
                    ?? Enumerable.Empty<ReferenceLocation>();

            // creating a new factory will make it very slow as we cannot cache anything
            contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            // for each location, analyze the code and see if it is a request handler
            var requestHandlersTasks = locations.Select(async l =>
            {
                var context = await contextFactory.CreateContextFor(l.Document, cancellationToken);
                var analyzer = new RequestHandlerAnalyzer(context);
                return analyzer.TryGetRequestHandlerFromSyntaxTree(l.Location.SourceSpan, cancellationToken);
            }).ToArray();

            await Task.WhenAll(requestHandlersTasks);

            // filter out results that are not request handlers
            var requestHandlers = requestHandlersTasks.Select(r => r.Result).Where(r => r != null);

            // TODO: implement a method on RequestHandlerAnalyser that can analyze multiple instances in batch, given that
            // we can perform some optizations (like filter out locations within same class)

            // for each request handler found, see if it implements the request we have
            var requestHandler = requestHandlers.FirstOrDefault(h => h.DeclaredSupportedRequestTypes.Any(request => request.Equals(requestType)));

            if (requestHandler == null)
            {
                // // FIXME: this is unexpected, I think, revisit if we need to throw really
                // throw new HandymanErrorException(new Error("NoRequestHandlerFound", $"No request handler was found to implement '{requestType}'."));
                return null;
            }

            var requestHandlerAnalysisContext = await contextFactory.CreateContextFor(requestHandler.Document, cancellationToken);
            var requestLocations = RequestHandlerAnalyzer.FindRequestUseLocations(requestHandler, requestHandler.ExecuteMethodSyntax, requestHandlerAnalysisContext, cancellationToken);

            // because requestLocation.TypeSymbol can be on a different compilation than info.Type
            // we cannot compare the objects directly
            string displayName = requestType.DeclaringType.ToDisplayString();
            var location = requestLocations.FirstOrDefault(l => l.TypeSymbol.Equals(requestType.DeclaringType, SymbolEqualityComparer.Default) || l.TypeSymbol.ToDisplayString() == displayName)
                ?? new TypeLocation<RequestHandlerDefinition>() { ContainingType = requestHandler, Location = requestHandlerAnalysisContext.SyntaxRoot.GetLocation() }; // if not found, default's to handler's location

            return location;
        }

        /// <summary>
        /// Given a syntax node, tries to figure out the closest token that references a Request. Throws if nothing closeby references a request.
        /// </summary>
        /// <param name="node">The node in the syntax tree.</param>
        /// <param name="search">How many nodes to search around provided node. Default is 0.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The TypeInfo of the referenced request.</returns>
        private RequestType GetRequestReferenceOrThrow(SyntaxNode node, uint search = 0, CancellationToken cancellationToken = default)
        {
            // not always the identifier name syntax node will result in the type info directly (e.g. on a constructor statement, you need the constructor statement itself)
            // I haven't found out a deterministic way to do this, but it seems intuitive that the type information is not 'too far away' from the identifier
            TypeInfo info = default;
            for (int i = 0;
                i < search && node != null && (info = this.context.SemanticModel.GetTypeInfo(node, cancellationToken)).Type == null;
                node = node.Parent, i++) ;

            if (info.Type == null)
            {
                throw new HandymanErrorException(new Error("NotAType", "The selected token is not a type. Make sure you have selected a type and you have no compilation error."));
            }

            var requestType = this.ResolveRequestFromDeclaringType(info.Type);

            if (requestType == null)
            {
                throw new HandymanErrorException(new Error("NotARequestType", $"The selected type '{info.Type.Name}' does not implement contract of a Request type."));
            }

            return requestType;
        }
    }
}