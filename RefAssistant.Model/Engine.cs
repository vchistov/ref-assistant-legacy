using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lardite.RefAssistant.Model.Contracts;
using Lardite.RefAssistant.ObjectModel;
//using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model
{
    public sealed class Engine
    {
        public Task<IEnumerable<VsProjectReference>> FindUnusedReferences(IVsProject project)
        {
            //var configurator = ServiceConfigurator.GetConfigurator(project);
            //var assemblyService = configurator.AssemblyService;

            //var primaryAssembly = assemblyService.GetProjectAssembly();
            //var manifestAssemblies = assemblyService.GetManifestAssemblies(primaryAssembly.Id).ToList();

            //foreach (var id in manifestAssemblies)
            //{
            //    var asm = assemblyService.GetAssembly(id);
            //    var innerAsm = assemblyService.GetManifestAssemblies(asm.Id).ToList();
            //}

            return Task<IEnumerable<VsProjectReference>>.Factory.StartNew(() => DoWork(project));
        }

        private IEnumerable<VsProjectReference> DoWork(IVsProject project)
        {
            var projectInfo = new ProjectInfo
                {
                    Name = project.Name,
                    Type = project.Kind,
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
