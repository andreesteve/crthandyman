using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CommerceRuntimeHandyman
{
    class VsHostService : Handyman.IHostService
    {
        private readonly DTE dte;

        public VsHostService(DTE dte)
        {
            this.dte = dte;
        }

        public string GetDefaultNamespace(string projectName)
        {
            var project = this.dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projectName);
            if (project == null)
            {
                // if not found, try to see if there are folders/subprojects
                foreach (var p in this.dte.Solution.Projects.Cast<Project>())
                {
                    foreach (var pItem in p.ProjectItems)
                    {
                        var ppItem = (ProjectItem)pItem;
                        if (ppItem.SubProject != null && ppItem.SubProject.Name == projectName)
                        {
                            project = ppItem.SubProject;
                            break;
                        }
                    }
                }

                if (project == null)
                {
                    throw new ArgumentException($"Project '{projectName}' was not found.");
                }                
            }

            return (string)project.Properties.Item("DefaultNamespace")?.Value;
        }
    }
}
