using System;
using System.Linq;
using Handyman.Generators;
using Handyman.Settings;
using Handyman.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Handyman
{
    public class WorkspaceManager
    {
        private readonly Workspace workspace;
        private readonly IHostService hostServices;

        private IWorkspaceSettings settings;

        public WorkspaceManager(Workspace workspace, IWorkspaceSettings settings, IHostService hostServices)
        {
            this.workspace = workspace;
            this.Settings = settings;
            this.hostServices = hostServices;
            this.SettingsHaveChanged = false;
        }

        public IWorkspaceSettings Settings
        {
            get
            {
                return this.settings;
            }

            set
            {
                this.settings = value ?? throw new ArgumentNullException(nameof(this.settings));
                this.SettingsHaveChanged = true;
            }
        }

        public bool SettingsHaveChanged
        {
            get;
            set;
        }

        public bool CreateOrUpdateRequestHandlerDefinition(RequestHandlerMethodDefinition requestHandler)
        {
            var generator = new MemberedTypeGenerator();
            string requestProjectName = this.settings.DefaultRequestProjectName;

            Project project = this.GetProjectByNameOrThrow(requestProjectName);

            var defaultNamespace = this.hostServices.GetDefaultNamespace(project.Name);
            if (!string.IsNullOrWhiteSpace(defaultNamespace))
            {
                requestHandler.SetContainingNamespace(defaultNamespace);
            }

            string requestCode = generator.GenerateSyntax(requestHandler.RequestType);
            var document = this.CreateOrUpdateDocument(project, requestHandler.RequestType.Name, requestCode);

            project = document.Project;

            if (!requestHandler.ResponseType.IsVoidResponse)
            {
                string responseCode = generator.GenerateSyntax(requestHandler.ResponseType);
                document = this.CreateOrUpdateDocument(project, requestHandler.ResponseType.Name, responseCode);
                project = document.Project;
            }

            return workspace.TryApplyChanges(project.Solution);
        }

        private Document CreateOrUpdateDocument(Project project, string name, string documentContent)
        {
            name += ".cs";

            var document = project.Documents.FirstOrDefault(d => d.Name == name);
            var text = SourceText.From(documentContent);

            if (document == null)
            {
                document = project.AddDocument(name, text);
            }
            else
            {
                document = document.WithText(text);
            }

            return document;
        }

        private Project GetProjectByNameOrThrow(string projectName)
        {
            Project project = null;

            if (string.IsNullOrWhiteSpace(projectName))
            {
                project = this.workspace.CurrentSolution.Projects.First();
            }
            else
            {
                project = this.workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == projectName);
            }

            if (project == null)
            {
                throw new ArgumentException($"Couldn't find project named '{ projectName ?? string.Empty }'.");
            }

            return project;
        }

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