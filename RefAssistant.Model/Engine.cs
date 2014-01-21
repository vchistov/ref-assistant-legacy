using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lardite.RefAssistant.Model.Processing;
using Lardite.RefAssistant.Model.Processing.Data;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ObjectModel;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model
{
    public sealed class Engine
    {
        public Task<IEnumerable<VsProjectReference>> FindUnusedReferences(IVsProject project)
        {
            return Task<IEnumerable<VsProjectReference>>.Factory.StartNew(() => DoWorkOld(project));
        }

        private IEnumerable<VsProjectReference> DoWork(IVsProject project)
        {
            var configurator = ServiceConfigurator.GetConfigurator(
                new ServiceConfiguratorOptions
                {
                    ProjectOutputDir = Path.GetDirectoryName(project.OutputAssemblyPath),
                    ProjectReferenceFiles = project.References.Select(@ref => @ref.Location).ToArray()
                });

            var innerProject = new Project(project, configurator);
            var projectAgent = ProjectAgentFactory.Create(innerProject, configurator);

            var unusedReferences = projectAgent.DoAnalysis();

            return project.References.Join(
                unusedReferences,
                r => r.Name,
                ur => ur.Name,
                (r, ur) => r).ToList();
        }

        [Obsolete("Use DoWork instead of.")]
        private IEnumerable<VsProjectReference> DoWorkOld(IVsProject project)
        {
            var projectInfo = new ProjectInfo
                {
                    Name = project.Name,
                    Type = project.KindGuid,
                    AssemblyPath = project.OutputAssemblyPath,
                    ConfigurationName = "NEW",
                    PlatformName = "RA.2.0",
                    References = project.References.Select(r => new ProjectReference
                        {
                            Name = r.Name,
                            Identity = r.Name,
                            Culture = r.Culture,
                            Location = r.Location,
                            Version = r.Version,
                            PublicKeyToken = r.PublicKeyToken
                        }).ToList()
                };

            using (ReferenceInspector referenceResolver = new ReferenceInspector())
            {
                var result = new InspectResult(referenceResolver.Inspect(new ProjectEvaluator(projectInfo)));

                if (result != null && result.Result != null && !result.Result.IsSuccess)
                {
                    throw new ApplicationException(result.Result.Exception.Message, result.Result.Exception);
                }

                if (result == null || !result.HasUnusedReferences)
                {
                    return Enumerable.Empty<VsProjectReference>();
                }

                return project.References.Join(
                    result.Result.UnusedReferences,
                    r => r.Name,
                    ur => ur.Name,
                    (r, ur) => r).ToList();
            }
        }        
    }
}
