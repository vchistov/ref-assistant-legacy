using System;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal static class VsProjectMapper
    {
        public static IVsProjectExtended Map(Project project)
        {
            Guid kind = Guid.Parse(project.Kind);

            if (kind == ProjectKinds.CSharp)
            {
                return new CSharpProject(project);
            }

            throw new NotSupportedException(string.Format("The project kind {0} is not supported.", kind));
        }
    }
}
