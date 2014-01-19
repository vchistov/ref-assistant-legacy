using System.IO;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.VsProxy.Projects.References;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    [ProjectKind(ProjectKinds.FSharp, "f2a71f9b-5d33-465a-a702-920d77279786")]
    internal sealed class FSharpProject : VsBaseProject
    {
        public FSharpProject(Project project)
            : base(project, (vsp) => new OAAssemblyReferenceController(vsp))
        {
        }

        public override string OutputAssemblyPath
        {
            get
            {
                string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string fullPath = Project.Properties.Item("FullPath").Value.ToString();
                string targetName = Project.Properties.Item("OutputFileName").Value.ToString();

                return Path.Combine(fullPath, outputPath, targetName);
            }
        }
    }
}
