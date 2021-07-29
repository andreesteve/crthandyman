using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace CommerceRuntimeHandyman.AssociateMethodWithRequest
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Test suggested action")]
    [ContentType("text")]
    public class SuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
    {
        [Import]
        private WorkspaceManagerFactory workspaceManager = null;

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            if (textBuffer == null || textView == null)
            {
                return null;
            }

            var document = textBuffer.GetRelatedDocuments().First();
            return new SuggestedActionsSource(this.workspaceManager.Manager, document);
        }
    }
}
