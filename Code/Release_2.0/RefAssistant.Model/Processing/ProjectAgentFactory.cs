using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Model.Processing.Data;
using Lardite.RefAssistant.Model.Processing.ProjectAgents;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing
{
    internal sealed class ProjectAgentFactory
    {
        public static ProjectAgentBase Create(Project project, IServiceConfigurator services)
        {
            foreach (var type in LookupServiceAgents())
            {
                var projectKind = type.GetCustomAttribute<ProjectKindAttribute>();
                Contract.Assert(projectKind != null);
                if (projectKind.Kind == project.Kind)
                {
                    return (ProjectAgentBase)Activator.CreateInstance(type, project, services);
                }
            }

            ThrowUtils.NotSupported(string.Format("The project kind '{0}' is not supported.", project.Kind));
            return null;
        }

        private static IEnumerable<Type> LookupServiceAgents()
        {
            var baseType = typeof(ProjectAgentBase);
            return typeof(ProjectAgentFactory)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && baseType.IsAssignableFrom(t));
        }
    }
}
