﻿using System;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Model.Contracts;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;

namespace Lardite.RefAssistant.ReflectionServices
{
    public sealed class ServiceConfigurator : IServiceConfigurator
    {
        private readonly IVsProject _project;
        private readonly Lazy<IAssemblyService> _assemblyService;
        private readonly Lazy<ICustomAttributeService> _customAttributeService;
        private readonly Lazy<IAssemblyContainer> _assemblyContainer;

        private ServiceConfigurator(IVsProject project)
        {
            ThrowUtils.ArgumentNull(() => project);

            _project = project;
            _assemblyContainer = new Lazy<IAssemblyContainer>(CreateAssemblyContainer);
            _assemblyService = new Lazy<IAssemblyService>(InitAssemblyService);
            _customAttributeService = new Lazy<ICustomAttributeService>(InitCustomAttributeService);            
        }

        public static IServiceConfigurator GetConfigurator(IVsProject project)
        {
            return new ServiceConfigurator(project);
        }

        IAssemblyService IServiceConfigurator.AssemblyService
        {
            get { return _assemblyService.Value; }
        }

        ICustomAttributeService IServiceConfigurator.CustomAttributeService
        {
            get { return _customAttributeService.Value; }
        }

        private IAssemblyContainer AssemblyContainer
        {
            get { return _assemblyContainer.Value; }
        }

        private IAssemblyService InitAssemblyService()
        {
            Contract.Assert(_project != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(_project.OutputAssemblyPath));

            var projectAssemblyIdProvider = new ProjectAssemblyIdProvider(_project.OutputAssemblyPath);

            return new AssemblyService(projectAssemblyIdProvider, AssemblyContainer);
        }

        private ICustomAttributeService InitCustomAttributeService()
        {
            return new CustomAttributeService(AssemblyContainer);
        }

        private IAssemblyContainer CreateAssemblyContainer()
        {
            Contract.Ensures(Contract.Result<IAssemblyContainer>() != null);
            Contract.Assert(_project != null);

            var resolver = new ProjectSpecificAssemblyResolver(_project);
            return new AssemblyContainer(resolver);
        }
    }
}
