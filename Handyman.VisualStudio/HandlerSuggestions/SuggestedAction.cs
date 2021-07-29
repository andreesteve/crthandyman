using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Handyman;
using Handyman.DocumentAnalyzers;
using Handyman.Errors;
using Handyman.Generators;
using Handyman.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;

namespace CommerceRuntimeHandyman.AssociateMethodWithRequest
{
    public class SuggestedAction : ISuggestedAction
    {
        public bool HasActionSets => false;

        public string DisplayText => "Create or update request/response";

        public object IconMoniker => string.Empty;

        public string IconAutomationText => string.Empty;

        public string InputGestureText => string.Empty;

        public bool HasPreview => false;

        ImageMoniker ISuggestedAction.IconMoniker => default(ImageMoniker);

        private readonly Document document;
        private readonly int tokenPosition;
        private readonly WorkspaceManager workspaceManager;

        private bool? canRun;
        private MethodDeclarationSyntax methodSyntax;
        private AnalysisContext context;        

        public SuggestedAction(WorkspaceManager workspaceManager, Document document, int tokenPosition)
        {
            this.document = document;
            this.tokenPosition = tokenPosition;
            this.workspaceManager = workspaceManager;
        }

        public void Dispose()
        {
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public async Task<bool> CanRun(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.canRun == null)
            {
                try
                {
                    this.context = this.context ?? await AnalysisContext.Create(this.document, cancellationToken);
                    this.methodSyntax = this.context.SyntaxRoot.TryGetMethodDeclaratioNearPosition(this.tokenPosition);
                    this.canRun = this.methodSyntax != null;
                }
                catch (HandymanErrorException)
                {
                    // TODO log it
                    this.canRun = false;
                }
            }

            return this.canRun.Value;
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            if (this.CanRun(cancellationToken).Result)
            {
                var requestHandlerDefinition = new RequestHandlerAnalyzer(this.context).TryGetHandlerDefinition(this.methodSyntax);

                if (requestHandlerDefinition == null)
                {
                    MessageBox.Show("This method cannot be converted into a request handler.");
                }
                else if (!this.workspaceManager.CreateOrUpdateRequestHandlerDefinition(requestHandlerDefinition))
                {
                    MessageBox.Show("Couldn't apply changes to project");
                }
            }
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.NewGuid();
            return true;
        }
    }
}