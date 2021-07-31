using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handyman.Errors;
using Handyman.ProjectAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

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
        internal ILoggerFactory LoggerFactory { get; private set; }

        public AnalysisContextFactory(ILoggerFactory loggerFactory)
        {
            this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Creates an <see cref="AnalysisContext"/> for <paramref name="document"/>.
        /// </summary>
        /// <param name="document">The document under analysis.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="AnalysisContext"/> for <paramref name="document"/>.</returns>
        public Task<AnalysisContext> CreateContextFor(Document document, CancellationToken cancellationToken = default)
        {
            return this.CreateContextForInternal(throwOnError: true, document, cancellationToken);
        }

        /// <summary>
        /// Creates an <see cref="AnalysisContext"/> for <paramref name="document"/>.
        /// </summary>
        /// <param name="document">The document under analysis.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="AnalysisContext"/> for <paramref name="document"/>.</returns>
        public Task<AnalysisContext> TryCreateContextFor(Document document, CancellationToken cancellationToken = default)
        {
            return this.CreateContextForInternal(throwOnError: false, document, cancellationToken);
        }

        private async Task<AnalysisContext> CreateContextForInternal(bool throwOnError, Document document, CancellationToken cancellationToken = default)
        {
            // FIXME: review this implementation, need to remove failed resolutions from cache when new compilation occurs

            // this caches multiple uses of project (e.g. project with many documents)
            if (!this.projects.TryGetValue(document.Project, out ProjectCache projectCache))
            {
                var compilation = await document.Project.GetCompilationAsync(cancellationToken);
                var reference = await new CommerceRuntimeReferenceAnalyzer(this.LoggerFactory.CreateLogger<CommerceRuntimeReferenceAnalyzer>(), document.Project).Find(throwOnError, cancellationToken);

                if (reference != null)
                {
                    projectCache = new ProjectCache()
                    {
                        Compilation = compilation,
                        CommerceRuntimeReference = reference,
                        TypeCache = new TypeCache(),
                    };
                }

                // cache even in case of resolution error (projectCache => null), so we don't need to try again
                this.projects.TryAdd(document.Project, projectCache);
            }

            if (projectCache == null)
            {
                if (throwOnError)
                {
                    throw new HandymanErrorException(new Error(Error.ErrorCode.CannotCreateAnalysisContext, $"Cannot create analysis context for project {document.Project.Name}. Caller required to halt on error. This is likely due to a failed compilation for the project."));
                }
                return null;
            }

            // this caches multiple uses of document
            if (!documents.TryGetValue(document, out AnalysisContext context))
            {
                context = await AnalysisContext.Create(this.LoggerFactory, document, cancellationToken, projectCache.TypeCache, projectCache.CommerceRuntimeReference);
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