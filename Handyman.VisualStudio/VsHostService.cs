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
                throw new ArgumentException($"Project '{projectName}' was not found.");
            }

            return (string)project.Properties.Item("DefaultNamespace")?.Value;
        }
    }
}
