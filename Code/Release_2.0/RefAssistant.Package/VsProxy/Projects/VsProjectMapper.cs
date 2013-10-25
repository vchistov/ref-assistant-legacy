using System;
using EnvDTE;

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
            else if (kind == ProjectKinds.FSharp)
            {
                return new FSharpProject(project);
            }
            else if (kind == ProjectKinds.VisualCppCli)
            {
                return new VisualCppCliProject(project);
            }
            else if (kind == ProjectKinds.VisualBasic)
            {
                return new VBNetProject(project);
            }

            throw new NotSupportedException(string.Format(Resources.VsProjectMapper_NotSupported, kind));
        }
    }
}
