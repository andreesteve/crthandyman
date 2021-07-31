using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Handyman.Types;
using Handyman.Errors;
using Handyman.Comparers;

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

        public async Task<TypeLocation> FindImplementation(int tokenPosition, CancellationToken cancellationToken = default)
        {
            // TODO: handle class definition symbol
            SyntaxNode node = this.context.SyntaxRoot.FindToken(tokenPosition).Parent;
            var info = this.GetRequestReferenceOrThrow(node, search: 3, cancellationToken);

            var locations = (await SymbolFinder.FindReferencesAsync(info.Type, context.Document.Project.Solution, cancellationToken))
                // TODO figure out when we need r.Definition.Name == info.Type.Name (HACK!!!)
                .First(r => r.Definition.Equals(info.Type, SymbolEqualityComparer.Default) || r.Definition.Name == info.Type.Name)?.Locations
                    ?? Enumerable.Empty<ReferenceLocation>();

            AnalysisContextFactory contextFactory = new AnalysisContextFactory();

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
            var requestHandler = requestHandlers.FirstOrDefault(h => h.DeclaredSupportedRequestTypes.Contains(info.Type, new ITypeSymbolEqualityComparer()));

            if (requestHandler == null)
            {
                throw new HandymanErrorException(new Error("NoRequestHandlerFound", $"No request handler was found to implement '{info.Type.Name}'."));
            }

            var requestHandlerAnalysisContext = await contextFactory.CreateContextFor(requestHandler.Document, cancellationToken);
            var requestLocations = RequestHandlerAnalyzer.FindRequestImplementationLocations(requestHandler, requestHandlerAnalysisContext, cancellationToken);

            // because requestLocation.TypeSymbol can be on a different compilation than info.Type
            // we cannot compare the objects directly
            string displayName = info.Type.ToDisplayString();
            var location = requestLocations.FirstOrDefault(l => l.TypeSymbol.Equals(info.Type, SymbolEqualityComparer.Default) || l.TypeSymbol.ToDisplayString() == displayName)
                ?? new TypeLocation() { Location = requestHandlerAnalysisContext.SyntaxRoot.GetLocation() }; // if not found, default's to handler's location

            return location;
        }

        /// <summary>
        /// Given a syntax node, tries to figure out the closest token that references a Request. Throws if nothing closeby references a request.
        /// </summary>
        /// <param name="node">The node in the syntax tree.</param>
        /// <param name="search">How many nodes to search around provided node. Default is 0.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The TypeInfo of the referenced request.</returns>
        internal TypeInfo GetRequestReferenceOrThrow(SyntaxNode node, uint search = 0, CancellationToken cancellationToken = default)
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

            bool isRequest = info.Type.IsDerivedFrom(this.context.CommerceRuntimeReference.RequestTypeSymbol);

            if (!isRequest)
            {
                throw new HandymanErrorException(new Error("NotARequestType", $"The selected type '{info.Type.Name}' is not a Request."));
            }

            return info;
        }
    }
}