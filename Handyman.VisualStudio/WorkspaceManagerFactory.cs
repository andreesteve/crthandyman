using System;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using Handyman;
using Handyman.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;

namespace CommerceRuntimeHandyman
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorkspaceManagerFactory
    {
        [ImportingConstructor]
        public WorkspaceManagerFactory([Import(typeof(SVsServiceProvider), AllowDefault = true)] IServiceProvider vsServiceProvider, [Import(typeof(VisualStudioWorkspace), AllowDefault = true)] Workspace vsWorkspace)
        {
            DTE dte = (DTE)vsServiceProvider.GetService(typeof(DTE));
            VsHostService vsServices = new VsHostService(dte);

            // start with empty settings
            this.Manager = new WorkspaceManager(vsWorkspace, new WorkspaceSettings(), vsServices);
        }

        public void InitializeWorkspaceSettings (WorkspaceSettings settings)
        {
            this.Manager.Settings = settings;

            // this is initialization
            this.Manager.SettingsHaveChanged = false;
        }

        public WorkspaceManager Manager { get; private set; }

        ////    //this.dte = (DTE2)ServiceProvider.GetService(typeof(DTE2));
        ////    private EnvDTE.Project ActiveProject
        ////    {
        ////        get
        ////        {
        ////            EnvDTE.Project activeProject = null;

        ////            Array activeSolutionProjects = this.dte.ActiveSolutionProjects as Array;
        ////            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
        ////            {
        ////                activeProject = activeSolutionProjects.GetValue(0) as EnvDTE.Project;
        ////            }

        ////            return activeProject;
        ////        }
        ////    }

        ////    /*
        ////     public static IList<Project> Projects()
        ////{
        ////    Projects projects = GetActiveIDE().Solution.Projects;
        ////    List<Project> list = new List<Project>();
        ////    var item = projects.GetEnumerator();
        ////    while (item.MoveNext())
        ////    {
        ////        var project = item.Current as Project;
        ////        if (project == null)
        ////        {
        ////            continue;
        ////        }

        ////        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        ////        {
        ////            list.AddRange(GetSolutionFolderProjects(project));
        ////        }
        ////        else
        ////        {
        ////            list.Add(project);
        ////        }
        ////    }

        ////    return list;
        ////}

        ////private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        ////{
        ////    List<Project> list = new List<Project>();
        ////    for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
        ////    {
        ////        var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
        ////        if (subProject == null)
        ////        {
        ////            continue;
        ////        }

        ////        // If this is another solution folder, do a recursive call, otherwise add
        ////        if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        ////        {
        ////           list.AddRange(GetSolutionFolderProjects(subProject));
        ////        }
        ////       else
        ////       {
        ////           list.Add(subProject);
        ////       }
        ////    }
        ////    return list;
        ////}
        ////     */
    }
}