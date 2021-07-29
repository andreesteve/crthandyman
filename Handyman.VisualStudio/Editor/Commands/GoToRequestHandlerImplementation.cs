using System;
using System.ComponentModel.Design;
using System.Threading;
using CommerceRuntimeHandyman.Editor.Extensions;
using EnvDTE;
using Handyman.DocumentAnalyzers;
using Handyman.Errors;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace CommerceRuntimeHandyman.Editor.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GoToRequestHandlerImplementation
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("2ac1dc24-8580-43c7-b0fa-0be7b5bd8b0b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToRequestHandlerImplementation"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GoToRequestHandlerImplementation(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GoToRequestHandlerImplementation Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GoToRequestHandlerImplementation(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource(60 * 1000); // 1 minute timeout            
            this.NavigateToRequestHandlerDefinition(cancellation.Token);
        }

        private async void NavigateToRequestHandlerDefinition(CancellationToken cancellationToken = default(CancellationToken))
        {
            IWpfTextView view;

            if (this.ServiceProvider.TryGetActiveWpfTextView(out view))
            {
                string message = string.Empty;
                try
                {
                    var position = view.Caret.Position.BufferPosition;
                    var document = position.Snapshot.GetOpenDocumentInCurrentContextWithChanges();                    
                    var context = await AnalysisContext.Create(document, cancellationToken);
                    var location = await new RequestResponseTypeAnalyzer(context).FindImplementation(position.Position, cancellationToken);

                    if (location != null)
                    {
                        var dte = (DTE)this.ServiceProvider.GetService(typeof(DTE));
                        dte.ItemOperations.OpenFile(location.Location.SourceTree.FilePath);

                        // looks like lines are indexed at 0
                        var line = location.Location.GetMappedLineSpan().StartLinePosition.Line + 1;

                        // TODO: implement focusing on line that implements method for request
                        dte.ExecuteCommand("Edit.Goto", line.ToString());
                    }
                    else
                    {
                        // handler not found
                        message = "The type selected is not a request/response type. Check for compilation errors as they affect request/response resolution.";
                    }
                }
                catch (OperationCanceledException)
                {
                    message = "Operation cancelled due to time out.";
                }
                catch (HandymanErrorException exception)
                {
                    message = exception.Error.Message;
                }
                catch (Exception exception)
                {
                    // TODO log this
                    message = $"An unhandled exception happened :(  {exception}";
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    VsShellUtilities.ShowMessageBox(
                                this.ServiceProvider,
                                message,
                                "Error",
                                OLEMSGICON.OLEMSGICON_INFO,
                                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            else
            {
                // TODO handle no active view
            }
        }
    }
}
