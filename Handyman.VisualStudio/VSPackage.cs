//------------------------------------------------------------------------------
// <copyright file="VSPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using CommerceRuntimeHandyman.Editor.Commands;
using CommerceRuntimeHandyman.Settings;
using EnvDTE;
using Handyman.Settings;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CommerceRuntimeHandyman
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(OptionPageGrid), OptionPageGrid.Category, OptionPageGrid.Name, 0, 0, true)]
    [ProvideSolutionProperties(VSPackage.SolutionSettingsKey)]
    public sealed class VSPackage : Package, IVsPersistSolutionProps // most of the persistent solution props implementation was based on https://github.com/pvginkel/VisualGit/blob/master/VisualGit.Package/VisualGitPackage.SolutionProperties.cs
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4a564a1f-0b49-48ea-abc3-694fa6e01f84";

        internal const string SolutionSettingsKey = "CommerceRuntimeHandymanSettings";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.            
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            ((OptionPageGrid)this.GetDialogPage(typeof(OptionPageGrid))).UpdateSettings();

            var mcs = (OleMenuCommandService)GetService(typeof(IMenuCommandService));
            var commandId = new CommandID(typeof(Commands).GUID, (int)Commands.SetRequestProjectCommand);
            mcs.AddCommand(new MenuCommand(delegate {

                IntPtr hierarchyPointer, selectionContainerPointer;
                Object selectedObject = null;
                IVsMultiItemSelect multiItemSelect;
                uint projectItemId;

                IVsMonitorSelection monitorSelection =
                        (IVsMonitorSelection)Package.GetGlobalService(
                        typeof(SVsShellMonitorSelection));

                monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                     out projectItemId,
                                                     out multiItemSelect,
                                                     out selectionContainerPointer);

                IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                                                     hierarchyPointer,
                                                     typeof(IVsHierarchy)) as IVsHierarchy;

                if (selectedHierarchy != null)
                {
                    ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                      projectItemId,
                                                      (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                      out selectedObject));
                }

                Project selectedProject = selectedObject as Project;

                var factory = this.GetWorkspaceFactory();
                WorkspaceSettings settings = (WorkspaceSettings)factory.Manager.Settings;
                settings.DefaultRequestProjectName = selectedProject.Name;

                factory.Manager.Settings = settings;

            }, commandId));
            GoToRequestHandlerImplementation.Initialize(this);
        }

        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            return VSConstants.S_OK;
        }

        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            return VSConstants.S_OK;
        }

        public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        {
            return VSConstants.S_OK;
        }

        public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        {
            return VSConstants.S_OK;
        }

        public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
        {
            // This function is called by the IDE to determine if something needs to be saved in the solution.
            // If the package returns that it has dirty properties, the shell will callback on SaveSolutionProps

            // only do work at the solution level
            if (pHierarchy == null)
            {
                var factory = this.GetWorkspaceFactory();
                VSQUERYSAVESLNPROPS result = VSQUERYSAVESLNPROPS.QSP_HasNoProps;

                if (factory != null)
                {
                    if (factory.Manager.SettingsHaveChanged)
                    {
                        result = VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
                    }
                    else
                    {
                        result = VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps;
                    }
                }

                pqsspSave[0] = result;
            }

            return VSConstants.S_OK;
        }

        public int SaveSolutionProps(IVsHierarchy pHierarchy, IVsSolutionPersistence pPersistence)
        {
            // This function gets called by the shell after QuerySaveSolutionProps returned QSP_HasDirtyProps
            // only do work at the solution level
            if (pHierarchy == null)
            {
                var factory = this.GetWorkspaceFactory();

                if (factory != null && factory.Manager.SettingsHaveChanged)
                {
                    pPersistence.SavePackageSolutionProps(1 /* true */, null, this, SolutionSettingsKey);
                    factory.Manager.SettingsHaveChanged = false;
                }
            }

            return VSConstants.S_OK;
        }

        public int WriteSolutionProps(IVsHierarchy pHierarchy, string pszKey, IPropertyBag pPropBag)
        {
            // This method is called from the VS implementation after a request from SaveSolutionProps

            if (pHierarchy != null)
                return VSConstants.S_OK; // Not send by our code!
            else if (pPropBag == null)
                return VSConstants.E_POINTER;
            else if (pszKey != SolutionSettingsKey)
                return VSConstants.E_INVALIDARG; // not our settings

            var factory = this.GetWorkspaceFactory();

            if (factory == null)
            {
                return VSConstants.E_FAIL;
            }

            // serialize properties
            string properties = JsonConvert.SerializeObject(factory.Manager.Settings);

            using (PropertyBag bag = new PropertyBag(pPropBag))
            {
                bag.SetQuoted(SolutionSettingsKey, properties);
            }

            return VSConstants.S_OK;
        }

        public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
        {
            if (pHierarchy != null)
                return VSConstants.S_OK; // Not send by our code!
            else if (pPropBag == null)
                return VSConstants.E_POINTER;
            else if (pszKey != SolutionSettingsKey)
                return VSConstants.E_INVALIDARG; // not our settings

            var factory = this.GetWorkspaceFactory();

            if (factory == null)
            {
                return VSConstants.E_FAIL;
            }

            string properties;

            using (PropertyBag bag = new PropertyBag(pPropBag))
            {
                bag.TryGetQuoted(SolutionSettingsKey, out properties);
            }
    
            if (!string.IsNullOrWhiteSpace(properties))
            {
                var settings = JsonConvert.DeserializeObject<WorkspaceSettings>(properties);
                factory.InitializeWorkspaceSettings(settings);
            }

            return VSConstants.S_OK;
        }

        public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        {
            var factory = this.GetWorkspaceFactory();

            if (factory != null)
            {
                // mark settings as changed we will try to save them again
                factory.Manager.SettingsHaveChanged = true;
            }

            return VSConstants.S_OK;
        }

        private WorkspaceManagerFactory GetWorkspaceFactory()
        {
            var provider = (IComponentModel)this.GetService(typeof(SComponentModel));

            if (provider != null)
            {
                return provider.GetService<WorkspaceManagerFactory>();
            }

            return null;
        }
    }
}
