using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Handyman.ProjectAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Handyman.DocumentAnalyzers
{
    /// <summary>
    /// A class that groups together the document, the syntax tree root and the semantic model.
    /// </summary>
    public class AnalysisContext
    {
        public Document Document { get; private set; }

        public SyntaxNode SyntaxRoot { get; private set; }

        public SemanticModel SemanticModel { get; private set; }

        public CommerceRuntimeReference CommerceRuntimeReference { get; private set; }

        public TypeCache TypeCache { get; set; }

        private ILoggerFactory loggerFactory;

        public static async Task<AnalysisContext> Create(ILoggerFactory loggerFactory, Document document, CancellationToken cancellationToken, TypeCache typeCache, CommerceRuntimeReference reference = null)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var syntaxRoot = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken);
            reference = reference ?? throw new ArgumentNullException(nameof(reference));

            return new AnalysisContext()
            {
                Document = document,
                SemanticModel = semanticModel,
                SyntaxRoot = syntaxRoot,
                CommerceRuntimeReference = reference,
                TypeCache = typeCache,
                loggerFactory = loggerFactory,
            };
        }

        internal ILogger NewLogger<T>()
        {
            return this.loggerFactory.CreateLogger<T>();
        }
    }
}
