using System;
using System.Diagnostics.Contracts;
using System.Linq;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal static class VsProjectMapper
    {
        public static IVsProjectExtended Map(Project project)
        {
            Guid kind = Guid.Parse(project.Kind);

            var interfaceType = typeof(IVsProjectExtended);
            var wrapperTypes = typeof(VsProjectMapper)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

            foreach (var type in wrapperTypes)
            {
                var projectKind = type.GetCustomAttribute<ProjectKindAttribute>();
                Contract.Assert(projectKind != null);
                if (projectKind.Guid == kind)
                {
                    return (IVsProjectExtended)Activator.CreateInstance(type, project);
                }
            }

            ThrowUtils.NotSupported(string.Format(Resources.VsProjectMapper_NotSupported, kind));
            return null;
        }
    }
}
