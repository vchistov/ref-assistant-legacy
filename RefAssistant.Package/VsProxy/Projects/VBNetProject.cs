using System.IO;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.VsProxy.Projects.References;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    [ProjectKind(VsProjectKinds.VBNet, "f184b08f-c81c-45f6-a57f-5abd9991f28f")]
    internal sealed class VBNetProject : VsBaseProject
    {
        public VBNetProject(Project project)
            : base(project, (vsp) => new Reference3Controller(vsp))
        {
        }

        public override string OutputAssemblyPath
        {
            get
            {
                string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string buildPath = Project.Properties.Item("LocalPath").Value.ToString();
                string targetName = Project.Properties.Item("OutputFileName").Value.ToString();

                return Path.Combine(buildPath, outputPath, targetName);
            }
        }
    }
}
