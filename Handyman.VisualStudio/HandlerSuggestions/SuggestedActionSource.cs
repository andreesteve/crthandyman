using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Handyman;
using Handyman.DocumentAnalyzers;
using Handyman.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace CommerceRuntimeHandyman.AssociateMethodWithRequest
{
    public class SuggestedActionsSource : ISuggestedActionsSource
    {
        private WorkspaceManager workspaceManager;
        public event EventHandler<EventArgs> SuggestedActionsChanged;
        private readonly Document document;

        public SuggestedActionsSource(WorkspaceManager workspaceManager, Document document)
        {
            this.workspaceManager = workspaceManager;
            this.document = document;
        }

        public void Dispose()
        {
            SuggestedActionsChanged = null;
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return new[] { new SuggestedActionSet(new[] { new SuggestedAction(this.workspaceManager, document, range.Start.Position) }) };
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return new SuggestedAction(this.workspaceManager, document, range.Start.Position).CanRun(cancellationToken);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.NewGuid();
            return true;
        }
    }
}
