namespace Lardite.RefAssistant.ReflectionServices
{
    public interface IServiceConfigurator
    {
        IAssemblyService AssemblyService { get; }

        ITypeService TypeService { get; }

        ICustomAttributeService CustomAttributeService { get; }
    }
}
