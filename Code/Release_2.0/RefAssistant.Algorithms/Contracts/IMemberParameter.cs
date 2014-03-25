namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMemberParameter : ICustomAttributeProvider
    {
        IMemberType ParameterType { get; }
    }
}
