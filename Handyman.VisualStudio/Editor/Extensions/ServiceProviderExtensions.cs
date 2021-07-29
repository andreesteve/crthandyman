using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CommerceRuntimeHandyman.Editor.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IServiceProvider"/> associated with the Visual Studio Editor.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Tries to get the active <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="serviceProvider">The COM service provider.</param>
        /// <param name="textView">Output parameter for the <see cref="IWpfTextView"/>.</param>
        /// <returns>A value indicating whether there is an active view and it was successfully retrieved.</returns>
        public static bool TryGetActiveWpfTextView(this IServiceProvider serviceProvider, out IWpfTextView textView)
        {            
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;

            IVsTextView vsView = null;
            int result = textManager?.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out vsView) ?? -1;

            if (result == 0)
            {
                var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                var adapter = (IVsEditorAdaptersFactoryService)componentModel.GetService<IVsEditorAdaptersFactoryService>();
                textView = adapter.GetWpfTextView(vsView);
                return true;
            }

            textView = null;
            return false;
        }
    }
}
