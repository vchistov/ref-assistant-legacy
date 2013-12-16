using System;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Model.Contracts;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Loaders;

namespace Lardite.RefAssistant.ReflectionServices
{
    public sealed class ServiceConfigurator : IServiceConfigurator
    {
        private readonly IVsProject _project;
        private readonly Lazy<IAssemblyService> _assemblyService;

        private ServiceConfigurator(IVsProject project)
        {
            ThrowUtils.ArgumentNull(() => project);

            _project = project;
            _assemblyService = new Lazy<IAssemblyService>(GetAssemblyService);
        }

        public static IServiceConfigurator GetConfigurator(IVsProject project)
        {
            return new ServiceConfigurator(project);
        }

        IAssemblyService IServiceConfigurator.AssemblyService
        {
            get { return _assemblyService.Value; }
        }

        private IAssemblyService GetAssemblyService()
        {
            Contract.Assert(_project != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(_project.OutputAssemblyPath));

            var projectAssemblyIdProvider = new ProjectAssemblyIdProvider(_project.OutputAssemblyPath);

            var resolver = new ProjectSpecificAssemblyResolver(_project);
            var loader = new AssemblyLoader(resolver);

            var container = new AssemblyContainer(loader);

            return new AssemblyService(projectAssemblyIdProvider, container);
        }
    }
}
