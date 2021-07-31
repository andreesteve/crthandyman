using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handyman.ProjectAnalyzers;
using Microsoft.CodeAnalysis;

namespace Handyman.DocumentAnalyzers
{
    /// <summary>
    /// A factory for optmal creation of <see cref="AnalysisContext"/>.
    /// This class assumes that there are no concurrent <see cref="Compilation"/> instances for the same <see cref="Project"/>.
    /// </summary>
    public sealed class AnalysisContextFactory
    {
        private readonly ConcurrentDictionary<Project, ProjectCache> projects = new ConcurrentDictionary<Project, ProjectCache>();
        private readonly ConcurrentDictionary<Document, AnalysisContext> documents = new ConcurrentDictionary<Document, AnalysisContext>();

        /// <summary>
        /// Creates an <see cref="AnalysisContext"/> for <paramref name="document"/>.
        /// </summary>
        /// <param name="document">The document under analysis.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="AnalysisContext"/> for <paramref name="document"/>.</returns>
        public async Task<AnalysisContext> CreateContextFor(Document document, CancellationToken cancellationToken = default)
        {
            // this caches multiple uses of project (e.g. project with many documents)
            if (!this.projects.TryGetValue(document.Project, out ProjectCache projectCache))
            {
                var compilation = await document.Project.GetCompilationAsync(cancellationToken);
                var reference = await new CommerceRuntimeReferenceAnalyzer(document.Project).Find(cancellationToken);
                projectCache = new ProjectCache()
                {
                    Compilation = compilation,
                    CommerceRuntimeReference = reference,
                    TypeCache = new TypeCache(),
                };
                this.projects.TryAdd(document.Project, projectCache);
            }

            // this caches multiple uses of document
            if (!documents.TryGetValue(document, out AnalysisContext context))
            {
                context = await AnalysisContext.Create(document, cancellationToken, projectCache.TypeCache, projectCache.CommerceRuntimeReference);
                documents.TryAdd(document, context);
            }

            return context;
        }

        private class ProjectCache
        {
            public Compilation Compilation { get; set; }

            public CommerceRuntimeReference CommerceRuntimeReference { get; set; }

            public TypeCache TypeCache { get; set; }
        }
    }
}