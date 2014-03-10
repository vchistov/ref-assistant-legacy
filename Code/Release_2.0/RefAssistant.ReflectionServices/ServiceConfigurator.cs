using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;

namespace Lardite.RefAssistant.ReflectionServices
{
    public sealed class ServiceConfigurator : IServiceConfigurator
    {
        private readonly ServiceConfiguratorOptions _options;
        private readonly Lazy<IAssemblyService> _assemblyService;
        private readonly Lazy<ITypeService> _typeService;
        private readonly Lazy<ICustomAttributeService> _customAttributeService;
        private readonly Lazy<IAssemblyContainer> _assemblyContainer;

        private ServiceConfigurator(ServiceConfiguratorOptions options)
        {
            ThrowUtils.ArgumentNull(() => options);

            _options = options;
            _assemblyContainer = new Lazy<IAssemblyContainer>(CreateAssemblyContainer);
            _assemblyService = new Lazy<IAssemblyService>(InitAssemblyService);
            _typeService = new Lazy<ITypeService>(InitTypeService);
            _customAttributeService = new Lazy<ICustomAttributeService>(InitCustomAttributeService);            
        }

        public static IServiceConfigurator GetConfigurator(ServiceConfiguratorOptions options)
        {
            return new ServiceConfigurator(options);
        }

        IAssemblyService IServiceConfigurator.AssemblyService
        {
            get { return _assemblyService.Value; }
        }

        ITypeService IServiceConfigurator.TypeService
        {
            get { return _typeService.Value; }
        }

        ICustomAttributeService IServiceConfigurator.CustomAttributeService
        {
            get { return _customAttributeService.Value; }
        }

        #region Helpers
        
        private IAssemblyContainer AssemblyContainer
        {
            get { return _assemblyContainer.Value; }
        }

        private IAssemblyService InitAssemblyService()
        {
            var codeFilesGrubber = new EmbeddedCodeFilesGrubber(
                this.AssemblyContainer, 
                _options.ProjectReferenceFiles);

            return new AssemblyService(this.AssemblyContainer, codeFilesGrubber);
        }

        private ITypeService InitTypeService()
        {
            var importTypeResolver = new ImportTypeResolver(this.AssemblyContainer, _options.ProjectReferenceFiles);
            return new TypeService(this.AssemblyContainer, importTypeResolver);
        }

        private ICustomAttributeService InitCustomAttributeService()
        {
            return new CustomAttributeService(this.AssemblyContainer);
        }

        private IAssemblyContainer CreateAssemblyContainer()
        {
            Contract.Ensures(Contract.Result<IAssemblyContainer>() != null);
            Contract.Assert(_options != null);

            var resolver = new ProjectSpecificAssemblyResolver(
                _options.ProjectReferenceFiles ?? Enumerable.Empty<string>(),
                _options.ProjectOutputDir);

            return new AssemblyContainer(resolver);
        }

        #endregion
    }
}
